using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GeminiLab.Core2;
using GeminiLab.Core2.Collections;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.IO;
using GeminiLab.Core2.Markup.Json;
using GeminiLab.Core2.Text;
using GeminiLab.Core2.Yielder;
using GeminiLab.Glos;
using GeminiLab.Glos.Serialization;
using GeminiLab.Glug;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Gliep.Dumper {
    public static class Program {
        private static string ReadNextOp(in ReadOnlySpan<byte> ops, ref int ip) {
            var sb = new StringBuilder($"{ip:X6}: ");

            var b = ip;
            var op = (GlosOp) ops[ip++];
            var len = ops.Length;

            ulong imm;
            long imms = 0;
            bool failed = false, hasImm = true;
            switch (GlosOpInfo.Immediates[(int) op]) {
            case GlosOpImmediate.Byte:
                if (ip + 1 > len) {
                    failed = true;
                } else {
                    imm = ops[ip++];
                    imms = unchecked((sbyte) (byte) imm);
                }

                break;
            case GlosOpImmediate.Dword:
                if (ip + 4 > len) {
                    failed = true;
                } else {
                    imm = (ulong) ops[ip]
                        | ((ulong) ops[ip + 1] << 8)
                        | ((ulong) ops[ip + 2] << 16)
                        | ((ulong) ops[ip + 3] << 24);

                    imms = unchecked((int) (uint) imm);
                    ip += 4;
                }

                break;
            case GlosOpImmediate.Qword:
                if (ip + 4 > len) {
                    failed = true;
                } else {
                    imm = (ulong) ops[ip]
                        | ((ulong) ops[ip + 1] << 8)
                        | ((ulong) ops[ip + 2] << 16)
                        | ((ulong) ops[ip + 3] << 24)
                        | ((ulong) ops[ip + 4] << 32)
                        | ((ulong) ops[ip + 5] << 40)
                        | ((ulong) ops[ip + 6] << 48)
                        | ((ulong) ops[ip + 7] << 56);

                    imms = unchecked((long) imm);
                    ip += 8;
                }

                break;
            default:
                imm = 0;
                imms = 0;
                hasImm = false;
                break;
            }

            string opText = "";
            if (!failed) {
                opText = OpToText(op);
                if (hasImm) {
                    if (op == GlosOp.LdFun
                     || op == GlosOp.LdStr
                     || op == GlosOp.LdFunS
                     || op == GlosOp.LdStrS
                     || op == GlosOp.LdQ
                     || op == GlosOp.Ld
                     || op == GlosOp.LdS
                     || op == GlosOp.LdArg
                     || op == GlosOp.LdLoc
                     || op == GlosOp.StLoc
                     || op == GlosOp.LdArgS
                     || op == GlosOp.LdLocS
                     || op == GlosOp.StLocS
                     || op == GlosOp.ShpRv
                     || op == GlosOp.ShpRvS) {
                        opText += $" {imms}";
                    } else if (op == GlosOp.LdFlt) {
                        IntegerFloatUnion ifu = default;
                        ifu.Integer = imms;
                        opText += $" {ifu.Float:E6}";
                    } else if (GlosOpInfo.Categories[(int) op] == GlosOpCategory.Branch) {
                        var target = ip + imms;
                        opText += $" <{target:X6}>";
                    }
                }
            } else {
                ip = len;
            }

            sb.Append($"{opText,-25};");
            foreach (var x in ops[b..ip]) {
                sb.Append($" {x:x2}");
            }

            return sb.ToString();
        }

        private static string OpToText(GlosOp op) {
            if (op == GlosOp.LdNeg1) return "ld.neg1";

            var s = op.ToString();
            if (s.Length > 1 && (s[^1].IsUpper() || s[^1].IsDecimalDigit())) {
                return s[..^1].ToLower() + "." + s[^1];
            }

            return s.ToLower();
        }

        public static void DumpUnit(IGlosUnit unit) {
            Console.WriteLine(@"= Glos Unit");
            Console.WriteLine(@"== overview");
            Console.WriteLine($@"{unit.FunctionTable.Count} function(s), {unit.StringTable.Count} string constant(s), entry #{unit.Entry}");
            Console.WriteLine(@"== string constant table");
            for (int i = 0; i < unit.StringTable.Count; ++i) Console.WriteLine($@"#{i}: ""{EscapeSequenceConverter.Encode(unit.StringTable[i])}""");
            Console.WriteLine(@"== function table");
            for (int i = 0; i < unit.FunctionTable.Count; ++i) {
                var fun = unit.FunctionTable[i];

                Console.WriteLine($@"#{i}: ""{fun.Name}"", loc size {fun.LocalVariableSize}{(fun.VariableInContext.Count > 0 ? $", ctx var: {fun.VariableInContext.JoinBy(", ")}" : "")}{(unit.Entry == i ? ", entry" : "")}");

                var ops = fun.Code;
                var ip = 0;

                while (ip < ops.Length) {
                    Console.WriteLine(ReadNextOp(in ops, ref ip));
                }
            }
        }

        public static void DumpTokenStream(IGlugTokenStream stream) {
            while (stream.HasNext()) {
                var tok = stream.Next();
                var output = string.Format($"{{0,-{tok.Position.Source.Length + 10}}}", $"({tok.Position})");

                output += ((GlugTokenType[]) typeof(GlugTokenType).GetEnumValues()).Contains(tok.Type) ? tok.Type.ToString() : $"0x{(uint) (tok.Type):x8}";
                if (tok.Type.HasInteger()) output += $", {tok.ValueInt}(0x{tok.ValueInt:x16})";
                if (tok.Type.HasString()) output += $", \"{EscapeSequenceConverter.Encode(tok.ValueString!)}\"";
                Console.WriteLine(output);
            }
        }

        public static void DumpHex(ReadOnlySpan<byte> b) {
            var len = b.Length;

            int ptr = 0;
            while (ptr < len) {
                Console.Write($@"{ptr:X8} ");

                for (int offset = 0; offset < 16; ++offset) {
                    if (ptr + offset < len) {
                        Console.Write($@" {b[ptr + offset]:X2}");
                    } else {
                        Console.Write(@"   ");
                    }

                    if (offset == 7) Console.Write(' ');
                }

                Console.Write(@" |");

                for (int offset = 0; offset < 16; ++offset) {
                    if (ptr + offset < len) {
                        Console.Write(ToPrintable(b[ptr + offset]));
                    } else {
                        Console.Write(' ');
                    }

                    if (offset == 7) Console.Write(' ');
                }

                Console.WriteLine(@"|");

                ptr += 16;
            }

            static char ToPrintable(byte input) {
                if (input >= ' ' && input <= '~') return (char) input;

                return '.';
            }
        }

        public class ResettableTokenStream : IGlugTokenStream {
            public void Dispose() { }

            private readonly IEnumerable<GlugToken> _tokens;
            private          IEnumerator<GlugToken> _enumerator;
            private          GlugToken?             _buff;

            public ResettableTokenStream(IGlugTokenStream source) {
                _tokens = source.ToList();
                _enumerator = _tokens.GetEnumerator();
            }

            public bool HasNext() {
                if (_buff != null) {
                    return true;
                }

                if (!_enumerator.MoveNext()) {
                    return false;
                }

                _buff = _enumerator.Current;
                return true;
            }

            public GlugToken Next() {
                if (_buff != null) {
                    var temp = _buff;
                    _buff = null;
                    return temp;
                }

                if (!HasNext()) throw new InvalidOperationException();

                return _buff!;
            }

            public void Reset() {
                _enumerator.Reset();
            }
        }

        public static void ProcessInput(CommandLineOptions.Input input) {
            var sourceName =
                input.IsCommandLine       ? "<command-line>" :
                input.InputContent == "-" ? "<stdin>" :
                                            input.InputContent;

            using var sourceReader =
                input.IsCommandLine       ? new StringReader(input.InputContent) :
                input.InputContent == "-" ? Console.In :
                                            new StreamReader(new FileStream(input.InputContent, FileMode.Open, FileAccess.Read));

            IGlugTokenStream tok = new GlugTokenizer(sourceReader, sourceName);

            if (input.DumpTokenStream) {
                var newTok = new ResettableTokenStream(tok);
                tok.Dispose();
                DumpTokenStream(newTok);
                newTok.Reset();
                tok = newTok;
            }

            var root = TypicalCompiler.Parse(tok);
            tok.Dispose();

            if (input.DumpAST) {
                new DumpVisitor(new IndentedWriter(Console.Out)).VisitNode(root);
            }

            var unit = TypicalCompiler.PostProcessAndCodeGen(root);

            if (input.DumpUnit) {
                DumpUnit(unit);
            }

            if (!input.Execute) return;

            var vm = new GlosViMa();
            vm.WorkingDirectory = Environment.CurrentDirectory;

            var global = new GlosContext(null!);
            GlosBuiltInFunctionGenerator.AddFromInstanceFunctions(new Functions(vm), global);
            try {
                var bin = GlosUnitSerializer.Serialize(unit);

                DumpHex(bin);

                var unit1 = GlosUnitSerializer.Deserialize(bin);
                if (unit1 != null) DumpUnit(unit1);

                vm.ExecuteUnit(unit, Array.Empty<GlosValue>(), global);
            } catch (Exception ex) {
                Console.WriteLine($@"{ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void Main(string[] args) {
            var opt = new CommandLineParser<CommandLineOptions>().Parse(args);

            opt.Inputs.ForEach(ProcessInput);
        }
    }
}
