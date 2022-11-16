public static class AlphabetGenerator
{
    struct InputDataSet
    {
        public string Letter;
        public string BraillePoints;

        public InputDataSet(string letter, string braillePoints)
        {
            Letter = letter;
            BraillePoints = braillePoints;
        }
    }

    public static int Main(string[] args)
    {
        var inputData = new List<InputDataSet>()
        {
            // Group 1
            new InputDataSet("A", "1"),
            new InputDataSet("B", "12"),
            new InputDataSet("C", "14"),
            new InputDataSet("D", "145"),
            new InputDataSet("E", "15"),
            new InputDataSet("F", "124"),
            new InputDataSet("G", "1245"),
            new InputDataSet("H", "125"),
            new InputDataSet("I", "24"),
            new InputDataSet("J", "245"),

            // Group 2
            new InputDataSet("K", "31"),
            new InputDataSet("L", "312"),
            new InputDataSet("M", "314"),
            new InputDataSet("N", "3145"),
            new InputDataSet("O", "315"),
            new InputDataSet("P", "3124"),
            new InputDataSet("Q", "31245"),
            new InputDataSet("R", "3125"),
            new InputDataSet("S", "324"),
            new InputDataSet("T", "3245"),

            // Group 3
            new InputDataSet("U", "361"),
            new InputDataSet("V", "3612"),
            new InputDataSet("X", "3614"),
            new InputDataSet("Y", "36145"),
            new InputDataSet("ß", "3615"),
            new InputDataSet("ST", "36124"),

            // Group 4
            new InputDataSet("AU", "61"),
            new InputDataSet("EU", "612"),
            new InputDataSet("EI", "614"),
            new InputDataSet("CH", "6145"),
            new InputDataSet("SCH", "615"),
            new InputDataSet("Ü", "6124"),
            new InputDataSet("Ö", "61245"),
            new InputDataSet("W", "6125"),

            // Alternative combinations
            new InputDataSet("ÄU", "34"),
            new InputDataSet("Ä", "345"),
            new InputDataSet("IE", "346"),
            new InputDataSet("Dot", "3"),
            new InputDataSet("Dash", "36"),
            new InputDataSet("Comma", "2"),
            new InputDataSet("SemiColon", "23"),
            new InputDataSet("Colon", "25"),
            new InputDataSet("Questionmark", "26"),
            new InputDataSet("Exclamationmark", "235"),
            
            // Numbers
            new InputDataSet("Number", "3456"),
            new InputDataSet("1", "1"),
            new InputDataSet("2", "12"),
            new InputDataSet("3", "14"),
            new InputDataSet("4", "145"),
            new InputDataSet("5", "15"),
            new InputDataSet("6", "124"),
            new InputDataSet("7", "1245"),
            new InputDataSet("8", "125"),
            new InputDataSet("9", "24"),
            new InputDataSet("0", "245"),
        };

        foreach (var entry in inputData)
        {
            //StlGenerator.StlGenerator.GenerateStl($"Letter{entry.Letter}", $"J:\\3DPrinter\\Braille\\Alphabet\\{entry.Letter}.stl", entry.BraillePoints);
        }


        return 0;
    }
}