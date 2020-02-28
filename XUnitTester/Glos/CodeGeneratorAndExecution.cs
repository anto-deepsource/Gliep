using System;
using System.Collections.Generic;
using System.Globalization;
using GeminiLab.Core2;
using GeminiLab.Glos.CodeGenerator;
using GeminiLab.Glos.ViMa;
using Xunit;
using XUnitTester.Checker;

namespace XUnitTester.Glos {
    public class CodeGeneratorAndExecution : GlosTestBase {
        [Fact]
        public void IntegerArithmeticOp() {
            var fgen = Builder.AddFunction();
            var locs = new LocalVariable[5] {
                fgen.AllocateLocalVariable(), fgen.AllocateLocalVariable(), fgen.AllocateLocalVariable(),
                fgen.AllocateLocalVariable(), fgen.AllocateLocalVariable()
            };
            
            fgen.AppendLd(0xefcdab8967452301);
            fgen.AppendStLoc(locs[0]);
            fgen.AppendLd(0x0123456789abcdef);
            fgen.AppendStLoc(locs[1]);
            fgen.AppendLd(-1);
            fgen.AppendStLoc(locs[2]);
            fgen.AppendLd(0x11111111);
            fgen.AppendStLoc(locs[3]);
            fgen.AppendLd(2);
            fgen.AppendStLoc(locs[4]);

            fgen.AppendLdDel();

            fgen.AppendLdLoc(locs[3]);
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendAdd(); // expected: 0x0000000022222222
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendAdd(); // expected: 0xdf9b5712ce8a4602
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendLdLoc(locs[1]);
            fgen.AppendAdd(); // expected: 0xf0f0f0f0f0f0f0f0

            fgen.AppendLdLoc(locs[2]);
            fgen.AppendLdLoc(locs[2]);
            fgen.AppendSub(); // expected: 0x0000000000000000
            fgen.AppendLdLoc(locs[1]);
            fgen.AppendLdLoc(locs[2]);
            fgen.AppendSub(); // expected: 0x0123456789abcdf0
            fgen.AppendLdLoc(locs[1]);
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendSub(); // expected: 0x01234567789abcde

            fgen.AppendLdLoc(locs[3]);
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendMul(); // expected: 0x0123456787654321
            fgen.AppendLdLoc(locs[1]);
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendMul(); // expected: 0xa9cf824ab13e7aef
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendLdLoc(locs[2]);
            fgen.AppendMul(); // expected: 0xffffffffeeeeeeef

            fgen.AppendLdLoc(locs[1]);
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendDiv(); // expected: 0x0000000011111111
            fgen.AppendLdLoc(locs[2]);
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendDiv(); // expected: 0x0000000000000000
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendDiv(); // expected: 0xffffffff0d0d0d0d

            fgen.AppendLdLoc(locs[1]);
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendMod(); // expected: 0x0000000002468ace
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendMod(); // expected: 0xfffffffff0ac6824
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendLdLoc(locs[4]);
            fgen.AppendMod(); // expected: 0xffffffffffffffff

            fgen.AppendLdLoc(locs[2]);
            fgen.AppendNeg(); // expected: 0x0000000000000001
            fgen.AppendLd(long.MinValue);
            fgen.AppendNeg(); // expected: 0x8000000000000000

            fgen.AppendRet();

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            unchecked {
                GlosValueArrayChecker.Create(res)
                    .First().AssertInteger((long)0x0000000022222222ul)
                    .MoveNext().AssertInteger((long)0xdf9b5712ce8a4602ul)
                    .MoveNext().AssertInteger((long)0xf0f0f0f0f0f0f0f0ul)
                    .MoveNext().AssertInteger((long)0x0000000000000000ul)
                    .MoveNext().AssertInteger((long)0x0123456789abcdf0ul)
                    .MoveNext().AssertInteger((long)0x01234567789abcdeul)
                    .MoveNext().AssertInteger((long)0x0123456787654321ul)
                    .MoveNext().AssertInteger((long)0xa9cf824ab13e7aeful)
                    .MoveNext().AssertInteger((long)0xffffffffeeeeeeeful)
                    .MoveNext().AssertInteger((long)0x0000000011111111ul)
                    .MoveNext().AssertInteger((long)0x0000000000000000ul)
                    .MoveNext().AssertInteger((long)0xffffffff0d0d0d0dul)
                    .MoveNext().AssertInteger((long)0x0000000002468aceul)
                    .MoveNext().AssertInteger((long)0xfffffffff0ac6824ul)
                    .MoveNext().AssertInteger((long)0xfffffffffffffffful)
                    .MoveNext().AssertInteger((long)0x0000000000000001ul)
                    .MoveNext().AssertInteger((long)0x8000000000000000ul)
                    .MoveNext().AssertEnd();
            }
        }

