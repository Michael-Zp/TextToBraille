using Newtonsoft.Json;

namespace TextGenerator.Alphabet
{
    public abstract class Letter
    {
        public abstract bool IsPointPresent(int knobNumber);

        public UInt16 Bitmask { get; set; }

        public abstract bool IsNumber { get; set; }

        public abstract string Symbol { get; set; }

        [JsonIgnore]
        public abstract string EncodingAsString { get; }

        public abstract void Initialize(string encoding, bool isNumber, string symbol = "");

        public abstract void Initialize(UInt16 encoding, bool isNumber, string symbol = "");
    }
}
