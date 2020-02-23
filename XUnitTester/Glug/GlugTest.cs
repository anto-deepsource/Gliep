using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using XUnitTester.Checker;

namespace XUnitTester.Glug {
    public class GlugTest : GlugExecutionTestBase {
        [Fact]
        public void Evaluation() {
            var code = @"
                [1, 2, false, {}, nil, 1 + 2, if (true) 1, if (false) 2]
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
                fn counter [begin] { begin = begin - 1; fn -> begin = begin + 1 };
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
                fn gcd[a, b] if (a > b) gcd[b, a] else if (~(0 < a)) b else gcd[b % a, a];
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
    }
}
