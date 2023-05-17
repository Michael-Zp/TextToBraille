using Newtonsoft.Json;

namespace TextGenerator.Alphabet
{
    public class SixPointLetter : Letter
    {
        public override bool IsNumber { get; set; }
        public override string Symbol { get; set; }

        [JsonIgnore]
        public override string EncodingAsString
        {
            get
            {
                string str = "";

                for (var i = 0; i < 6; i++)
                {
                    if (IsPointPresent(i))
                    {
                        str += $"{i + 1}";
                    }
                }

                return str;
            }
        }

        public SixPointLetter()
        {

        }

        private SixPointLetter(bool isNumber)
        {
            IsNumber = isNumber;
        }

        /// <summary>
        /// Use a string as a representation for a braille letter.
        /// Simply list all the points that are present in the letter in this encoding:
        /// 
        /// 1 4
        /// 2 5
        /// 3 6
        /// 
        /// E.g. "156"
        /// </summary>
        /// <param name="encoding"></param>
        public SixPointLetter(string encoding, bool isNumber)
            : this(isNumber)
        {
            Initialize(encoding, isNumber);
        }

        public SixPointLetter(UInt16 bitmask, bool isNumber)
            : this(isNumber)
        {
            Initialize(bitmask, isNumber);
        }

        public override void Initialize(string encoding, bool isNumber, string symbol = "")
        {
            if (string.IsNullOrEmpty(encoding))
            {
                return;
            }

            UInt16 bitmask = 0x0000;

            for (int i = 1; i <= 6; i++)
            {
                if (encoding.Contains($"{i}"))
                {
                    bitmask += (UInt16)(1 << (i - 1));
                }
            }

            Initialize(bitmask, isNumber, symbol);
        }

        public override void Initialize(UInt16 bitmask, bool isNumber, string symbol = "")
        {
            Bitmask = bitmask;
            IsNumber = isNumber;
            Symbol = symbol;
        }

        public override bool IsPointPresent(int knobNumber)
        {
            return (Bitmask & (1 << knobNumber)) != 0;
        }
    }
}
