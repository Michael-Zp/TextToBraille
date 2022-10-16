using CommandLine;

namespace TextGenerator
{
    internal class TextGenerator
    {
        public class CLOptions
        {
            [Option('o', "OutputPath", Default = ".")]
            public string OutputDirectory { get; set; } = ".";

            [Option('n', "NamePrefix", Default = ".")]
            public string NamePrefix { get; set; } = string.Empty;

            [Option('i', "InputText", Required = true)]
            public string InputText { get; set; } = "LoremIpsum";

            [Option('t', "TextOutput")]
            public bool TextOutput { get; set; } = false;
        }


        static int Main(string[] args)
        {
            var clOptions = Parser.Default.ParseArguments<CLOptions>(args).Value;

            if (clOptions == null)
            {
                return 1;
            }

            // Should be the right settings for my printer
            var typesettingOptions = new StlGenerator.StlGenerator.TypesettingOptions(6, 9, 2, 4);
            var maxLineLength = (int)Math.Floor((200 - typesettingOptions.SpaceBetweenLetters) / (typesettingOptions.LetterWidth + typesettingOptions.SpaceBetweenLetters)) - 1;
            var maxRowsPerPage = (int)Math.Floor((210 - typesettingOptions.SpaceBetweenLines) / (typesettingOptions.LetterHeight + typesettingOptions.SpaceBetweenLines)) - 1;
            var textWithLineBreaks = new List<List<string>>();

            var currentLine = new List<string>();
            foreach (var word in clOptions.InputText.Split(" "))
            {
                var braillePointsForWord = WordToBraillePoints(word);

                if (braillePointsForWord.Count + currentLine.Count <= maxLineLength)
                {
                    // Found a word, that fits in the current line
                    currentLine.AddRange(braillePointsForWord);

                    // Only add a whitespace if there is space left
                    // No need to break to the next line. The next word will do that anyway
                    if (currentLine.Count + 1 < maxLineLength)
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
                    
                    while (braillePointsForWord.Count > maxLineLength)
                    {
                        currentLine.AddRange(braillePointsForWord.Take(maxLineLength - 1).ToList());
                        currentLine.AddRange(WordToBraillePoints("-"));
                        braillePointsForWord.RemoveRange(0, maxLineLength - 1);
                        textWithLineBreaks.Add(currentLine);
                        currentLine = new List<string>();
                    }

                    currentLine.AddRange(braillePointsForWord);

                    if (currentLine.Count + 1 < maxLineLength)
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

            for (int pageNumber = 1; textWithLineBreaks.Count > 0; ++pageNumber)
            {
                // Print each page seperately
                var page = textWithLineBreaks.Take(maxRowsPerPage).ToList();
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
                    clOptions.TextOutput ? StlGenerator.StlGenerator.OutputFormat.Text : StlGenerator.StlGenerator.OutputFormat.Binary);
            }

            return 0;
        }

        private static List<string> WordToBraillePoints(string inputWord)
        {
            // Braille is numbered like this:
            // 1 4
            // 2 5
            // 3 6
            // So we can determine the position of the knobs by listing all the used positions (Yes this could be done with a bit mask, but is easier this way)
            const string NumberIndicator = "3456";
            var brailleAlphabet = new Dictionary<string, string>()
            {
                // Group 1
                { "A".ToLower(), "1" },
                { "B".ToLower(), "12" },
                { "C".ToLower(), "14" },
                { "D".ToLower(), "145" },
                { "E".ToLower(), "15" },
                { "F".ToLower(), "124" },
                { "G".ToLower(), "1245" },
                { "H".ToLower(), "125" },
                { "I".ToLower(), "24" },
                { "J".ToLower(), "245" },

                // Group 2 (Group 1 with prefix of '3')
                { "K".ToLower(), "31" },
                { "L".ToLower(), "312" },
                { "M".ToLower(), "314" },
                { "N".ToLower(), "3145" },
                { "O".ToLower(), "315" },
                { "P".ToLower(), "3124" },
                { "Q".ToLower(), "31245" },
                { "R".ToLower(), "3125" },
                { "S".ToLower(), "324" },
                { "T".ToLower(), "3245" },

                // Group 3 (Group 1 with prefix of '36')
                { "U".ToLower(), "361" },
                { "V".ToLower(), "3612" },
                { "X".ToLower(), "3614" },
                { "Y".ToLower(), "36145" },
                { "ß".ToLower(), "3615" },
                { "ST".ToLower(), "36124" },

                // Group 4 (Group 1 with prefix of '6')
                { "AU".ToLower(), "61" },
                { "EU".ToLower(), "612" },
                { "EI".ToLower(), "614" },
                { "CH".ToLower(), "6145" },
                { "SCH".ToLower(), "615" },
                { "Ü".ToLower(), "6124" },
                { "Ö".ToLower(), "61245" },
                { "W".ToLower(), "6125" },

                // Combinations that fall out of line
                { "ÄU".ToLower(), "34" },
                { "Ä".ToLower(), "345" },
                { "IE".ToLower(), "346" },
                { ".".ToLower(), "3" },
                { "-".ToLower(), "36" },
                { ",".ToLower(), "2" },
                { ";".ToLower(), "23" },
                { ":".ToLower(), "25" },
                { "?".ToLower(), "26" },
                { "!".ToLower(), "235" },
            
                // Numbers (are indicated by the NumberIndicator)
                { "1".ToLower(), "1" },
                { "2".ToLower(), "12" },
                { "3".ToLower(), "14" },
                { "4".ToLower(), "145" },
                { "5".ToLower(), "15" },
                { "6".ToLower(), "124" },
                { "7".ToLower(), "1245" },
                { "8".ToLower(), "125" },
                { "9".ToLower(), "24" },
                { "0".ToLower(), "245" },
            };


            List<string> output = new List<string>();

            var maxKeyLength = 3; // The maximum length of a key in the alphabet
            for (int i = 0; i < inputWord.Length; ++i)
            {
                // Start looking for the largest chunks first and then trickle down to lower lengths
                for (int k = maxKeyLength; k > 0; --k)
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