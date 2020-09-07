using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using GeminiLab.Core2;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.IO;
using GeminiLab.Core2.Logger;
using GeminiLab.Core2.Logger.Appenders;
using GeminiLab.Glug;
using GeminiLab.Glug.AST;
using GeminiLab.Glute.Compile;

namespace GeminiLab.Glute {
    public class CommandLineOptions {
        [Option(Option = 'd')] public string Path { get; set; } = ".";
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
            ctx.AddCategory("virtual");
            ctx.AddAppender("console", new ColoredConsoleAppender());
            ctx.Connect("default", "console", Filters.Threshold(Logger.LevelDebug));
            ctx.Connect("virtual", "console", Filters.Threshold(Logger.LevelDebug));

            // new Processor(new FileSystem(), ctx.GetLogger("default")!).ProcessDirectory(options.Path);

            var mfs = new MockFileSystem();
            mfs.AddDirectory("/");
            mfs.AddFile("/.glute", new MockFileData("<~~ !x = 1; ~~>"));
            mfs.AddFile("/a.glute", new MockFileData("<~ x ~~>"));
            mfs.AddFile("/b.glute", new MockFileData("<~~x=x+1;~~>\n<~x*10~>\nb"));

            new Processor(mfs, ctx.GetLogger("virtual")!).ProcessDirectory("/");

            Console.WriteLine(mfs.GetFile("/a").Contents.Decode(Encoding.UTF8));
            Console.WriteLine(mfs.GetFile("/b").Contents.Decode(Encoding.UTF8));

            return;
        }
    }
}
