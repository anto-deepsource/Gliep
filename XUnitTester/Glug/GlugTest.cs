using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using XUnitTester.Checker;

namespace XUnitTester.Glug {
    public class GlugTest : GlugExecutionTestBase {
        [Fact]
        public void Fibonacci() {
            var code = $@"
                fn fibo [x] if (x <= 1) x else fibo[x - 2] + fibo[x - 1];
                return [fibo[0], fibo[1], fibo[10]];
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .First().AssertInteger(0)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(55)
                .MoveNext().AssertEnd();
        }
    }
}
