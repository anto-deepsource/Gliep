using System.Collections.Generic;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.CommandLineParser.Default;

namespace GeminiLab.Gliep.Dumper {
    // ReSharper disable UnassignedGetOnlyAutoProperty UnusedAutoPropertyAccessor.Local ClassNeverInstantiated.Global
    public class CommandLineOptions {
        [ShortOption('t'), LongOption("dump-token-stream")]
        private bool DumpTokenStream { get; set; }

        [ShortOption('a'), LongOption("dump-ast")]
        private bool DumpAST { get; set; }

        [ShortOption('u'), LongOption("dump-unit")]
        private bool DumpUnit { get; set; }

        [ShortOption('n'), LongOption("do-not-execute")]
        private bool DoNotExecute { get; set; }

        private void SetDefaultOptions() {
            DumpTokenStream = DumpAST = DumpUnit = DoNotExecute = false;
        }

        [ShortOption('c', OptionParameter.Required), LongOption("code", OptionParameter.Required)]
        private void OnCommandLineSource(string code) {
            Inputs.Add(new Input(DumpTokenStream, DumpAST, DumpUnit, !DoNotExecute, true, code));
            
            SetDefaultOptions();
        }

        [NonOptionArgument]
        private void OnFileSource(string file) {
            Inputs.Add(new Input(DumpTokenStream, DumpAST, DumpUnit, !DoNotExecute, false, file));
            
            SetDefaultOptions();
        }

        public class Input {
            public Input(bool dumpTokenStream, bool dumpAST, bool dumpUnit, bool execute, bool isCommandLine, string inputContent) {
                DumpTokenStream = dumpTokenStream;
                DumpAST = dumpAST;
                DumpUnit = dumpUnit;
                Execute = execute;
                IsCommandLine = isCommandLine;
                InputContent = inputContent;
            }
            
            public bool   DumpTokenStream { get; set; }
            public bool   DumpAST         { get; set; }
            public bool   DumpUnit        { get; set; }
            public bool   Execute         { get; set; }
            public bool   IsCommandLine   { get; set; }
            public string InputContent    { get; set; }
        }

        public IList<Input> Inputs { get; } = new List<Input>();
        
        public CommandLineOptions() {
            SetDefaultOptions();
        }
    }
    // ReSharper restore UnassignedGetOnlyAutoProperty UnusedAutoPropertyAccessor.Local ClassNeverInstantiated.Global
}
