using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace TextGenerator.Alphabet
{
    public class Alphabet<TLetter>
        where TLetter : Letter, new()
    {
        public Dictionary<string, TLetter> Letters { get; private set; } = new Dictionary<string, TLetter>();

        public TLetter NumberIndicator { get; private set; } = new TLetter();

        /// <summary>
        /// The maximum length of a symbol in the alphabet (e.g. 'sch' is one symbol in German)
        /// </summary>
        public int MaxSymbolLength { get; private set; }

        public Alphabet(string numberIndicator)
        {
            NumberIndicator.Initialize(numberIndicator, false);
        }

        private struct JsonFormat
        {
            public List<TLetter> Letters;

            public string NumberIndicator;
        }

        public string ToJson(bool indented = false)
        {
            JsonFormat jsonFormat = new JsonFormat()
            {
                Letters = new List<TLetter>()
            };

            foreach (var kvp in Letters)
            {
                jsonFormat.Letters.Add(kvp.Value);
            }
            jsonFormat.NumberIndicator = NumberIndicator.EncodingAsString;

            return JsonConvert.SerializeObject(jsonFormat, indented ? Formatting.Indented : Formatting.None);
        }

        public static Alphabet<TLetter> FromJson(string json)
        {
            JsonFormat jsonFormat = JsonConvert.DeserializeObject<JsonFormat>(json);
            Alphabet<TLetter> alphabet = new Alphabet<TLetter>(jsonFormat.NumberIndicator);

            foreach (var letter in jsonFormat.Letters)
            {
                alphabet.AddLetter(letter.Symbol, letter.EncodingAsString);
            }

            return alphabet;
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
