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
            List<List<string>> textWithLineBreaks = SplitTextIntoLines(clOptions.InputText, printPlateProps);

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

        private static List<List<string>> SplitTextIntoLines(string inputText, PrintPlateProperties printPlateProps)
        {
            var textWithLineBreaks = new List<List<string>>();

            var currentLine = new List<string>();
            foreach (var word in inputText.Split(" "))
            {
                var braillePointsForWord = WordToBraillePoints(word);

                if (braillePointsForWord.Count + currentLine.Count <= printPlateProps.MaxLineLength)
                {
                    // Found a word, that fits in the current line
                    currentLine.AddRange(braillePointsForWord);

                    // Only add a whitespace if there is space left
                    // No need to break to the next line. The next word will do that anyway
                    if (currentLine.Count + 1 < printPlateProps.MaxLineLength)
                    {
                        currentLine.Add("");
                    }
                }
                else
                {
                    // If the word was super long, the current line will be empty, and thus we should not print it
                    if (currentLine.Count > 0)
                    {
                        textWithLineBreaks.Add(currentLine);
                        currentLine = new List<string>();
                    }

                    while (braillePointsForWord.Count > printPlateProps.MaxLineLength)
                    {
                        currentLine.AddRange(braillePointsForWord.Take(printPlateProps.MaxLineLength - 1).ToList());
                        currentLine.AddRange(WordToBraillePoints("-"));
                        braillePointsForWord.RemoveRange(0, printPlateProps.MaxLineLength - 1);
                        textWithLineBreaks.Add(currentLine);
                        currentLine = new List<string>();
                    }

                    currentLine.AddRange(braillePointsForWord);

                    if (currentLine.Count + 1 < printPlateProps.MaxLineLength)
                    {
                        currentLine.Add("");
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

        private static List<string> WordToBraillePoints(string inputWord)
        {
            Alphabet<SixPointLetter> alphabet = new Alphabet<SixPointLetter>("3456");

            // Group 1
            alphabet.AddLetter("A".ToLower(), 1);
            alphabet.AddLetter("B".ToLower(), 12);
            alphabet.AddLetter("C".ToLower(), 14);
            alphabet.AddLetter("D".ToLower(), 145);
            alphabet.AddLetter("E".ToLower(), 15);
            alphabet.AddLetter("F".ToLower(), 124);
            alphabet.AddLetter("G".ToLower(), 1245);
            alphabet.AddLetter("H".ToLower(), 125);
            alphabet.AddLetter("I".ToLower(), 24);
            alphabet.AddLetter("J".ToLower(), 245);

            // Group 2 (Group 1 with prefix of '3')
            alphabet.AddLetter("K".ToLower(), 31);
            alphabet.AddLetter("L".ToLower(), 312);
            alphabet.AddLetter("M".ToLower(), 314);
            alphabet.AddLetter("N".ToLower(), 3145);
            alphabet.AddLetter("O".ToLower(), 315);
            alphabet.AddLetter("P".ToLower(), 3124);
            alphabet.AddLetter("Q".ToLower(), 31245);
            alphabet.AddLetter("R".ToLower(), 3125);
            alphabet.AddLetter("S".ToLower(), 324);
            alphabet.AddLetter("T".ToLower(), 3245);

            // Group 3 (Group 1 with prefix of '36')
            alphabet.AddLetter("U".ToLower(), 361);
            alphabet.AddLetter("V".ToLower(), 3612);
            alphabet.AddLetter("X".ToLower(), 3614);
            alphabet.AddLetter("Y".ToLower(), 36145);
            alphabet.AddLetter("ß".ToLower(), 3615);
            alphabet.AddLetter("ST".ToLower(), 36124);

            // Group 4 (Group 1 with prefix of '6')
            alphabet.AddLetter("AU".ToLower(), 61);
            alphabet.AddLetter("EU".ToLower(), 612);
            alphabet.AddLetter("EI".ToLower(), 614);
            alphabet.AddLetter("CH".ToLower(), 6145);
            alphabet.AddLetter("SCH".ToLower(), 615);
            alphabet.AddLetter("Ü".ToLower(), 6124);
            alphabet.AddLetter("Ö".ToLower(), 61245);
            alphabet.AddLetter("W".ToLower(), 6125);

            // Combinations that fall out of line
            alphabet.AddLetter("ÄU".ToLower(), 34);
            alphabet.AddLetter("Ä".ToLower(), 345);
            alphabet.AddLetter("IE".ToLower(), 346);
            alphabet.AddLetter(".".ToLower(), 3);
            alphabet.AddLetter("-".ToLower(), 36);
            alphabet.AddLetter(",".ToLower(), 2);
            alphabet.AddLetter(";".ToLower(), 23);
            alphabet.AddLetter(":".ToLower(), 25);
            alphabet.AddLetter("?".ToLower(), 26);
            alphabet.AddLetter("!".ToLower(), 235);
            
            // Numbers (are indicated by the NumberIndicator)
            alphabet.AddLetter("1".ToLower(), 1);
            alphabet.AddLetter("2".ToLower(), 12);
            alphabet.AddLetter("3".ToLower(), 14);
            alphabet.AddLetter("4".ToLower(), 145);
            alphabet.AddLetter("5".ToLower(), 15);
            alphabet.AddLetter("6".ToLower(), 124);
            alphabet.AddLetter("7".ToLower(), 1245);
            alphabet.AddLetter("8".ToLower(), 125);
            alphabet.AddLetter("9".ToLower(), 24);
            alphabet.AddLetter("0".ToLower(), 245);


            List<string> output = new List<string>();

            for (int i = 0; i < inputWord.Length; ++i)
            {
                // Start looking for the largest chunks first and then trickle down to lower lengths
                for (int k = alphabet.MaxSymbolLength; k > 0; --k)
                {
                    if (inputWord.Length - i >= k)
                    {
                        var key = inputWord.Substring(i, k).ToLower();
                        if (brailleAlphabet.ContainsKey(key))
                        {
                            // Add number indicator on a number
                            if (inputWord[0] >= '0' && inputWord[0] <= '9')
                            {
                                output.Add(NumberIndicator);
                            }

                            output.Add(brailleAlphabet[key]);
                            i += k; // We skip the found letters (can be multiple like 'st')
                            k = maxKeyLength; // Reset the inner loop
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