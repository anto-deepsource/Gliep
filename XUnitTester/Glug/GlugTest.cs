using System;
using System.Collections.Generic;
using GeminiLab.Core2.Random;
using GeminiLab.Core2.Random.RNG;
using GeminiLab.Core2.Yielder;
using GeminiLab.Glos;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glug {
    public class GlugTest : GlugExecutionTestBase {

        [Fact]
        public void Fibonacci() {
            var code = @"
                fn fibo [x] if (x <= 1) x else fibo[x - 2] + fibo[x - 1];
                return [fibo[0], fibo[1], fibo[10]];
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(0)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(55)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void EightQueens() {
            var code = @"
                !ans = 0;

                fn dfs[r, c, md, ad] (
                    if (r >= 8) return ans = ans + 1;

                    nx = 0;
                    while (nx < 8) (
                        if ((c & (1 << nx)) == 0 & (md & (1 << (r - nx + 7))) == 0 & (ad & (1 << (r + nx))) == 0)
                            dfs[r + 1, c | (1 << nx), md | (1 << (r - nx + 7)), ad | (1 << (r + nx))];
                        nx = nx + 1;
                    )
                );

                dfs[0, 0, 0, 0];
                ans;
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(92)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Counter() {
            var code = @"
                fn counter [begin] ( begin = begin - 1; fn -> begin = begin + 1 );
                [!ca, cb, !cc, cd] = [counter 0, counter(0), counter[7], counter$-1];

                return [ca == cb, ca ~= cb, ca[], ca[], ca[], cb[], ca[], cc[], ca[], cd[], ca[]];
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertInteger(0)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(0)
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(7)
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertInteger(-1)
                .MoveNext().AssertInteger(5)
                .MoveNext().AssertEnd();
        }

        private static uint gcd(uint x, uint y) {
            if (x > y) return gcd(y, x);
            if (x == 0) return y;
            return gcd(y % x, x);
        }

        public static IEnumerable<object[]> GcdTestCases(int count) {
            var rngx = new I32ToU32RNG<PCG>();
            var rngy = new I32ToU32RNG<PCG>();

            return rngx.Zip(rngy, (x, y) => new object[] { x, y, gcd(x, y) }).Take(count).ToList();
        }

        public static IEnumerable<object[]> GcdSmallTestCases(int count) {
            var rngx = new I32ToU32RNG<PCG>();
            var rngy = new I32ToU32RNG<PCG>();

            return rngx.Zip(rngy, (x, y) => new object[] { x & 0xfffu, y & 0xfffu, gcd(x & 0xfffu, y & 0xfffu) }).Take(count).ToList();
        }

        [Theory]
        [MemberData(nameof(GcdTestCases), 1024)]
        [MemberData(nameof(GcdSmallTestCases), 512)]
        public void RecursiveGcd(uint x, uint y, uint expected) {
            var code = @$"
                !gcd = [a, b] -> if (a > b) gcd[b, a] elif (~(0 < a)) b else gcd[b % a, a];
                [gcd[{x}, {y}]]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(expected)
                .MoveNext().AssertEnd();
        }

        [Theory]
        [MemberData(nameof(GcdTestCases), 1024)]
        [MemberData(nameof(GcdSmallTestCases), 512)]
        public void LoopGcd(uint x, uint y, uint expected) {
            var code = @$"
                !gcd = [a, b] -> (while (b > 0) [a, b] = [b, a % b]; a);
                fn gcd_long[a, b] while (true) ( if (b == 0) break a; [a, b] = [b, a % b]);
                [gcd[{x}, {y}], gcd_long[{x}, {y}]]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(expected)
                .MoveNext().AssertInteger(expected)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void YCombinator() {
            var code = @"
                Y = f -> (x -> f(x x))(x -> f(v -> x x v));
                Y (f -> n -> if (n == 0) 1 else n * f(n - 1)) 10
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(3628800)
                .MoveNext().AssertEnd();
        }


        [Theory]
        [InlineData("1 + true", GlosOp.Add, GlosValueType.Integer, GlosValueType.Boolean)]
        [InlineData("\"\" - 1", GlosOp.Sub, GlosValueType.String, GlosValueType.Integer)]
        [InlineData("nil * {}", GlosOp.Mul, GlosValueType.Nil, GlosValueType.Table)]
        [InlineData("2 / \"\"", GlosOp.Div, GlosValueType.Integer, GlosValueType.String)]
        [InlineData("\"\" % 2", GlosOp.Mod, GlosValueType.String, GlosValueType.Integer)]
        [InlineData("true << false", GlosOp.Lsh, GlosValueType.Boolean, GlosValueType.Boolean)]
        [InlineData("\"\" >> 44353", GlosOp.Rsh, GlosValueType.String, GlosValueType.Integer)]
        [InlineData("true & 1", GlosOp.And, GlosValueType.Boolean, GlosValueType.Integer)]
        [InlineData("1 | \"\"", GlosOp.Orr, GlosValueType.Integer, GlosValueType.String)]
        [InlineData("{} ^ nil", GlosOp.Xor, GlosValueType.Table, GlosValueType.Nil)]
        [InlineData("false > 1", GlosOp.Gtr, GlosValueType.Boolean, GlosValueType.Integer)]
        [InlineData("{} < true", GlosOp.Lss, GlosValueType.Table, GlosValueType.Boolean)]
        [InlineData("nil >= {}", GlosOp.Geq, GlosValueType.Nil, GlosValueType.Table)]
        [InlineData("true <= 1", GlosOp.Leq, GlosValueType.Boolean, GlosValueType.Integer)]
        public void BadBiOp(string code, GlosOp op, GlosValueType left, GlosValueType right) {
            var exception = Assert.IsType<GlosInvalidBinaryOperandTypeException>(Assert.Throws<GlosRuntimeException>(() => Execute(code)).InnerException);
            Assert.Equal(op, exception.Op);
            Assert.Equal(left, exception.Left.Type);
            Assert.Equal(right, exception.Right.Type);
        }

        [Fact]
        public void Vector2D() { 
            var code = @"
                fn is_numeric[x] `type x == ""integer"" | `type x == ""float"";

                vector = { 
                    .__add: [a, b] -> vector.new[a.x + b.x, a.y + b.y],
                    .__sub: [a, b] -> vector.new[a.x - b.x, a.y - b.y],
                    .__mul: [a, b] -> if (is_numeric a) vector.new[a * b.x, a * b.y] 
                                      elif (is_numeric b) vector.new[b * a.x, b * a.y] 
                                      else a.x * b.x + a.y * b.y,
                    .__lss: [a, b] -> vector.len2$a < vector.len2$b,
                    .__equ: [a, b] -> a.x == b.x & a.y == b.y,
                    .__neg: v -> vector.new[-v.x, -v.y],

                    .new: [x, y] -> (rv = { .x: x, .y: y }; `meta rv = vector; rv),
                    .len2: v -> v * v,
                };


                x = vector.new[1, 0];
                y = vector.new[0, 1];
                c = x * 1 + 2 * y;
                d = x * 3 - 4 * -y;
                e = x * 5;

                [
                    c.x, c.y, d.x, d.y,
                    c * d, c > d, c < d, c >= d, c <= d, c == d, c ~= d,
                    e * d, e > d, e < d, e >= d, e <= d, e == d, e ~= d,
                    d * e, d > e, d < e, d >= e, d <= e, d == e, d ~= e,
                ];
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertInteger(11)
                .MoveNext().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertInteger(15)
                .MoveNext().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertInteger(15)
                .MoveNext().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void TableHash() {
            var code = @"
                mt = { .__hash: v -> v.x % 4, .__equ: [x, y] -> x.x % 4 == y.x % 4 };

                fn ntb[x] (
                    rv = { .x: x };
                    `meta rv = mt;
                    rv;
                );

                k = {};
                k@[ntb[0]] = 0;
                k@[ntb[1]] = 1;

                [k @ (ntb 101), k @ (ntb 100)]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(0)
                .MoveNext().AssertEnd();
        }

        private static ulong reverseBit(ulong x) {
            ulong rv = 0;
            for (int i = 0; i < 64; ++i) {
                if ((x & (1ul << i)) != 0) rv |= 1ul << (63 - i);
            }
            return rv;
        }

        public static IEnumerable<object[]> BitsReverseTestCases(int count) {
            return new I32ToU64RNG<PCG>().Map(x => new object[] { x, reverseBit(x) }).Take(count).ToList();
        }

        [Theory]
        [MemberData(nameof(BitsReverseTestCases), 1024)]
        public void BitsReverse(ulong origin, ulong expected) {
            var code = $@"
                x = 0x{origin:x16};
                x = ((x & 0x5555555555555555) <<  1) | ((x & 0xAAAAAAAAAAAAAAAA) >>  1);
                x = ((x & 0x3333333333333333) <<  2) | ((x & 0xCCCCCCCCCCCCCCCC) >>  2);
                x = ((x & 0x0F0F0F0F0F0F0F0F) <<  4) | ((x & 0xF0F0F0F0F0F0F0F0) >>  4);
                x = ((x & 0x00FF00FF00FF00FF) <<  8) | ((x & 0xFF00FF00FF00FF00) >>  8);
                x = ((x & 0x0000FFFF0000FFFF) << 16) | ((x & 0xFFFF0000FFFF0000) >> 16);
                x = ((x & 0x00000000FFFFFFFF) << 32) | ((x & 0xFFFFFFFF00000000) >> 32);
                x;
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(unchecked((long)expected))
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void LogicalInteger128() {
            var code = @"
                i128 = {
                    .__and: [x, y] -> i128.new[x.hi & y.hi, x.lo & y.lo],
                    .__orr: [x, y] -> i128.new[x.hi | y.hi, x.lo | y.lo],
                    .__xor: [x, y] -> i128.new[x.hi ^ y.hi, x.lo ^ y.lo],
                    .__not: v -> i128.new[~(x.hi), ~(x.lo)],

                    .new: [hi, lo] -> (rv = { .hi: hi, .lo: lo }; `meta rv = i128; rv),
                };


                x = i128.new[0x00000000ffffffff, 0xffffffff00000000];
                y = i128.new[0x3333cccc3333cccc, 0x5555aaaa5555aaaa];
                a = x & y;
                b = x | y;
                c = x ^ y;
                d = ~x;

                [ a.hi, a.lo, b.hi, b.lo, c.hi, c.lo, d.hi, d.lo ];
            ";

            unchecked {
                GlosValueArrayChecker.Create(Execute(code))
                    .FirstOne().AssertInteger((long)0x000000003333ccccul)
                    .MoveNext().AssertInteger((long)0x5555aaaa00000000ul)
                    .MoveNext().AssertInteger((long)0x3333ccccfffffffful)
                    .MoveNext().AssertInteger((long)0xffffffff5555aaaaul)
                    .MoveNext().AssertInteger((long)0x3333cccccccc3333ul)
                    .MoveNext().AssertInteger((long)0xaaaa55555555aaaaul)
                    .MoveNext().AssertInteger((long)0xffffffff00000000ul)
                    .MoveNext().AssertInteger((long)0x00000000fffffffful)
                    .MoveNext().AssertEnd();
            }
        }

        [Fact]
        public void Integer128() {
            var code = @"
                i128 = {
                    .__lss: [x, y] -> if (x.hi ~= y.hi) x.hi < y.hi else x.lo < y.lo,
                    .__equ: [x, y] -> x.hi == y.hi && x.lo == y.lo,
                    .__and: [x, y] -> i128.new[x.hi & y.hi, x.lo & y.lo],
                    .__orr: [x, y] -> i128.new[x.hi | y.hi, x.lo | y.lo],
                    .__xor: [x, y] -> i128.new[x.hi ^ y.hi, x.lo ^ y.lo],
                    .__not: v -> i128.new[~(x.hi), ~(x.lo)],

                    .new: [hi, lo] -> (rv = { .hi: hi, .lo: lo }; `meta rv = i128; rv),
                };

                x = i128.new[0x00000000ffffffff, 0xffffffff00000000];
                y = i128.new[0x3333cccc3333cccc, 0x5555aaaa5555aaaa];
                a = x & y;
                b = x | y;
                c = x ^ y;
                d = ~x;

                [ a.hi, a.lo, b.hi, b.lo, c.hi, c.lo, d.hi, d.lo, x == y, x ~= y, x < y, x > y ];
            ";

            unchecked {
                GlosValueArrayChecker.Create(Execute(code))
                    .FirstOne().AssertInteger((long)0x000000003333ccccul)
                    .MoveNext().AssertInteger((long)0x5555aaaa00000000ul)
                    .MoveNext().AssertInteger((long)0x3333ccccfffffffful)
                    .MoveNext().AssertInteger((long)0xffffffff5555aaaaul)
                    .MoveNext().AssertInteger((long)0x3333cccccccc3333ul)
                    .MoveNext().AssertInteger((long)0xaaaa55555555aaaaul)
                    .MoveNext().AssertInteger((long)0xffffffff00000000ul)
                    .MoveNext().AssertInteger((long)0x00000000fffffffful)
                    .MoveNext().AssertFalse()
                    .MoveNext().AssertTrue()
                    .MoveNext().AssertTrue()
                    .MoveNext().AssertFalse()
                    .MoveNext().AssertEnd();
            }
        }

        [Theory]
        [InlineData("a", "b", "c")]
        [InlineData("d", "f", "e")]
        [InlineData("h", "g", "i")]
        [InlineData("k", "l", "j")]
        [InlineData("o", "m", "n")]
        [InlineData("r", "q", "p")]
        public void ThreeStringSort(string a, string b, string c) {
            var code = $@"
                [a, b, c] = [""{a}"", ""{b}"", ""{c}""];
                if (a >= b)
                    if (c < a)
                        if (b > c) [c, b, a]
                        else [b, c, a]
                    else [b, a, c]
                else
                    if (c <= b)
                        if (a > c) [c, a, b]
                        else [a, c, b]
                    else [a, b, c]
            ";

            var list = new List<string> { a, b, c };
            list.Sort();

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertString(list[0])
                .MoveNext().AssertString(list[1])
                .MoveNext().AssertString(list[2])
                .MoveNext().AssertEnd();
        }

        public static IEnumerable<object[]> BisectionMethodTestCases(int count) {
            var min = -(1 << 14);
            var max = -min - 1;

            for (int i = 0; i < count; ++i) {
                int a, b, c;
                int delta;

                do {
                    a = DefaultRNG.I32.Next(min, max);
                    b = DefaultRNG.I32.Next(min, max);
                    c = DefaultRNG.I32.Next(min, max);

                    delta = b * b - 4 * a * c;
                } while (delta <= 0 || a == 0);

                var mid = -b / 2.0 / a;
                var spa = Math.Sqrt(delta) / 2.0 / a;
                var sol1 = mid - spa;
                var sol2 = mid + spa;

                if (sol1 > sol2) {
                    var temp = sol1;
                    sol1 = sol2;
                    sol2 = temp;
                }

                var coin = DefaultRNG.Coin.Next();

                var another = coin ? -Math.Abs(mid) - 2.5 * Math.Abs(spa) : Math.Abs(mid) + 2.5 * Math.Abs(spa);
                var answer = coin ? sol1 : sol2;

                yield return new object[] { a, b, c, Math.Min(mid, another), Math.Max(mid, another), answer };
            }
        }
        
        [Theory]
        [MemberData(nameof(BisectionMethodTestCases), 1024)]
        public void BisectionMethod(int a, int b, int c, double l, double r, double expected) {
            const double epsilon = 1e-6;

            var code = $@"
                [a, b, c] = [{a}, {b}, {c}];
                f = x -> a * x * x + b * x + c;
                eps = {epsilon:F20};
                [l, r] = [{l:F20}, {r:F20}];

                rising = (f r) > 0;

                while ((r - l) >= eps) (
                    mid = (l + r) / 2.0;
                    if (rising ^ (f mid) > 0) l = mid else r = mid;
                );
                
                l;
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertFloatAbsoluteError(expected, epsilon)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void CoroutineBasic() {
            var code = $@"
                # swap two pairs
                c = -> fn [a, b] (
                    [c, d] = <- [b, a];
                    return [d, c];
                );

                [x, y] = c <- [1, 2];
                [z, w] = c <- [4, 5];

                [x, y, z, w] # [2, 1, 5, 4] expected
            ";
            
            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(2)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(5)
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertEnd();
        }
    }
}
