namespace Shared
{
    public class TypesettingOptions
    {
        public float LetterWidth;
        public float LetterHeight;
        public float SpaceBetweenLetters;
        public float SpaceBetweenLines;

        private TypesettingOptions()
        { }

        public TypesettingOptions(float letterWidth, float letterHeight, float spaceBetweenLetters, float spaceBetweenLines)
        {
            LetterWidth = letterWidth;
            LetterHeight = letterHeight;
            SpaceBetweenLetters = spaceBetweenLetters;
            SpaceBetweenLines = spaceBetweenLines;
        }
    }
}