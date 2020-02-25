using System;
using System.Collections.Generic;
using System.Text;
using GeminiLab.Glos;
using GeminiLab.Glos.ViMa;
using Xunit;

namespace XUnitTester.Glos {
    public class ExceptionsTest : GlosTestBase {
        [Fact]
        public void BadBranch() {
            var fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.BS,
                (byte)42,
                (byte)GlosOp.LdDel,
                (byte)GlosOp.Ld0,
                (byte)GlosOp.Ret,
            }, 0);

            Builder.Entry = fun;

            var exception = Assert.Throws<GlosInvalidProgramCounterException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });

            Assert.Equal(44, exception.ProgramCounter);
        }

        [Fact]
        public void UnexpectedEndOfCode() {
            var fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.LdQ,
                (byte)0,
                (byte)0,
                (byte)0,
            }, 0);

            Builder.Entry = fun;

            Assert.Throws<GlosUnexpectedEndOfCodeException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });
        }

        [Fact]
        public void LocalVariableOutOfRange() {
            var fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.LdLoc0,
            }, 0);

            Builder.Entry = fun;

            var exception = Assert.Throws<GlosLocalVariableIndexOutOfRangeException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });

            Assert.Equal(0, exception.Index);

            fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.StLoc1,
            }, 0);

            Builder.Entry = fun;

            exception = Assert.Throws<GlosLocalVariableIndexOutOfRangeException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });

            Assert.Equal(1, exception.Index);
        }

        [Fact]
        public void UnknownOpException() {
            var fun = Builder.AddFunctionRaw(new[] {
                (byte)0xff, // TODO: mark op 0xff reserved for invalid
            }, 0);

            Builder.Entry = fun;

            var exception = Assert.Throws<GlosUnknownOpException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });

            Assert.Equal((byte)0xff, exception.Op);
        }
    }
}
