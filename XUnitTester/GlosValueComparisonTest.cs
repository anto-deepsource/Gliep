using System;
using System.Collections.Generic;
using System.Text;
using GeminiLab.Glos.ViMa;
using Xunit;

namespace XUnitTester {
    public class GlosValueComparisonTest : GlosTestBase {
        private readonly GlosValue.Comparer _cmp;

        public GlosValueComparisonTest() {
            _cmp = new GlosValue.Comparer(ViMa);
        }

        [Fact]
        public void ComparisonTest() {
            var a = new GlosTable(ViMa);
            var at = new GlosTable(ViMa);

            at.UpdateEntryLocally(GlosMetamethodNames.Equ, GlosValue.NewExternalFunction(values => new GlosValue[] { false }));
            at.UpdateEntryLocally(GlosMetamethodNames.Lss, GlosValue.NewExternalFunction(values => new GlosValue[] { false }));
            a.Metatable = at;

            var vals = new GlosValue[] {
                GlosValue.NewNil(),
                0,
                1.0,
                true,

                a,
                at,
                "123",
                GlosValue.NewExternalFunction(values => new GlosValue[] { false }),
            };

            var valLen = vals.Length;

            for (int i = 0; i < valLen; ++i) {
                for (int j = 0; j < valLen; ++j) {
                    ref var x = ref vals[i];
                    ref var y = ref vals[j];

                    if (i == j) {
                        if (x.Type == GlosValueType.Table && x.AssertTable().Metatable != null) {
                            Assert.False(_cmp.EqualTo(x, y));
                        } else {
                            Assert.True(_cmp.EqualTo(x, y));
                        }

                        if (x.Type == GlosValueType.Integer || x.Type == GlosValueType.Float ||
                            x.Type == GlosValueType.String || (x.Type == GlosValueType.Table && x.AssertTable().Metatable != null)) {
                            Assert.False(_cmp.LessThan(x, y));
                            Assert.False(_cmp.GreaterThan(x, y));
                        } else {
                            Assert.ThrowsAny<Exception>(() => _cmp.LessThan(vals[i], vals[j]));
                            Assert.ThrowsAny<Exception>(() => _cmp.GreaterThan(vals[i], vals[j]));
                        }
                    } else {
                        Assert.False(_cmp.EqualTo(vals[i], vals[j]));

                        if ((x.Type == GlosValueType.Integer || x.Type == GlosValueType.Float) &&
                            (y.Type == GlosValueType.Integer || y.Type == GlosValueType.Float)) {
                            double xv = x.Type == GlosValueType.Integer ? x.AssertInteger() : x.AssertFloat();
                            double yv = y.Type == GlosValueType.Integer ? y.AssertInteger() : y.AssertFloat();

                            Assert.Equal(xv < yv, _cmp.LessThan(x, y));
                            Assert.Equal(xv > yv, _cmp.GreaterThan(x, y));
                        } else if ((x.Type == GlosValueType.Table && x.AssertTable().Metatable != null) ||
                                   (y.Type == GlosValueType.Table && y.AssertTable().Metatable != null)) {
                            Assert.False(_cmp.LessThan(x, y));
                            Assert.False(_cmp.GreaterThan(x, y));
                        } else {
                            Assert.ThrowsAny<Exception>(() => _cmp.LessThan(vals[i], vals[j]));
                            Assert.ThrowsAny<Exception>(() => _cmp.GreaterThan(vals[i], vals[j]));
                        }
                    }

                    Assert.True(_cmp.EqualTo(vals[i], vals[j]) ^ _cmp.UnequalTo(vals[i], vals[j]));
                }
            }
        }
    }
}
