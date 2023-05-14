namespace TextGenerator.Alphabet
{
    public interface ILetter
    {
        bool IsPointPresent(int knobNumber);

        bool IsNumber { get; }

        void Initialize(string encoding, bool isNumber, string symbol = "");

        void Initialize(UInt16 encoding, bool isNumber, string symbol = "");
    }
}
