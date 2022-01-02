using System;
using System.Collections.Generic;
using GeminiLab.Core2.Random;
using GeminiLab.Core2.Random.RNG;
using GeminiLab.Glos;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glos {
    public partial class Calculation : GlosTestBase {
        private static GlosOp[] FloatBiOpList { get; } = new[] {
            GlosOp.Add, GlosOp.Sub, GlosOp.Mul, GlosOp.Div,
            GlosOp.Gtr, GlosOp.Lss, GlosOp.Geq, GlosOp.Leq,
            GlosOp.Equ, GlosOp.Neq,
        };

        public static IEnumerable<object[]> GetFloatBiOpTestCases(int count) {
            var opc = FloatBiOpList.MakeChooser();
            var ran = new U64ToI64RNG(DefaultRNG.U64);
            for (int i = 0; i < count; ++i) {
                var op = opc.Next();
                var opl = BitConverter.Int64BitsToDouble(ran.Next());
                var opr = BitConverter.Int64BitsToDouble(ran.Next());
                double result;

                if (op == GlosOp.Equ || op == GlosOp.Neq) {
                    if (DefaultRNG.Coin.Next() && DefaultRNG.Coin.Next()) opr = opl;
                }

                unchecked {
                    result = op switch {
                        GlosOp.Add => opl + opr,
                        GlosOp.Sub => opl - opr,
                        GlosOp.Mul => opl * opr,
                        GlosOp.Div => opl / opr,
                        GlosOp.Gtr => opl > opr ? double.PositiveInfinity : 0,
                        GlosOp.Lss => opl < opr ? double.PositiveInfinity : 0,
                        GlosOp.Geq => opl >= opr ? double.PositiveInfinity : 0,
                        GlosOp.Leq => opl <= opr ? double.PositiveInfinity : 0,
                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        GlosOp.Equ => opl == opr ? double.PositiveInfinity : 0,
                        GlosOp.Neq => opl != opr ? double.PositiveInfinity : 0,
                        // ReSharper restore CompareOfFloatsByEqualityOperator
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }

                yield return new object[] { op, opl, opr, result };
            }
        }

        [Theory]
        [MemberData(nameof(GetFloatBiOpTestCases), 2048)]
        public void FloatBiOp(GlosOp op, double opl, double opr, double result) {
            var fgen = Builder.AddFunction();

            fgen.AppendLdFlt(opl);
            fgen.AppendLdFlt(opr);
            fgen.AppendInstruction(op);

            fgen.SetEntry();

            var res = Execute();

            var checker = GlosValueArrayChecker.Create(res).FirstOne();

            if (op == GlosOp.Gtr || op == GlosOp.Lss || op == GlosOp.Geq || op == GlosOp.Leq || op == GlosOp.Equ || op == GlosOp.Neq) {
                checker.AssertBoolean(result > 0);
            } else {
                checker.AssertFloat(result);
            }

            checker.MoveNext().AssertEnd();
        }
    }
}
