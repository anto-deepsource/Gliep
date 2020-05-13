using System;
using System.IO.Abstractions;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.IO;
using GeminiLab.Core2.Logger;
using GeminiLab.Core2.Logger.Appenders;
using GeminiLab.Glug;
using GeminiLab.Glug.AST;
using GeminiLab.Glute.Compile;

namespace GeminiLab.Glute {
    public class CommandLineOptions {
        [Option(Option = 'd')]
        public string Path { get; set; } = ".";
    }

    public static class Program {
        public static void Main(string[] args) {
            var options = CommandLineParser<CommandLineOptions>.Parse(args);

            /*
            var tree = new GluteParser(new GluteTokenizer(Console.In)).Parse();
            new DumpVisitor(new IndentedWriter(Console.Out)).Visit(tree);
            var unit = GlutePostProcess.PostProcessAndCodeGen(tree);

            Gliep.Dumper.Program.DumpUnit(unit);
            return;
            */

            using var ctx = new LoggerContext();
            ctx.AddCategory("default");
            ctx.AddAppender("console", new ColoredConsoleAppender());
            ctx.Connect("default", "console", Filters.Threshold(Logger.LevelDebug));

            new Processor(new FileSystem(), ctx.GetLogger("default")!).ProcessDirectory(@"C:\Users\Gemini.APFEL\source\repos\Gliep\meta\example.glute\");

            return;
        }
    }
}
