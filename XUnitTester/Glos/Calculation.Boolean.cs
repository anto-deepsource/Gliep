using System;
using System.Collections.Generic;
using GeminiLab.Glos;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glos {
    public partial class Calculation {
        private static GlosOp[] BooleanBiOpList { get; } = new[] {
            GlosOp.And, GlosOp.Orr, GlosOp.Xor, GlosOp.Equ, GlosOp.Neq,
        };

        public static IEnumerable<object[]> GetBooleanBiOpTestCases() {
            var all = new[] { true, false };

            foreach (var op in BooleanBiOpList) {
                foreach (var opl in all) {
                    foreach (var opr in all) {
                        bool result;
                        unchecked {
                            result = op switch {
                                GlosOp.And => opl & opr,
                                GlosOp.Orr => opl | opr,
                                GlosOp.Xor => opl ^ opr,
                                GlosOp.Equ => opl == opr,
                                GlosOp.Neq => opl ^ opr,
                                _ => throw new ArgumentOutOfRangeException()
                            };
                        }

                        yield return new object[] { op, opl, opr, result };
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetBooleanBiOpTestCases))]
        public void BooleanBiOp(GlosOp op, bool opl, bool opr, bool result) {
            var fgen = Builder.AddFunction();

            fgen.AppendLdBool(opl);
            fgen.AppendLdBool(opr);
            fgen.AppendInstruction(op);

            fgen.SetEntry();

            var res = Execute();

            GlosValueArrayChecker.Create(res)
                .FirstOne().AssertBoolean(result)
                .MoveNext().AssertEnd();
        }
    }
}
