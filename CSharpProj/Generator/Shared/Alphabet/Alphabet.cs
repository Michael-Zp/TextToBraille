using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace TextGenerator.Alphabet
{
    public class Alphabet<TLetter>
        where TLetter : Letter, new()
    {
        public Dictionary<UInt64, TLetter> Letters { get; private set; } = new Dictionary<UInt64, TLetter>();

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
            if (letter.Length > 4)
            {
                throw new ArgumentException("Symbol can't be longer than 4 characters.");
            }

            Letters.Add(LetterToIndex(letter), brailleChar);
            MaxSymbolLength = Math.Max(MaxSymbolLength, letter.Length);
        }

        public bool TryGetSymbol(ReadOnlySpan<char> symbol, out TLetter brailleLetter)
        {
            brailleLetter = default;
            UInt64 index = LetterToIndex(symbol);
            if (Letters.ContainsKey(index))
            {
                brailleLetter = Letters[index];
                return true;
            }

            return false;
        }

        private bool IsNumber(string letter)
        {
            return letter.Length == 1 && char.IsNumber(letter[0]);
        }
        
        private UInt64 LetterToIndex(ReadOnlySpan<char> letter)
        {
            UInt64 index = 0;
            for (int i = 0; i < letter.Length; ++i)
            {
                index = index + ((ulong)letter[i] << (sizeof(char) * i));
            }
            return index;
        }
    }
}
