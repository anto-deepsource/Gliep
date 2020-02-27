using System;
using System.Collections.Generic;
using System.Text;
using GeminiLab.Glos;
using GeminiLab.Glos.ViMa;
using Xunit;

namespace XUnitTester.Glos {
    public class ExceptionsTest : GlosTestBase {
        // TODO: a custom exception assert mechanic
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

            var exception = Assert.Throws<GlosRuntimeException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });

            // Assert.Equal(44, exception.InstructionPointer);
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

            Assert.Throws<GlosRuntimeException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });
        }

        [Fact]
        public void LocalVariableOutOfRange() {
            var fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.LdLoc0,
            }, 0);

            Builder.Entry = fun;

            var exception = Assert.Throws<GlosRuntimeException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });

            // Assert.Equal(0, exception.Index);

            fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.StLoc1,
            }, 0);

            Builder.Entry = fun;

            exception = Assert.Throws<GlosRuntimeException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });

            // Assert.Equal(1, exception.Index);
        }

        [Fact]
        public void UnknownOpException() {
            var fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.Invalid,
            }, 0);

            Builder.Entry = fun;

            var exception = Assert.Throws<GlosRuntimeException>(() => {
                ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            });

            // Assert.Equal((byte)0xff, exception.Op);
        }
    }
}
