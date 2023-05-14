namespace TextGenerator.Alphabet
{
    public class SixPointLetter : ILetter
    {
        private UInt16 Bitmask = 0x0000;

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
            Initialize(encoding);
        }

        public SixPointLetter(UInt16 bitmask, bool isNumber)
            : this(isNumber)
        {
            Initialize(bitmask);
        }

        public bool IsNumber { get; private set; }

        public void Initialize(string encoding, bool isNumber)
        {
            if (string.IsNullOrEmpty(encoding))
            {
                return;
            }

            UInt16 bitmask = 0x0000;

            for (int i = 0; i < 6; i++)
            {
                if (encoding.Contains($"{i}"))
                {
                    bitmask += (UInt16)(1 << i);
                }
            }

            Initialize(bitmask, isNumber);
        }

        public void Initialize(UInt16 bitmask, bool isNumber)
        {
            Bitmask = bitmask;
            IsNumber = isNumber;
        }

        public bool KnobIsPresent(int knobNumber)
        {
            return (Bitmask & (1 << knobNumber)) != 0;
        }
    }
}
