namespace Shared
{
    public class PrintOptions
    {
        public bool PrintWithBasePlate { get; private set; }

        public PrintOptions(bool printWithBasePlate)
        {
            PrintWithBasePlate = printWithBasePlate;
        }
    }
}