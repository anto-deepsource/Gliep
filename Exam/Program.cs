using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;

using GeminiLab.Core2;
using GeminiLab.Core2.Collections;
using GeminiLab.Core2.IO;
using GeminiLab.Core2.Text;
using GeminiLab.Core2.Yielder;
using GeminiLab.Glos.CodeGenerator;
using GeminiLab.Glos.ViMa;
using GeminiLab.Glug;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Parser;
using GeminiLab.Glug.Tokenizer;

namespace Exam {
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
                        opText += $" <{ip:X6}>";
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
            if (s[^1].IsUpper() || s[^1].IsDecimalDigit()) {
                return s[..^1].ToLower() + "." + s[^1];
            }

            return s.ToLower();
        }

        public static void DumpUnit(GlosUnit unit) {
            Console.WriteLine("= Glos Unit");
            Console.WriteLine("== overview");
            Console.WriteLine($"{unit.FunctionTable.Count} function(s), {unit.StringTable.Count} string constant(s), entry #{unit.Entry}");
            Console.WriteLine("== string constant table");
            for (int i = 0; i < unit.StringTable.Count; ++i) Console.WriteLine($"#{i}: \"{EscapeSequenceConverter.Encode(unit.StringTable[i])}\"");
            Console.WriteLine("== function table");
            for (int i = 0; i < unit.FunctionTable.Count; ++i) {
                var fun = unit.FunctionTable[i];

                Console.WriteLine($"#{i}: loc size {fun.LocalVariableSize}{(fun.VariableInContext.Count > 0 ? $", ctx var: {fun.VariableInContext.JoinBy(", ")}" : "")}{(unit.Entry == i ? ", entry" : "")}");

                var ops = fun.Op;
                var ip = 0;

                while (ip < ops.Length) {
                    Console.WriteLine(ReadNextOp(in ops, ref ip));
                }
            }
        }

        public static void Main(string[] args) {
            var sb = new StringBuilder();

            string s;
            while ((s = Console.ReadLine()) != null) sb.AppendLine(s);

            var tok = new GlugTokenizer(new StringReader(sb.ToString()));
            /*
            while (true) {
                var token = tok.GetToken();
                if (token == null) break;

                Console.WriteLine(token.Type);
            } 
            return;
            */
            var root = GlugParser.Parse(tok);
            // new DumpVisitor(new IndentedWriter(Console.Out)).Visit(root);

            var rootFun = new Function("<root>", new List<string>(), root);

            var vdv = new FunctionAndVarDefVisitor();
            vdv.Visit(rootFun);

            var vcv = new VarRefVisitor(vdv.RootTable);
            vcv.Visit(rootFun);

            vdv.DetermineVariablePlace();
            
            var gen = new CodeGenVisitor();
            gen.Visit(rootFun);

            var unit = gen.Builder.GetResult();

            // DumpUnit(unit);

            var global = new GlosContext(null!);
            global.CreateVariable("print", GlosValue.NewExternalFunction(param => {
                Console.WriteLine(param.Select(x => x.ToString()).JoinBy(" "));

                return Array.Empty<GlosValue>();
            }));

            var vm = new GlosViMa();
            vm.ExecuteUnit(unit, Array.Empty<GlosValue>(), global);
        }

        public static T Default<T>() => default;
    }
}
