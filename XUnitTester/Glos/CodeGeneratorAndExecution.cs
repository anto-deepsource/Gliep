using System;
using System.Collections.Generic;
using System.Globalization;

using GeminiLab.Core2;
using GeminiLab.Core2.Random;
using GeminiLab.Core2.Random.RNG;
using GeminiLab.Glos.CodeGenerator;
using GeminiLab.Glos.ViMa;

using Xunit;
using XUnitTester.Checker;

namespace XUnitTester.Glos {
    public class CodeGeneratorAndExecution : GlosTestBase {
        public static GlosOp[] BinaryOperations { get; } = new[] {
            GlosOp.Add, GlosOp.Sub, GlosOp.Mul, GlosOp.Div,
            GlosOp.Mod, GlosOp.Lsh, GlosOp.Rsh, GlosOp.And,
            GlosOp.Orr, GlosOp.Xor, GlosOp.Gtr, GlosOp.Lss,
            GlosOp.Geq, GlosOp.Leq, GlosOp.Equ, GlosOp.Neq,
        };

        public static IEnumerable<object[]> GetIntegerBinaryOperatorTestcases(int count) {
            GlosOp op;
            long opl, opr, result;
            var opc = BinaryOperations.MakeChooser();
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
        [MemberData(nameof(GetIntegerBinaryOperatorTestcases), 2048)]
        public void BinaryOperatorTest(GlosOp op, long opl, long opr, long result) {
            var fgen = Builder.AddFunction();

            fgen.AppendLd(opl);
            fgen.AppendLd(opr);
            fgen.AppendInstruction(op);

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            var checker = GlosValueArrayChecker.Create(res).First();

            if (op == GlosOp.Gtr || op == GlosOp.Lss || op == GlosOp.Geq || op == GlosOp.Leq || op == GlosOp.Equ || op == GlosOp.Neq) {
                checker.AssertBoolean(result != 0);
            } else {
                checker.AssertInteger(result);
            }

            checker.MoveNext().AssertEnd();
        }



        public static GlosOp[] BooleanBinaryOperations { get; } = new[] {
            GlosOp.And,  GlosOp.Orr, GlosOp.Xor, GlosOp.Equ, GlosOp.Neq,
        };

        public static IEnumerable<object[]> GetBooleanBinaryOperatorTestcases() {
            var all = new[] { true, false };

            foreach (var op in BooleanBinaryOperations) {
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
        [MemberData(nameof(GetBooleanBinaryOperatorTestcases))]
        public void BooleanBinaryOperatorTest(GlosOp op, bool opl, bool opr, bool result) {
            var fgen = Builder.AddFunction();

            fgen.AppendLdBool(opl);
            fgen.AppendLdBool(opr);
            fgen.AppendInstruction(op);

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertBoolean(result)
                .MoveNext().AssertEnd();
        }



        [Fact]
        public void NumericArithmeticOp() {
            var fgen = Builder.AddFunction();
            var a = fgen.AllocateLocalVariable();
            var b = fgen.AllocateLocalVariable();
            var c = fgen.AllocateLocalVariable();

            fgen.AppendLd(0x1248);
            fgen.AppendStLoc(a);
            fgen.AppendLdFltRaw(0x40b2480000000000);
            fgen.AppendStLoc(b);
            fgen.AppendLdFltRaw(0x40b2480000000001);
            fgen.AppendStLoc(c);

            fgen.AppendLdDel();

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendAdd(); // expected: 9360.0
            fgen.AppendLdLoc(c);
            fgen.AppendLdLoc(b);
            fgen.AppendAdd(); // expected: 9360.0

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendSub(); // expected: 0.0
            fgen.AppendLdLoc(c);
            fgen.AppendLdLoc(b);
            fgen.AppendSub(); // expected: 0x3d70000000000000 as float / 9.0949470177292824E-13

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendMul(); // expected: 21902400.0
            fgen.AppendLdLoc(c);
            fgen.AppendLdLoc(b);
            fgen.AppendMul(); // expected: 0x4174e34400000001 as float / 21902400.000000004

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendDiv(); // expected: 1.0
            fgen.AppendLdLoc(c);
            fgen.AppendLdLoc(b);
            fgen.AppendDiv(); // expected: 0x3ff0000000000001 as float / 1.0000000000000002

            fgen.AppendLdLoc(b);
            fgen.AppendNeg(); // expected: -4680.0

            fgen.AppendRet();

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertFloat(9360.0)
                .MoveNext().AssertFloat(9360.0)
                .MoveNext().AssertFloat(0.0)
                .MoveNext().AssertFloat(0x3d70000000000000ul)
                .MoveNext().AssertFloat(21902400.0)
                .MoveNext().AssertFloat(0x4174e34400000001ul)
                .MoveNext().AssertFloat(1.0)
                .MoveNext().AssertFloat(0x3ff0000000000001ul)
                .MoveNext().AssertFloat(-4680.0)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void StringLoadAndOp() {
            var s1 = "str1";
            var s2 = "字符串";

            var fgen = Builder.AddFunction();
            var a = fgen.AllocateLocalVariable();
            var b = fgen.AllocateLocalVariable();

            fgen.AppendLdStr(s1);
            fgen.AppendStLoc(a);
            fgen.AppendLdStr(s2);
            fgen.AppendStLoc(b);

            // remove to test default return delimiter
            // fgen.AppendLdDel();

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendAdd();

            fgen.AppendRet();

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertString(s1)
                .MoveNext().AssertString(s1 + s2)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void FunctionCall() {
            var inc = Builder.AddFunction();

            inc.AppendLdDel(); // duplicate LdDel to test whether ViMa clear del stack of callee correctly.
            inc.AppendLdDel();
            inc.AppendLdDel();
            inc.AppendLdArg(0);
            inc.AppendLd(1);
            inc.AppendAdd();
            inc.AppendLd(1);
            inc.AppendRet();

            var fgen = Builder.AddFunction();

            var incLoc = fgen.AllocateLocalVariable();

            fgen.AppendLdFun(inc);
            // it should be ok here even if we do not bind it, test it.
            // fgen.AppendBind();
            fgen.AppendStLoc(incLoc);

            fgen.AppendLdDel();

            fgen.AppendLdDel();
            fgen.AppendLd(1);
            fgen.AppendLdLoc(incLoc);
            fgen.AppendCall();
            fgen.AppendShpRv(1);

            fgen.AppendLdDel();
            fgen.AppendLd(2);
            fgen.AppendLd(3);
            fgen.AppendLdLoc(incLoc);
            fgen.AppendCall();
            fgen.AppendShpRv(3);

            fgen.AppendRet();

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertInteger(2)
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertNil()
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void TableWithUenAndRenMetamethods() {
            var metaUen = Builder.AddFunction();

            metaUen.AppendLdArg(2);
            metaUen.AppendLd(1);
            metaUen.AppendAdd();
            metaUen.AppendLdArg(0);
            metaUen.AppendLdArg(1);
            metaUen.AppendUenL();
            metaUen.AppendRet();

            var metaRen = Builder.AddFunction();

            metaRen.AppendLdFalse();
            metaRen.AppendRet();

            var fgen = Builder.AddFunction();
            var at = fgen.AllocateLocalVariable();
            var a = fgen.AllocateLocalVariable();
            var bt = fgen.AllocateLocalVariable();
            var b = fgen.AllocateLocalVariable();
            var ct = fgen.AllocateLocalVariable();
            var c = fgen.AllocateLocalVariable();

            fgen.AppendLdNTbl();
            fgen.AppendStLoc(at);
            fgen.AppendLdNTbl();
            fgen.AppendStLoc(a);
            fgen.AppendLdNTbl();
            fgen.AppendStLoc(bt);
            fgen.AppendLdNTbl();
            fgen.AppendStLoc(b);
            fgen.AppendLdNTbl();
            fgen.AppendStLoc(ct);
            fgen.AppendLdNTbl();
            fgen.AppendStLoc(c);

            fgen.AppendLdFun(metaUen);
            fgen.AppendLdLoc(at);
            fgen.AppendLdStr(GlosMetamethodNames.Uen);
            fgen.AppendUenL();

            fgen.AppendLdFun(metaRen);
            fgen.AppendLdLoc(at);
            fgen.AppendLdStr(GlosMetamethodNames.Ren);
            fgen.AppendUenL();

            fgen.AppendLdFun(metaUen);
            fgen.AppendLdLoc(bt);
            fgen.AppendLdStr(GlosMetamethodNames.Uen);
            fgen.AppendUenL();

            fgen.AppendLdFun(metaRen);
            fgen.AppendLdLoc(ct);
            fgen.AppendLdStr(GlosMetamethodNames.Ren);
            fgen.AppendUenL();

            fgen.AppendLdLoc(at);
            fgen.AppendLdLoc(a);
            fgen.AppendSmt();

            fgen.AppendLdLoc(bt);
            fgen.AppendLdLoc(b);
            fgen.AppendSmt();

            fgen.AppendLdLoc(ct);
            fgen.AppendLdLoc(c);
            fgen.AppendSmt();

            fgen.AppendLd(0);
            fgen.AppendLdLoc(a);
            fgen.AppendLd(0);
            fgen.AppendUen();
            fgen.AppendLd(1);
            fgen.AppendLdLoc(a);
            fgen.AppendLd(1);
            fgen.AppendUenL();

            fgen.AppendLd(0);
            fgen.AppendLdLoc(b);
            fgen.AppendLd(0);
            fgen.AppendUen();
            fgen.AppendLd(1);
            fgen.AppendLdLoc(b);
            fgen.AppendLd(1);
            fgen.AppendUenL();

            fgen.AppendLd(0);
            fgen.AppendLdLoc(c);
            fgen.AppendLd(0);
            fgen.AppendUen();
            fgen.AppendLd(1);
            fgen.AppendLdLoc(c);
            fgen.AppendLd(1);
            fgen.AppendUenL();

            fgen.AppendLdDel();

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendLdLoc(c);

            fgen.AppendLdLoc(a);
            fgen.AppendLd(0);
            fgen.AppendRenL();
            fgen.AppendLdLoc(a);
            fgen.AppendLd(0);
            fgen.AppendRen();

            fgen.AppendLdLoc(b);
            fgen.AppendLd(0);
            fgen.AppendRenL();
            fgen.AppendLdLoc(b);
            fgen.AppendLd(0);
            fgen.AppendRen();

            fgen.AppendLdLoc(c);
            fgen.AppendLd(0);
            fgen.AppendRenL();
            fgen.AppendLdLoc(c);
            fgen.AppendLd(0);
            fgen.AppendRen();

            fgen.AppendRet();

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertTable(t => {
                    GlosTableChecker.Create(t)
                        .Has(0, v => v.AssertInteger() == 1)
                        .Has(1, v => v.AssertInteger() == 1)
                        .AssertAllKeyChecked();
                })
                .MoveNext().AssertTable(t => {
                    GlosTableChecker.Create(t)
                        .Has(0, v => v.AssertInteger() == 1)
                        .Has(1, v => v.AssertInteger() == 1)
                        .AssertAllKeyChecked();
                })
                .MoveNext().AssertTable(t => {
                    GlosTableChecker.Create(t)
                        .Has(0, v => v.AssertInteger() == 0)
                        .Has(1, v => v.AssertInteger() == 1)
                        .AssertAllKeyChecked();
                })
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertFalse()
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(0)
                .MoveNext().AssertFalse()
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void TableWithExternalMetamethods() {
            var fgen = Builder.AddFunction();
            var at = fgen.AllocateLocalVariable();
            var a = fgen.AllocateLocalVariable();

            fgen.AppendLdNTbl();
            fgen.AppendStLoc(at);
            fgen.AppendLdNTbl();
            fgen.AppendStLoc(a);

            fgen.AppendLdArg(0);
            fgen.AppendLdLoc(at);
            fgen.AppendLdStr(GlosMetamethodNames.Uen);
            fgen.AppendUenL();

            fgen.AppendLdArg(1);
            fgen.AppendLdLoc(at);
            fgen.AppendLdStr(GlosMetamethodNames.Ren);
            fgen.AppendUenL();

            fgen.AppendLdLoc(at);
            fgen.AppendLdLoc(a);
            fgen.AppendSmt();

            fgen.AppendLd(0);
            fgen.AppendLdLoc(a);
            fgen.AppendLd(3);
            fgen.AppendUen();

            fgen.AppendLdDel();
            fgen.AppendLdLoc(a);

            fgen.AppendLdLoc(a);
            fgen.AppendLdNil();
            fgen.AppendRen();

            fgen.AppendLdLoc(a);
            fgen.AppendGmt();

            fgen.AppendRet();

            GlosExternalFunction uen = arg => {
                var t = arg[0].AssertTable();
                var k = arg[1];
                var v = arg[2];

                t.UpdateEntryLocally(v, k);
                return Array.Empty<GlosValue>();
            };

            GlosExternalFunction ren = arg => {
                return new[] { arg[1] };
            };

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, new GlosValue[] { uen, ren });

            GlosValueArrayChecker.Create(res)
                .First().AssertTable(t => {
                    GlosTableChecker.Create(t)
                        .Has(0, v => v.AssertInteger() == 3)
                        .AssertAllKeyChecked();
                })
                .MoveNext().AssertNil()
                .MoveNext().AssertTable(t => {
                    GlosTableChecker.Create(t)
                        .Has(GlosMetamethodNames.Uen, v => v.AssertExternalFunction() == uen)
                        .Has(GlosMetamethodNames.Ren, v => v.AssertExternalFunction() == ren)
                        .AssertAllKeyChecked();
                })
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Closure() {
            var ctr = "ctr";

            var ctrLambda = Builder.AddFunction();
            var cnt = ctrLambda.AllocateLocalVariable();

            ctrLambda.AppendLdStr(ctr);
            ctrLambda.AppendRvc();
            ctrLambda.AppendDup();
            ctrLambda.AppendStLoc(cnt);
            ctrLambda.AppendLd(1);
            ctrLambda.AppendAdd();
            ctrLambda.AppendLdStr(ctr);
            ctrLambda.AppendUvc();
            ctrLambda.AppendLdDel();
            ctrLambda.AppendLdLoc(cnt);
            ctrLambda.AppendRet();

            var counter = Builder.AddFunction();
            counter.VariableInContext = new[] { ctr };

            counter.AppendLdArg(0);
            counter.AppendLdStr(ctr);
            counter.AppendUvc();
            counter.AppendLdDel();
            counter.AppendLdFun(ctrLambda);
            counter.AppendBind();
            counter.AppendRet();

            var fgen = Builder.AddFunction();
            var counterF = fgen.AllocateLocalVariable();
            var counterA = fgen.AllocateLocalVariable();
            var counterB = fgen.AllocateLocalVariable();

            fgen.AppendLdFun(counter);
            fgen.AppendBind();
            fgen.AppendStLoc(counterF);

            fgen.AppendLdDel();
            fgen.AppendLd(0);
            fgen.AppendLdLoc(counterF);
            fgen.AppendCall();
            fgen.AppendShpRv(1);
            fgen.AppendStLoc(counterA);

            fgen.AppendLdDel();
            fgen.AppendLd(1);
            fgen.AppendLdLoc(counterF);
            fgen.AppendCall();
            fgen.AppendShpRv(1);
            fgen.AppendStLoc(counterB);

            fgen.AppendLdDel();

            fgen.AppendLdDel();
            fgen.AppendLdLoc(counterA);
            fgen.AppendCall();
            fgen.AppendShpRv(1);

            fgen.AppendLdDel();
            fgen.AppendLdLoc(counterA);
            fgen.AppendCall();
            fgen.AppendShpRv(1);

            fgen.AppendLdDel();
            fgen.AppendLdLoc(counterB);
            fgen.AppendCall();
            fgen.AppendShpRv(1);

            fgen.AppendLdDel();
            fgen.AppendLdLoc(counterA);
            fgen.AppendCall();
            fgen.AppendShpRv(1);

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertInteger(0)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void GlobalEnvironment() {
            var idx = "idx";

            var sub = Builder.AddFunction();

            sub.AppendLdStr(idx);
            sub.AppendRvg();
            sub.AppendLd(1);
            sub.AppendAdd();
            sub.AppendLdStr(idx);
            sub.AppendUvg();
            sub.AppendRet();
            
            var fgen = Builder.AddFunction();
            var subLoc = fgen.AllocateLocalVariable();

            fgen.AppendLdFun(sub);
            fgen.AppendBind();
            fgen.AppendStLoc(subLoc);

            fgen.AppendLdDel();
            fgen.AppendLdLoc(subLoc);
            fgen.AppendCall();
            fgen.AppendShpRv(0);

            fgen.AppendLdStr(idx);
            fgen.AppendRvg();
            fgen.AppendLd(1);
            fgen.AppendAdd();
            fgen.AppendLdStr(idx);
            fgen.AppendUvg();

            fgen.AppendLdDel();
            fgen.AppendLdLoc(subLoc);
            fgen.AppendCall();
            fgen.AppendShpRv(0);

            fgen.AppendLdStr(idx);
            fgen.AppendRvg();
            fgen.AppendLd(1);
            fgen.AppendAdd();
            fgen.AppendLdStr(idx);
            fgen.AppendUvg();

            fgen.AppendLdDel();
            fgen.AppendLdStr(idx);
            fgen.AppendRvg();

            fgen.AppendRet();

            fgen.SetEntry();

            var global = new GlosContext(null);
            global.CreateVariable(idx, 0);

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>(), global);

            Assert.Equal(4, global.GetVariableReference(idx).AssertInteger());

            GlosValueArrayChecker.Create(res)
                .First().AssertInteger(4)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void LdArgATest() {
            var fgen = Builder.AddFunction();
            
            // this function return the value and index first negative integer of integer-only arguments
            var iter = fgen.AllocateLocalVariable();

            fgen.AppendLd(0);
            fgen.AppendStLoc(iter);

            var loop = fgen.AllocateAndInsertLabel();
            var ret = fgen.AllocateLabel();
            var failed = fgen.AllocateLabel();

            fgen.AppendLdLoc(iter);
            fgen.AppendLdArgc();
            fgen.AppendLss();
            fgen.AppendBf(failed);
            fgen.AppendLdLoc(iter);
            fgen.AppendDup();
            fgen.AppendLdArgA();
            fgen.AppendDup();
            fgen.AppendLd(0);
            fgen.AppendLss();
            fgen.AppendBt(ret);
            fgen.AppendPop();
            fgen.AppendLd(1);
            fgen.AppendAdd();
            fgen.AppendStLoc(iter);
            fgen.AppendB(loop);

            fgen.InsertLabel(failed);
            fgen.AppendLdNeg1();
            fgen.AppendLdNeg1();

            fgen.InsertLabel(ret);
            fgen.AppendRet();
            
            fgen.SetEntry();

            var ran = new Random();
            for (int i = 0; i < 1024; ++i) {
                var len = ran.Next(0, i * 16);

                var args = new List<GlosValue>();
                var list = new List<int>();

                for (int j = 0; j < len; ++j) {
                    var v = ran.Next(int.MinValue, int.MaxValue);

                    args.Add(v);
                    list.Add(v);
                }

                var res = ViMa.ExecuteUnit(Unit, args.ToArray());
                var idx = list.FindIndex(x => x < 0);
                var val = idx >= 0 ? list[idx] : -1;

                GlosValueArrayChecker.Create(res)
                    .First().AssertInteger(idx)
                    .MoveNext().AssertInteger(val)
                    .MoveNext().AssertEnd();
            }
        }

        [Fact]
        public void MassiveLd() {
            var fgen = Builder.AddFunction();
            long expected = 0;

            unchecked {
                fgen.AppendLd(0);

                for (int i = 0; i < 64; ++i) {
                    for (int j = 0; j < 64; ++j) {
                        long v = (long)((1ul << i) | (1ul << j));

                        expected += v;
                        fgen.AppendLd(v);
                        fgen.AppendAdd();
                    }
                }
            }

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertInteger(expected)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void MassiveLdStrAndLdFun() {
            var fgen = Builder.AddFunction();
            string expected = "";

            unchecked {
                for (int i = 0; i < 1024; ++i) {
                    var s = i.ToString(CultureInfo.InvariantCulture);
                    expected += s;

                    var f = Builder.AddFunction();
                    f.AppendLdStr(s);

                    fgen.AppendLdDel();
                    fgen.AppendLdFun(f);
                    fgen.AppendCall();
                    fgen.AppendShpRv(1);
                    if (i != 0) fgen.AppendAdd();
                }
            }

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertString(expected)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Loop() {
            var fgen = Builder.AddFunction();
            var iter = fgen.AllocateLocalVariable();
            var max = fgen.AllocateLocalVariable();

            fgen.AppendLd(0);
            fgen.AppendStLoc(iter);
            fgen.AppendLdArg(0);
            fgen.AppendStLoc(max);

            fgen.AppendLdDel();

            var loopTag = fgen.AllocateAndInsertLabel();

            fgen.AppendLdLoc(iter);
            fgen.AppendLd(1);
            fgen.AppendAdd();
            fgen.AppendDup();
            fgen.AppendDup();
            fgen.AppendStLoc(iter);
            fgen.AppendLdLoc(max);
            fgen.AppendLss();
            fgen.AppendBt(loopTag);

            fgen.AppendRet();

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, new GlosValue[] { 1024 });

            var checker = GlosValueArrayChecker.Create(res);
            var it = checker.First();

            for (int i = 0; i < 1024; ++i) {
                it.AssertInteger(i + 1).MoveNext();
            }

            it.AssertEnd();
        }

        [Fact]
        public void MassiveLoc() {
            var size = 1024;
            var fgen = Builder.AddFunction();

            var list = new List<LocalVariable>();

            for (int i = 0; i < size; ++i) {
                var lv = fgen.AllocateLocalVariable();
                list.Add(lv);

                fgen.AppendLd(i);
                fgen.AppendStLoc(lv);
            }

            list.ForEach(fgen.AppendLdLoc);
            (size - 1).Times(fgen.AppendAdd);

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertInteger((size - 1) * size / 2)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Syscall() {
            // syscall 0: ldlocc
            ViMa.SetSyscall(0, (stack, callStack, delStack) => {
                stack.PushStack() = callStack.StackTop().Function.Prototype.LocalVariableSize;
            });

            // syscall 1: erase all locs
            ViMa.SetSyscall(1, (stack, callStack, delStack) => {
                foreach (ref var v in stack.AsSpan(callStack.StackTop().LocalVariablesBase, callStack.StackTop().Function.Prototype.LocalVariableSize)) {
                    v.SetNil();
                }
            });

            // syscall 2: set all locs to 1
            ViMa.SetSyscall(2, (stack, callStack, delStack) => {
                foreach (ref var v in stack.AsSpan(callStack.StackTop().LocalVariablesBase, callStack.StackTop().Function.Prototype.LocalVariableSize)) {
                    v.SetInteger(1);
                }
            });

            var locc = 4;
            var fgen = Builder.AddFunction();
            var locs = new LocalVariable[locc];
            for (int i = 0; i < locc; ++i) {
                fgen.AppendLd(i);
                fgen.AppendStLoc(locs[i] = fgen.AllocateLocalVariable());
            }

            fgen.AppendLdDel();
            fgen.AppendSyscall(0);
            for (int i = 0; i < locc; ++i) {
                fgen.AppendLdLoc(locs[i]);
            }
            fgen.AppendSyscall(1);
            for (int i = 0; i < locc; ++i) {
                fgen.AppendLdLoc(locs[i]);
            }
            fgen.AppendSyscall(2);
            for (int i = 0; i < locc; ++i) {
                fgen.AppendLdLoc(locs[i]);
            }

            fgen.AppendRet();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());
            var checker = GlosValueArrayChecker.Create(res).First();

            checker.AssertInteger(locc).MoveNext();
            for (int i = 0; i < locc; ++i) {
                checker.AssertInteger(i).MoveNext();
            }

            for (int i = 0; i < locc; ++i) {
                checker.AssertNil().MoveNext();
            }

            for (int i = 0; i < locc; ++i) {
                checker.AssertInteger(1).MoveNext();
            }

            checker.AssertEnd();
        }

        [Fact]
        public void Typeof() {
            var fgen = Builder.AddFunction();

            fgen.AppendLdNil();
            fgen.AppendTypeof();
            fgen.AppendLd(1);
            fgen.AppendTypeof();

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertString(nameof(GlosValueType.Nil).ToLowerInvariant())
                .MoveNext().AssertString(nameof(GlosValueType.Integer).ToLowerInvariant())
                .MoveNext().AssertEnd();
        }
    }
}
