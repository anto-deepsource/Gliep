using System;
using System.Collections.Generic;
using System.Globalization;
using GeminiLab.Core2;
using GeminiLab.Glos.CodeGenerator;
using GeminiLab.Glos.ViMa;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;
using XUnitTester.Checker;

namespace XUnitTester.Glos {
    public class CodeGeneratorTest : GlosTestBase {
        [Fact]
        public void Ld() {
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
        public void LdStrAndLdFun() {
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
        public void Loc() {
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
    }
}
