namespace TextGenerator.Alphabet
{
    public class Alphabet<TLetter>
        where TLetter : ILetter, new()
    {
        private Dictionary<string, TLetter> Letters { get; set; } = new Dictionary<string, TLetter>();

        public TLetter NumberIndicator { get; set; } = new TLetter();

        /// <summary>
        /// The maximum length of a symbol in the alphabet (e.g. 'sch' is one symbol in German)
        /// </summary>
        public int MaxSymbolLength { get; private set; }

        public Alphabet(string numberIndicator)
        {
            NumberIndicator.Initialize(numberIndicator, false);
        }

        public void AddLetter(string symbol, string brailleEncoding)
        {
            TLetter newLetter = new TLetter();
            newLetter.Initialize(brailleEncoding, IsNumber(symbol), symbol);
            AddLetter(symbol, newLetter);
        }

        public void AddLetter(string symbol, UInt16 brailleBitmask)
        {
            TLetter newLetter = new TLetter();
            newLetter.Initialize(brailleBitmask, IsNumber(symbol));
            AddLetter(symbol, newLetter);
        }

        public void AddLetter(string letter, TLetter brailleChar)
        {
            Letters.Add(letter, brailleChar);
            MaxSymbolLength = Math.Max(MaxSymbolLength, letter.Length);
        }

        public bool TryGetSymbol(string symbol, out TLetter brailleLetter)
        {
            brailleLetter = default;
            if (Letters.ContainsKey(symbol))
            {
                brailleLetter = Letters[symbol];
                return true;
            }

            return false;
        }

        private bool IsNumber(string letter)
        {
            return letter.Length == 1 && char.IsNumber(letter[0]);
        }
    }
}
