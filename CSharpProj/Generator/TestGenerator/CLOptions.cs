using CommandLine;

namespace StlGenerator
{
    public class CLOptions
    {
        [Option('o', "OutputPath", Default = ".")]
        public string OutputPath { get; set; } = ".";

        [Option('i', "BraillePoints", Required = true)]
        public string BraillePoints { get; set; } = "123456";
    }
}
