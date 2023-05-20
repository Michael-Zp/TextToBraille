using CommandLine;
using TextGenerator.Alphabet;
using Shared;

namespace TextGenerator
{
    internal class TextGenerator
    {
        static int Main(string[] args)
        {
            var clOptions = Parser.Default.ParseArguments<CLOptions>(args).Value;

            if (clOptions == null)
            {
                return 1;
            }

            // Should be the right settings for my printer
            var typesettingOptions = new TypesettingOptions(5.5f, 9.5f, 3, 5);
            var printPlateProps = PrintPlateProperties.Ender3V2(typesettingOptions);

            Alphabet<SixPointLetter> alphabet = null;
            using (StreamReader sr = new StreamReader(new FileStream($"../../../../Alphabets/{clOptions.Language}.json", FileMode.Open)))
            {
                alphabet = Alphabet<SixPointLetter>.FromJson(sr.ReadToEnd());
            }

            if (alphabet == null)
            {
                throw new Exception("Could not read alphabet");
            }

            List<List<Letter>> textWithLineBreaks = SplitTextIntoLines(clOptions.InputText.ToLower(), printPlateProps, alphabet);

            for (int pageNumber = 1; textWithLineBreaks.Count > 0; ++pageNumber)
            {
                // Print each page seperately
                var page = textWithLineBreaks.Take(printPlateProps.MaxRowsPerPage).ToList();
                textWithLineBreaks.RemoveRange(0, page.Count);

                var filename = $"Page{pageNumber}.stl";

                if (!string.IsNullOrEmpty(clOptions.NamePrefix))
                {
                    filename = $"{clOptions.NamePrefix}_{filename}";
                }

                StlGenerator.StlGenerator.GenerateStl(
                    $"Page{pageNumber}",
                    Path.Join(clOptions.OutputDirectory, filename),
                    page,
                    typesettingOptions,
                    clOptions.TextOutput ? OutputFormat.Text : OutputFormat.Binary,
                    new PrintOptions(clOptions.PrintWithBuildplate));
            }

            return 0;
        }

        private static List<List<Letter>> SplitTextIntoLines(ReadOnlySpan<char> inputText, PrintPlateProperties printPlateProps, Alphabet<SixPointLetter> alphabet)
        {
            var textWithLineBreaks = new List<List<Letter>>();

            var currentLine = new List<Letter>();
            for (int start = 0; start < inputText.Length;)
            {
                ReadOnlySpan<char> currentSlice = inputText.Slice(start, inputText.Length - start);
                int end = currentSlice.IndexOf(" ");

                if (end == -1)
                {
                    end = currentSlice.Length;
                }

                ReadOnlySpan<char> currentWord = inputText.Slice(start, end);
                start += end + 1;

                var braillePointsForWord = WordToBraillePoints(currentWord, alphabet);

                if (braillePointsForWord.Count + currentLine.Count <= printPlateProps.MaxLineLength)
                {
                    // Found a word, that fits in the current line
                    currentLine.AddRange(braillePointsForWord);

                    // Only add a whitespace if there is space left
                    // No need to break to the next line. The next word will do that anyway
                    if (currentLine.Count + 1 < printPlateProps.MaxLineLength)
                    {
                        currentLine.Add(new SixPointLetter());
                    }
                }
                else
                {
                    if (currentLine.Count > 0)
                    {
                        textWithLineBreaks.Add(currentLine);
                        currentLine = new List<Letter>();
                    }

                    while (braillePointsForWord.Count > printPlateProps.MaxLineLength)
                    {
                        currentLine.AddRange(braillePointsForWord.Take(printPlateProps.MaxLineLength - 1).ToList());
                        currentLine.AddRange(WordToBraillePoints("-", alphabet));
                        braillePointsForWord.RemoveRange(0, printPlateProps.MaxLineLength - 1);
                        textWithLineBreaks.Add(currentLine);
                        currentLine = new List<Letter>();
                    }

                    currentLine.AddRange(braillePointsForWord);

                    if (currentLine.Count + 1 < printPlateProps.MaxLineLength)
                    {
                        currentLine.Add(new SixPointLetter());
                    }
                }
            }

            if (currentLine.Count > 0)
            {
                // Add the last filled line, that never reached the line break.
                textWithLineBreaks.Add(currentLine);
            }

            return textWithLineBreaks;
        }

        private static List<Letter> WordToBraillePoints(ReadOnlySpan<char> inputWord, Alphabet<SixPointLetter> alphabet)
        {
            List<Letter> output = new List<Letter>();

            for (int i = 0; i < inputWord.Length; ++i)
            {
                // Start looking for the largest chunks first and then trickle down to lower lengths
                for (int k = alphabet.MaxSymbolLength; k > 0; --k)
                {
                    if (inputWord.Length - i >= k)
                    {
                        var symbol = inputWord.Slice(i, k);

                        if (alphabet.TryGetSymbol(symbol, out SixPointLetter letter))
                        {
                            // Add number indicator
                            if (letter.IsNumber)
                            {
                                output.Add(alphabet.NumberIndicator);
                            }

                            output.Add(letter);
                            i += k; // We skip the found letters (can be multiple like 'st')
                            k = alphabet.MaxSymbolLength; // Reset the inner loop
                        }
                    }
                }

                // As we reset the loop in the inner loop, we should never reach this here, but if we do, just print a warning
                if (i != inputWord.Length)
                {
                    Console.WriteLine($"Found unrecognized symbol in word {inputWord} at position {i}");
                }
            }

            return output;
        }
    }
}