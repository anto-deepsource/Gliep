using System;
using GeminiLab.Glos.ViMa;
using Xunit;
using XUnitTester.Checker;

namespace XUnitTester.Glos {
    public class BasicExecutionTest : GlosTestBase {
        [Fact]
        public void Return0() {
            var fid = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.Ld0,
                // (byte)GlosOp.Ret, // Test default return statement
            }, 0);

            Builder.Entry = fid;

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertInteger(0)
                .MoveNext().AssertEnd();
        }

        
        [Fact]
        public void IntegerLoadings() {
            var fid = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.LdDel,
                (byte)GlosOp.Ld1,
                (byte)GlosOp.Ld3,
                (byte)GlosOp.LdS,
                (byte)0xfe,
                (byte)GlosOp.Ld,
                (byte)0x78,
                (byte)0x56,
                (byte)0x34,
                (byte)0x12,
                (byte)GlosOp.LdQ,
                (byte)0x01,
                (byte)0x23,
                (byte)0x45,
                (byte)0x67,
                (byte)0x89,
                (byte)0xab,
                (byte)0xcd,
                (byte)0xef,
                (byte)GlosOp.LdNeg1,
                (byte)GlosOp.Ret,
            }, 0);

            Builder.Entry = fid;

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            unchecked {
                GlosValueArrayChecker.Create(res)
                    .First().AssertInteger((long)0x0000000000000001ul)
                    .MoveNext().AssertInteger((long)0x0000000000000003ul)
                    .MoveNext().AssertInteger((long)0xfffffffffffffffeul)
                    .MoveNext().AssertInteger((long)0x0000000012345678ul)
                    .MoveNext().AssertInteger((long)0xefcdab8967452301ul)
                    .MoveNext().AssertInteger((long)0xfffffffffffffffful)
                    .MoveNext().AssertEnd();
            }
        }

        [Fact]
        public void StackOperations() {
            var fid = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.LdDel,
                (byte)GlosOp.Ld1,
                (byte)GlosOp.Dup,
                (byte)GlosOp.Ld2,
                (byte)GlosOp.Ld3,
                (byte)GlosOp.LdS,
                (byte)0x04,
                (byte)GlosOp.Pop,
                (byte)GlosOp.Pop,
                (byte)GlosOp.Dup,
                (byte)GlosOp.StLoc1,
                (byte)GlosOp.StLoc0,
                (byte)GlosOp.StLoc2,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.LdLoc1,
                (byte)GlosOp.LdLoc2,
                (byte)GlosOp.Ret,
            }, 3);

            Builder.Entry = fid;

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            unchecked {
                GlosValueArrayChecker.Create(res)
                    .First().AssertInteger((long)0x0000000000000001ul)
                    .MoveNext().AssertInteger((long)0x0000000000000002ul)
                    .MoveNext().AssertInteger((long)0x0000000000000002ul)
                    .MoveNext().AssertInteger((long)0x0000000000000001ul)
                    .MoveNext().AssertEnd();
            }
        }
        
        [Fact]
        public void OtherLoadings() {
            var fid = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.LdDel,
                (byte)GlosOp.LdNil,
                (byte)GlosOp.LdFlt,
                (byte)0x00,
                (byte)0x00,
                (byte)0x00,
                (byte)0x12,
                (byte)0xba,
                (byte)0x2c,
                (byte)0xe0,
                (byte)0x40,
                (byte)GlosOp.LdTrue,
                (byte)GlosOp.LdFalse,
                (byte)GlosOp.LdArgc,
                (byte)GlosOp.LdArg2,
                (byte)GlosOp.LdArg3,
                (byte)GlosOp.Ret,
            }, 0);

            Builder.Entry = fid;

            var res = ViMa.ExecuteUnit(Unit, new[] { GlosValue.NewNil(), GlosValue.NewNil(), GlosValue.NewBoolean(true) });

            GlosValueArrayChecker.Create(res)
                .First().AssertNil()
                .MoveNext().AssertFloat(0x40e02cba12000000ul)
                .MoveNext().AssertTrue()
                .MoveNext().AssertFalse()
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertTrue()
                .MoveNext().AssertNil()
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void TableOfInteger() {
            var fid = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.LdNTbl,
                (byte)GlosOp.StLoc0,
                (byte)GlosOp.LdDel,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.Ld0,
                (byte)GlosOp.Ld1,
                (byte)GlosOp.UenL,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.Ld1,
                (byte)GlosOp.Ld0,
                (byte)GlosOp.Uen,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.Ld0,
                (byte)GlosOp.RenL,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.Ld0,
                (byte)GlosOp.Ren,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.Ld3,
                (byte)GlosOp.RenL,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.Ld3,
                (byte)GlosOp.Ren,
                (byte)GlosOp.Ret,
            }, 1);

            Builder.Entry = fid;

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertTable(t => {
                    GlosTableChecker.Create(t).
                        Has(0, v => v.AssertInteger() == 1).
                        Has(1, v => v.AssertInteger() == 0).
                        AssertAllKeyChecked();
                })
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertNil()
                .MoveNext().AssertNil()
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void ExternalFunction() {
            var funName = "ext_fun";
            var str = Builder.AddOrGetString(funName);

            var fid = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.LdStr,
                (byte)(str & 0xff),
                (byte)((str >> 8) & 0xff),
                (byte)((str >> 16) & 0xff),
                (byte)((str >> 24) & 0xff),
                (byte)GlosOp.Rvg,
                (byte)GlosOp.StLoc0,

                (byte)GlosOp.LdDel,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.Call,
                (byte)GlosOp.ShpRv1,

                (byte)GlosOp.LdDel,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.Call,
                (byte)GlosOp.ShpRv1,

                (byte)GlosOp.Ret,
            }, 2);

            var global = new GlosContext(null);
            global.CreateVariable(funName, GlosValue.NewExternalFunction(args => new GlosValue[] { args.Length }));
            
            Builder.Entry = fid;

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>(), global);

            GlosValueArrayChecker.Create(res)
                .First().AssertInteger(0)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertEnd();
        }

        /*
        [Fact]
        public void Syscall() {
            var fid = Builder.AddFunctionRaw(new[] {
                (byte)GlosOp.Ld0,
                (byte)GlosOp.StLoc0,

                (byte)GlosOp.SysC0,
                (byte)GlosOp.SysC1, // calling to an invalid syscall should be equivalent to nop

                (byte)GlosOp.LdLoc0,
                (byte)GlosOp.LdLoc1,

                (byte)GlosOp.Ret,
            }, 2);

            ViMa.SetSyscall(0, (GlosValue[] stack, ref long sptr, GlosStackFrame[] callStack, ref long cptr) => {
                stack[callStack[cptr - 1].LocalVariablesBase].SetBoolean(true);
                stack[callStack[cptr - 1].LocalVariablesBase + 1].SetBoolean(false);
            });

            Builder.Entry = fid;

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertTrue()
                .MoveNext().AssertFalse()
                .MoveNext().AssertEnd();
        }
        */
    }
}
