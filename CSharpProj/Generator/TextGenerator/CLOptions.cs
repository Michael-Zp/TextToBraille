using CommandLine;

namespace TextGenerator
{
    public class CLOptions
    {
        [Option('o', "OutputPath", Default = ".")]
        public string OutputDirectory { get; set; } = ".";

        [Option('n', "NamePrefix", Default = ".")]
        public string NamePrefix { get; set; } = string.Empty;

        [Option('i', "InputText", Required = true)]
        public string InputText { get; set; } = "LoremIpsum";

        [Option('t', "TextOutput")]
        public bool TextOutput { get; set; } = false;

        [Option('b', "PrintBuildplate")]
        public bool PrintWithBuildplate { get; set; } = false;
    }
}
