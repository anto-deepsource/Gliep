using System;
using System.Collections.Generic;
using System.Text;
using GeminiLab.Core2.Text;
using Xunit;
using XUnitTester.Checker;

namespace XUnitTester.Glug {
    public class GlugTest : GlugExecutionTestBase {
        [Fact]
        public void Evaluation() {
            var code = @"
                [1, 2, false, (), nil, 1 + 2, if (true) 1, if (false) 2]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .First().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertFalse()
                .MoveNext().AssertNil()
                .MoveNext().AssertNil()
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertNil()
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Fibonacci() {
            var code = @"
                fn fibo [x] if (x <= 1) x else fibo[x - 2] + fibo[x - 1];
                return [fibo[0], fibo[1], fibo[10]];
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .First().AssertInteger(0)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(55)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Counter() {
            var code = @"
                fn counter [begin] ( begin = begin - 1; fn -> begin = begin + 1 );
                [$ca, $cb, $cc, $cd] = [counter 0, counter(0), counter[7], counter@-1];

                return [ca[], ca[], ca[], cb[], ca[], cc[], ca[], cd[], ca[]];
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .First().AssertInteger(0)
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

        [Fact]
        public void RecursiveGcd() {
            var code = @"
                $gcd = [a, b] -> if (a > b) gcd[b, a] elif (~(0 < a)) b else gcd[b % a, a];
                [gcd[4, 6], gcd[2, 1], gcd[117, 39], gcd[1, 1], gcd[15, 28]]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .First().AssertInteger(2)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(39)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void YCombinator() {
            var code = @"
                $Y = f -> (x -> f(x x))(x -> f(v -> x x v));

                Y (f -> n -> if (n == 0) 1 else n * f(n - 1)) 10
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .First().AssertInteger(3628800)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void String() {
            string strA = "strA", strB = "ユニコードイグザンプル";
            string strEscape = "\\n";
            var code = $@"
                [""{strA}"", ""{strA}"" + ""{strB}"", ""{strEscape}""]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .First().AssertString(strA)
                .MoveNext().AssertString(strA + strB)
                .MoveNext().AssertString(EscapeSequenceConverter.Decode(strEscape))
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Beide() {
            var code = @"
                fn beide -> [1, 2];
                fn sum[x, y] x + y;

                return [beide[], beide[] - 1, sum@beide[]]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .First().AssertInteger(1)
                .MoveNext().AssertInteger(0)
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void RecursiveLoop() {
            var code = @" 
                fn loop[from, to, step, body] (
                    if (from < to) (
                        body[from];
                        loop[from + step, to, step, body];
                    )
                )
                $sum = 0;
                loop[1, 512 + 1, 1, i -> sum = sum + i];
                $mul = 1;
                loop[1, 10, 1, i -> mul = mul * i];
                return [sum, mul];
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .First().AssertInteger((1 + 512) * 512 / 2)
                .MoveNext().AssertInteger(362880)
                .MoveNext().AssertEnd();
        }
    }
}
