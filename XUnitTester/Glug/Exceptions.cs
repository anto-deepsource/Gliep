using System;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glug {
    public class Exceptions : GlugExecutionTestBase {
        [Fact]
        public void BadCoroutineSchedule() {
            // resuming a dead coroutine
            var code = @"
                c = -> [] -> 1;
                [c <- [], c <- []];
            ";

            Assert.Throws<InvalidOperationException>(() => { Execute(code); });

            // resuming your ancestor
            code = @"
                a = -> [] -> b <- [];
                b = -> [] -> a <- [];
                [b <- []];
            ";

            Assert.Throws<InvalidOperationException>(() => { Execute(code); });

            // resuming yourself
            code = @"
                a = -> [] -> a <- [];
                [a <- []];
            ";

            Assert.Throws<InvalidOperationException>(() => { Execute(code); });
        }
    }
}