        [Fact]
        public void IntegerBitwiseOp() {
            var fgen = Builder.AddFunction();
            var locs = new LocalVariable[5] {
                fgen.AllocateLocalVariable(), fgen.AllocateLocalVariable(), fgen.AllocateLocalVariable(),
                fgen.AllocateLocalVariable(), fgen.AllocateLocalVariable()
            };

            fgen.AppendLd(0xefcdab8967452301);
            fgen.AppendStLoc(locs[0]);
            fgen.AppendLd(0x0123456789abcdef);
            fgen.AppendStLoc(locs[1]);
            fgen.AppendLd(-1);
            fgen.AppendStLoc(locs[2]);
            fgen.AppendLd(0x11111111);
            fgen.AppendStLoc(locs[3]);
            fgen.AppendLd(2);
            fgen.AppendStLoc(locs[4]);

            fgen.AppendLdDel();

            fgen.AppendLdLoc(locs[3]);
            fgen.AppendLdLoc(locs[4]);
            fgen.AppendLsh(); // expected: 0x0000000044444444
            fgen.AppendLdLoc(locs[3]);
            fgen.AppendLdLoc(locs[4]);
            fgen.AppendRsh(); // expected: 0x0000000004444444
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendLdLoc(locs[1]);
            fgen.AppendAnd(); // expected: 0x0101010101010101
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendLdLoc(locs[1]);
            fgen.AppendOrr(); // expected: 0xefefefefefefefef
            fgen.AppendLdLoc(locs[0]);
            fgen.AppendLdLoc(locs[1]);
            fgen.AppendXor(); // expected: 0xeeeeeeeeeeeeeeee
            fgen.AppendLdLoc(locs[2]);
            fgen.AppendNot(); // expected: 0x0000000000000000

            fgen.AppendRet();

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            unchecked {
                GlosValueArrayChecker.Create(res)
                    .First().AssertInteger((long)0x0000000044444444ul)
                    .MoveNext().AssertInteger((long)0x0000000004444444ul)
                    .MoveNext().AssertInteger((long)0x0101010101010101ul)
                    .MoveNext().AssertInteger((long)0xefefefefefefefeful)
                    .MoveNext().AssertInteger((long)0xeeeeeeeeeeeeeeeeul)
                    .MoveNext().AssertInteger((long)0x0000000000000000ul)
                    .MoveNext().AssertEnd();
            }
        }

