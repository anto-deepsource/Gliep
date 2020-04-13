using System;
using GeminiLab.Core2.CommandLineParser;

namespace GeminiLab.Glute {
    public class CommandLineOptions {
        [Option(Option = 'd')]
        public string Path { get; set; } = ".";
    }

    public static class Program {
        public static void Main(string[] args) {
            var options = CommandLineParser<CommandLineOptions>.Parse(args);


        }
    }
}
