using System;
using System.Collections.Generic;
using GeminiLab.Core2.Random;
using GeminiLab.Core2.Random.RNG;
using GeminiLab.Glos;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glos {
    public partial class Calculation : GlosTestBase {
        private static GlosOp[] IntegerBiOpList { get; } = new[] {
            GlosOp.Add, GlosOp.Sub, GlosOp.Mul, GlosOp.Div,
            GlosOp.Mod, GlosOp.Lsh, GlosOp.Rsh, GlosOp.And,
            GlosOp.Orr, GlosOp.Xor, GlosOp.Gtr, GlosOp.Lss,
            GlosOp.Geq, GlosOp.Leq, GlosOp.Equ, GlosOp.Neq,
        };

        public static IEnumerable<object[]> GetIntegerBiOpTestCases(int count) {
            GlosOp op;
            long opl, opr, result;
            var opc = IntegerBiOpList.MakeChooser();
            var ran = new U64ToI64RNG(DefaultRNG.U64);
            for (int i = 0; i < count; ++i) {
                op = opc.Next();
                opl = ran.Next();
                opr = ran.Next();

                unchecked {
                    result = op switch {
                        GlosOp.Add => opl + opr,
                        GlosOp.Sub => opl - opr,
                        GlosOp.Mul => opl * opr,
                        GlosOp.Div => opl / opr,
                        GlosOp.Mod => opl % opr,
                        GlosOp.Lsh => opl << (int)opr,
                        GlosOp.Rsh => (long)((ulong)opl >> (int)opr),
                        GlosOp.And => opl & opr,
                        GlosOp.Orr => opl | opr,
                        GlosOp.Xor => opl ^ opr,
                        GlosOp.Gtr => opl > opr ? -1L : 0,
                        GlosOp.Lss => opl < opr ? -1L : 0,
                        GlosOp.Geq => opl >= opr ? -1L : 0,
                        GlosOp.Leq => opl <= opr ? -1L : 0,
                        GlosOp.Equ => opl == opr ? -1L : 0,
                        GlosOp.Neq => opl != opr ? -1L : 0,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }

                yield return new object[] { op, opl, opr, result };
            }
        }

        [Theory]
        [MemberData(nameof(GetIntegerBiOpTestCases), 2048)]
        public void IntegerBiOp(GlosOp op, long opl, long opr, long result) {
            var fgen = Builder.AddFunction();

            fgen.AppendLd(opl);
            fgen.AppendLd(opr);
            fgen.AppendInstruction(op);

            fgen.SetEntry();

            var res = Execute();

            var checker = GlosValueArrayChecker.Create(res).FirstOne();

            if (op == GlosOp.Gtr || op == GlosOp.Lss || op == GlosOp.Geq || op == GlosOp.Leq || op == GlosOp.Equ || op == GlosOp.Neq) {
                checker.AssertBoolean(result != 0);
            } else {
                checker.AssertInteger(result);
            }

            checker.MoveNext().AssertEnd();
        }
    }
}