        [Fact]
        public void IntegerComparisonOp() {
            var fgen = Builder.AddFunction();
            var a = fgen.AllocateLocalVariable();
            var b = fgen.AllocateLocalVariable();

            fgen.AppendLd(0xefcdab8967452301);
            fgen.AppendStLoc(a);
            fgen.AppendLd(0x123456789abcdef);
            fgen.AppendStLoc(b);

            fgen.AppendLdDel();

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(a);
            fgen.AppendGtr(); // F
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(a);
            fgen.AppendLss(); // F
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(a);
            fgen.AppendGeq(); // T
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(a);
            fgen.AppendLeq(); // T
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(a);
            fgen.AppendEqu(); // T
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(a);
            fgen.AppendNeq(); // F

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendGtr(); // F
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendLss(); // T
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendGeq(); // F
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendLeq(); // T
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendEqu(); // F
            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(b);
            fgen.AppendNeq(); // T

            fgen.AppendRet();

            fgen.SetEntry();

            var res = ViMa.ExecuteUnit(Unit, Array.Empty<GlosValue>());

            GlosValueArrayChecker.Create(res)
                .First().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertTrue()
                .MoveNext().AssertTrue()
                .MoveNext().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertFalse()
                .MoveNext().AssertTrue()
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

            metaUen.AppendLdArg(0);
            metaUen.AppendLdArg(1);
            metaUen.AppendLdArg(2);
            metaUen.AppendLd(1);
            metaUen.AppendAdd();
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

            fgen.AppendLdLoc(at);
            fgen.AppendLdStr(GlosMetamethodNames.Uen);
            fgen.AppendLdFun(metaUen);
            fgen.AppendUenL();

            fgen.AppendLdLoc(at);
            fgen.AppendLdStr(GlosMetamethodNames.Ren);
            fgen.AppendLdFun(metaRen);
            fgen.AppendUenL();

            fgen.AppendLdLoc(bt);
            fgen.AppendLdStr(GlosMetamethodNames.Uen);
            fgen.AppendLdFun(metaUen);
            fgen.AppendUenL();

            fgen.AppendLdLoc(ct);
            fgen.AppendLdStr(GlosMetamethodNames.Ren);
            fgen.AppendLdFun(metaRen);
            fgen.AppendUenL();

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(at);
            fgen.AppendSmt();
            fgen.AppendPop();

            fgen.AppendLdLoc(b);
            fgen.AppendLdLoc(bt);
            fgen.AppendSmt();
            fgen.AppendPop();

            fgen.AppendLdLoc(c);
            fgen.AppendLdLoc(ct);
            fgen.AppendSmt();
            fgen.AppendPop();

            fgen.AppendLdLoc(a);
            fgen.AppendLd(0);
            fgen.AppendLd(0);
            fgen.AppendUen();
            fgen.AppendLdLoc(a);
            fgen.AppendLd(1);
            fgen.AppendLd(1);
            fgen.AppendUenL();

            fgen.AppendLdLoc(b);
            fgen.AppendLd(0);
            fgen.AppendLd(0);
            fgen.AppendUen();
            fgen.AppendLdLoc(b);
            fgen.AppendLd(1);
            fgen.AppendLd(1);
            fgen.AppendUenL();

            fgen.AppendLdLoc(c);
            fgen.AppendLd(0);
            fgen.AppendLd(0);
            fgen.AppendUen();
            fgen.AppendLdLoc(c);
            fgen.AppendLd(1);
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

            fgen.AppendLdLoc(at);
            fgen.AppendLdStr(GlosMetamethodNames.Uen);
            fgen.AppendLdArg(0);
            fgen.AppendUenL();

            fgen.AppendLdLoc(at);
            fgen.AppendLdStr(GlosMetamethodNames.Ren);
            fgen.AppendLdArg(1);
            fgen.AppendUenL();

            fgen.AppendLdLoc(a);
            fgen.AppendLdLoc(at);
            fgen.AppendSmt();
            fgen.AppendPop();

            fgen.AppendLdLoc(a);
            fgen.AppendLd(3);
            fgen.AppendLd(0);
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
            ctrLambda.AppendDup();
            ctrLambda.AppendRvc();
            ctrLambda.AppendDup();
            ctrLambda.AppendStLoc(cnt);
            ctrLambda.AppendLd(1);
            ctrLambda.AppendAdd();
            ctrLambda.AppendUvc();
            ctrLambda.AppendLdDel();
            ctrLambda.AppendLdLoc(cnt);
            ctrLambda.AppendRet();

            var counter = Builder.AddFunction();
            counter.VariableInContext = new[] { ctr };

            counter.AppendLdStr(ctr);
            counter.AppendLdArg(0);
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
            sub.AppendDup();
            sub.AppendRvg();
            sub.AppendLd(1);
            sub.AppendAdd();
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
            fgen.AppendDup();
            fgen.AppendRvg();
            fgen.AppendLd(1);
            fgen.AppendAdd();
            fgen.AppendUvg();

            fgen.AppendLdDel();
            fgen.AppendLdLoc(subLoc);
            fgen.AppendCall();
            fgen.AppendShpRv(0);

            fgen.AppendLdStr(idx);
            fgen.AppendDup();
            fgen.AppendRvg();
            fgen.AppendLd(1);
            fgen.AppendAdd();
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
    }
}
