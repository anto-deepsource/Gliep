using System;
using System.IO;
using System.Linq;
using System.Text;
using GeminiLab.Core2;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.IO;
using GeminiLab.Core2.Text;
using GeminiLab.Glos;
using GeminiLab.Glug;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Gliep.Dumper {
    public static class Program {
        private static string ReadNextOp(in ReadOnlySpan<byte> ops, ref int ip) {
            var sb = new StringBuilder($"{ip:X6}: ");

            var b = ip;
            var op = (GlosOp)ops[ip++];
            var len = ops.Length;

            ulong imm;
            long imms = 0;
            bool failed = false, hasImm = true;
            switch (GlosOpInfo.Immediates[(int)op]) {
            case GlosOpImmediate.Byte:
                if (ip + 1 > len) {
                    failed = true;
                } else {
                    imm = ops[ip++];
                    imms = unchecked((sbyte)(byte)imm);
                }
                break;
            case GlosOpImmediate.Dword:
                if (ip + 4 > len) {
                    failed = true;
                } else {
                    imm = (ulong)ops[ip]
                          | ((ulong)ops[ip + 1] << 8)
                          | ((ulong)ops[ip + 2] << 16)
                          | ((ulong)ops[ip + 3] << 24);
                    imms = unchecked((int)(uint)imm);
                    ip += 4;
                }
                break;
            case GlosOpImmediate.Qword:
                if (ip + 4 > len) {
                    failed = true;
                } else {
                    imm = (ulong)ops[ip]
                          | ((ulong)ops[ip + 1] << 8)
                          | ((ulong)ops[ip + 2] << 16)
                          | ((ulong)ops[ip + 3] << 24)
                          | ((ulong)ops[ip + 4] << 32)
                          | ((ulong)ops[ip + 5] << 40)
                          | ((ulong)ops[ip + 6] << 48)
                          | ((ulong)ops[ip + 7] << 56);
                    imms = unchecked((long)imm);
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
                    if (op == GlosOp.LdFun || op == GlosOp.LdStr || op == GlosOp.LdFunS || op == GlosOp.LdStrS
                        || op == GlosOp.LdQ || op == GlosOp.Ld || op == GlosOp.LdS || op == GlosOp.LdArg
                        || op == GlosOp.LdLoc || op == GlosOp.StLoc || op == GlosOp.LdArgS || op == GlosOp.LdLocS
                        || op == GlosOp.StLocS || op == GlosOp.ShpRv || op == GlosOp.ShpRvS) {
                        opText += $" {imms}";
                    } else if (op == GlosOp.LdFlt) {
                        IntegerFloatUnion ifu = default;
                        ifu.Integer = imms;
                        opText += $" {ifu.Float:E6}";
                    } else if (GlosOpInfo.Categories[(int)op] == GlosOpCategory.Branch) {
                        var target = ip + imms;
                        opText += $" <{target:X6}>";
                    }
                }
            } else {
                ip = len;
            }

            sb.Append($"{opText,-25}");
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

        public static void DumpUnit(GlosUnit unit) {
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
                var output = string.Format($"{{0,-{tok.Source.Length + 10}}}", $"({tok.Source}:{tok.Row}:{tok.Column})");

                output += ((GlugTokenType[])typeof(GlugTokenType).GetEnumValues()).Contains(tok.Type) ? tok.Type.ToString() : $"0x{(uint)(tok.Type):x8}";
                if (tok.Type.HasInteger()) output += $", {tok.ValueInt}(0x{tok.ValueInt:x16})";
                if (tok.Type.HasString()) output += $", \"{EscapeSequenceConverter.Encode(tok.ValueString!)}\"";
                Console.WriteLine(output);
            }
        }

        public static void Main(string[] args) {
            var options = CommandLineParser<CommandLineOptions>.Parse(args);

            var sourceName = 
                options.Code != null ? "<command-line>" : 
                options.Input == "-" ? "<stdin>" : 
                options.Input;
            using var sourceReader =
                options.Code != null ? new StringReader(options.Code) :
                options.Input == "-" ? Console.In :
                new StreamReader(new FileStream(options.Input, FileMode.Open, FileAccess.Read));

            using var tok = new GlugTokenizer(sourceReader, sourceName);

            if (options.DumpTokenStreamAndExit) {
                DumpTokenStream(tok);
                return;
            }

            var root = TypicalCompiler.Parse(tok);

            if (options.DumpAST || options.DumpASTAndExit) {
                new DumpVisitor(new IndentedWriter(Console.Out)).Visit(root);

                if (options.DumpASTAndExit) return;
            }

            var unit = TypicalCompiler.PostProcessAndCodeGen(root);

            if (options.DumpUnit || options.DumpUnitAndExit) {
                DumpUnit(unit);

                if (options.DumpUnitAndExit) return;
            }

            var vm = new GlosViMa();

            var global = new GlosContext(null!);
            global.CreateVariable("print", GlosValue.NewExternalFunction(param => {
                Console.WriteLine(param.Select(x => vm.Calculator.Stringify(x)).JoinBy(" "));

                return Array.Empty<GlosValue>();
            }));
            global.CreateVariable("debug", GlosValue.NewExternalFunction(param => {
                Console.WriteLine(param.Select(x => GlosValue.Calculator.DebugStringify(x)).JoinBy(" "));

                return Array.Empty<GlosValue>();
            }));
            global.CreateVariable("format", GlosValue.NewExternalFunction(param => {
                if (param.Length <= 0) return new GlosValue[] { "" };

                var format = param[0].AssertString();
                var args = param[1..].Select(x => x.Type switch {
                    GlosValueType.Nil => "nil",
                    GlosValueType.Integer => x.ValueNumber.Integer,
                    GlosValueType.Float => x.ValueNumber.Float,
                    GlosValueType.Boolean => x.Truthy(),
                    _ => x.ValueObject,
                }).ToArray();

                return new GlosValue[] { string.Format(format, args: args) };
            }));

            try {
                vm.ExecuteUnit(unit, Array.Empty<GlosValue>(), global);
            } catch (Exception ex) {
                Console.WriteLine($@"{ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
