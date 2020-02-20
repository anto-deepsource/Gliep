using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GeminiLab.Glos.ViMa;
using Xunit;
using XUnitTester.Checker;

namespace XUnitTester {
    public class CodeGeneratorTest : GlosTestBase {
        [Fact]
        public void LdTest() {
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
        public void LdStrAndLdFunTest() {
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
        public void LoopTest() {
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

        /*
        [Fact]
        public void LoopTest() {
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

            var res = ViMa.ExecuteFunction(new GlosFunction(Unit.FunctionTable[fgen.Id], new GlosTable(ViMa)), new GlosValue[] { 1024 });

            var checker = GlosValueArrayChecker.Create(res);
            var it = checker.First();

            for (int i = 0; i < 1024; ++i) {
                it.AssertInteger(i + 1).MoveNext();
            }

            it.AssertEnd();
        }
        */
    }
}
