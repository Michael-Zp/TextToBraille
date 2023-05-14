namespace Shared
{
    public class PrintPlateProperties
    {
        public int MaxLineLength { get; private set; }

        public int MaxRowsPerPage { get; private set; }

        public static PrintPlateProperties Ender3V2(TypesettingOptions typesettingOptions)
        {
            return new PrintPlateProperties()
            {
                MaxLineLength = (int)Math.Floor((200 - typesettingOptions.SpaceBetweenLetters) / (typesettingOptions.LetterWidth + typesettingOptions.SpaceBetweenLetters)) - 1,
                MaxRowsPerPage = (int)Math.Floor((210 - typesettingOptions.SpaceBetweenLines) / (typesettingOptions.LetterHeight + typesettingOptions.SpaceBetweenLines)) - 1,
            };
        }

    }
}