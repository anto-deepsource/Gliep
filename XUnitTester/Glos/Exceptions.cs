using GeminiLab.Glos;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glos {
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

        [Fact]
        public void InvalidUnOperand() {
            var fgen = Builder.AddFunction();

            fgen.AppendLdStr("");
            fgen.AppendNeg();

            fgen.SetEntry();

            var exception = GlosRuntimeExceptionCatcher.Catch<GlosInvalidUnaryOperandTypeException>(() => {
                Execute();
            });

            Assert.Equal(GlosOp.Neg, exception.Op);
            Assert.Equal(GlosValueType.String, exception.Operand.Type);
            Assert.Equal("", exception.Operand.AssumeString());

            fgen = Builder.AddFunction();

            fgen.AppendLdStr("");
            fgen.AppendNot();

            fgen.SetEntry();

            exception = GlosRuntimeExceptionCatcher.Catch<GlosInvalidUnaryOperandTypeException>(() => {
                Execute();
            });

            Assert.Equal(GlosOp.Not, exception.Op);
            Assert.Equal(GlosValueType.String, exception.Operand.Type);
            Assert.Equal("", exception.Operand.AssumeString());
        }

        [Fact]
        public void IllLdStr() {
            var fgen = Builder.AddFunction();

            var invalidIndex = Builder.StringCount + 100;
            fgen.AppendLdStr(invalidIndex);
            fgen.AppendRet();

            fgen.SetEntry();

            var exception = GlosRuntimeExceptionCatcher.Catch<GlosStringIndexOutOfRangeException>(() => {
                Execute();
            });
            
            Assert.Equal(invalidIndex, exception.Index);
        }
        
        [Fact]
        public void IllLdFun() {
            var fgen = Builder.AddFunction();

            var invalidIndex = Builder.FunctionCount + 100;
            fgen.AppendLdFun(invalidIndex);
            fgen.AppendRet();

            fgen.SetEntry();

            var exception = GlosRuntimeExceptionCatcher.Catch<GlosFunctionIndexOutOfRangeException>(() => {
                Execute();
            });
            
            Assert.Equal(invalidIndex, exception.Index);
        }
    }
}
