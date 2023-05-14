namespace TextGenerator.Alphabet
{
    public interface ILetter
    {
        bool KnobIsPresent(int knobNumber);

        bool IsNumber { get; }

        void Initialize(string encoding, bool isNumber);

        void Initialize(UInt16 encoding, bool isNumber);
    }
}
