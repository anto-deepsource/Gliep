using System;
using System.Collections.Generic;
using System.Text;
using GeminiLab.Core2.CommandLineParser;

namespace GeminiLab.Gliep {
    // ReSharper disable UnassignedGetOnlyAutoProperty UnusedAutoPropertyAccessor.Local
    public class CommandLineOptions {
        [Option(Option = 'k', LongOption = "dump-token-stream-and-exit")]
        public bool DumpTokenStreamAndExit { get; private set; }

        [Option(Option = 'a', LongOption = "dump-ast")]
        public bool DumpAST { get; private set; }

        [Option(Option = 's', LongOption = "dump-ast-and-exit")]
        public bool DumpASTAndExit { get; private set; }

        [Option(Option = 'u', LongOption = "dump-unit")]
        public bool DumpUnit { get; private set; }

        [Option(Option = 'n', LongOption = "dump-unit-and-exit")]
        public bool DumpUnitAndExit { get; private set; }

        [Option(Option = 'i', LongOption = "input")]
        public string Input { get; private set; } = "-";

        [Option(Option = 'c', LongOption = "code")]
        public string? Code { get; private set; } = null;
    }
    // ReSharper restore UnassignedGetOnlyAutoProperty UnusedAutoPropertyAccessor.Local
}
