using System;

using GeminiLab.Glos;
using GeminiLab.Glos.ViMa;

using Xunit;
using XUnitTester.Misc;

namespace XUnitTester.Glos {
    public class Exceptions : GlosTestBase {
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

            var exception = GlosRuntimeExceptionCatcher.Catch<GlosInvalidInstructionPointerException>(() => {
                Execute();
            });

            Assert.Equal(44, ViMa.CallStackFrames[^1].InstructionPointer);
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

            GlosRuntimeExceptionCatcher.Catch<GlosUnexpectedEndOfCodeException>(() => {
                Execute();
            });
        }

        [Fact]
        public void LocalVariableOutOfRange() {
            var fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.LdLoc0,
            }, 0);

            Builder.Entry = fun;

            var exception = GlosRuntimeExceptionCatcher.Catch<GlosLocalVariableIndexOutOfRangeException>(() => {
                Execute();
            });

            Assert.Equal(0, exception.Index);

            fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.StLoc1,
            }, 0);

            Builder.Entry = fun;

            exception = GlosRuntimeExceptionCatcher.Catch<GlosLocalVariableIndexOutOfRangeException>(() => {
                Execute();
            });

            Assert.Equal(1, exception.Index);
        }

        [Fact]
        public void UnknownOp() {
            var fun = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.Invalid,
            }, 0);

            Builder.Entry = fun;

            var exception = GlosRuntimeExceptionCatcher.Catch<GlosUnknownOpException>(() => {
                Execute();
            });

            Assert.Equal((byte)GlosOp.Invalid, exception.Op);
        }

        [Fact]
        public void NotCallable() {
            var fgen = Builder.AddFunction();

            fgen.AppendLdDel();
            fgen.AppendLd(1);
            fgen.AppendCall();

            fgen.SetEntry();

            var exception = GlosRuntimeExceptionCatcher.Catch<GlosValueNotCallableException>(() => {
                Execute();
            });

            Assert.Equal(GlosValueType.Integer, exception.Value.Type);
            Assert.Equal(1, exception.Value.AssumeInteger());
        }

        [Fact]
        public void AssertionFailed() {
            var fgen = Builder.AddFunction();

            fgen.AppendLd(1);
            fgen.AppendGmt();
            fgen.AppendRet();

            fgen.SetEntry();

            var exception = GlosRuntimeExceptionCatcher.Catch<GlosValueTypeAssertionFailedException>(() => {
                Execute();
            });

            Assert.Equal(GlosValueType.Table, exception.Expected);
            Assert.Equal(GlosValueType.Integer, exception.Value.Type);
            Assert.Equal(1, exception.Value.AssumeInteger());
        }
    }
}
