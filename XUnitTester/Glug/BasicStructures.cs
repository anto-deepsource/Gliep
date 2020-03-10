using GeminiLab.Core2.Text;
using GeminiLab.Glos.ViMa;

using Xunit;
using XUnitTester.Misc;

namespace XUnitTester.Glug {
    public class BasicStructures : GlugExecutionTestBase {
        [Fact]
        public void Return0() {
            GlosValueArrayChecker.Create(Execute("0"))
                .FirstOne().AssertInteger(0)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Evaluation() {
            var code = @"
                return [1, -2, 2--2, nil == (), nil ~= `{}, -(1 + 2),]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(-2)
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertTrue()
                .MoveNext().AssertFalse()
                .MoveNext().AssertInteger(-3)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void String() {
            string strA = "strA", strB = "ユニコードイグザンプル\u4396";
            string strEscape = "\\n";
            var code = $@"
                [""{strA}"", ""{strA}"" + ""{strB}"", ""{strEscape}""]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertString(strA)
                .MoveNext().AssertString(strA + strB)
                .MoveNext().AssertString(EscapeSequenceConverter.Decode(strEscape))
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void If() {
            var code = $@"
                !a = if (true) 1;
                !b = if (false) 2;
                [!c, !d] = if (a == nil) [3, 3, 3] elif (b == nil) [4, 5, 6] else 5;
                !e;
                if (c == d) (
                    e = false;
                    1;
                ) elif (c > d) (
                    e = true;
                    2;
                ) else (
                    3;
                );
                [a, b, c, d, e]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertNil()
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertInteger(5)
                .MoveNext().AssertNil()
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void While() {
            var code = $@"
                i = 0;
                !a = while (i < 10) i = i + 1;
                !b = while (i > 0) ( if (i == 5) break [i, i]; i = i - 1 );
                while (i < 10) i = i + 1;
                !c = i;
                while (i > 0) ( if (i == 5) break(); i = i - 1 );
                !d = i;
                [a, b, c, d]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(10)
                .MoveNext().AssertInteger(5)
                .MoveNext().AssertInteger(10)
                .MoveNext().AssertInteger(5)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void FunctionCall() {
            var code = @"
                fn beide -> [1, 2];
                fn sum2[x, y] -> x + y;
                fn sum3[x, y, z] x + y + z;

                return [beide[], beide[] - 1, sum2$beide[], sum3$(beide[]..3)] .. beide[]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(0)
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(6)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void DeepRecursiveLoop() {
            var code = @"
                fn loop[from, to, step, body] (
                    if (from < to) (
                        body[from];
                        loop[from + step, to, step, body];
                    )
                );
                !sum = 0;
                loop[1, 131072 + 1, 1, i -> sum = sum + i];
                !mul = 1;
                loop[1, 10, 1, i -> mul = mul * i];
                return [sum, mul];
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger((1L + 131072) * 131072 / 2)
                .MoveNext().AssertInteger(362880)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void GlobalAndLocal() {
            var code = @"
                fn a -> x = 1;
                fn b -> !!x = 2;
                fn c -> !!x;

                return [c[], a[], b[], a[], c[], !!ext[], c[]]
            ";

            var context = new GlosContext(null);
            context.CreateVariable("ext", (GlosExternalFunction)((param) => { context.GetVariableReference("x") = 3; return new GlosValue[] { 3 }; }));

            GlosValueArrayChecker.Create(Execute(code, context))
                .FirstOne().AssertNil()
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Table() {
            var code = @"
                e = ""ee"";
                a = { .a: 1, .b: 2, ""dd"": 0, @e: 0 };
                a.c = 3; a@""dd"" = ((a@e = 5) - 1);
                
                b = { .a: a.ee };
                
                `a = `b = { .__add: [x, y] -> x.a + y.a };

                [a.a, a.b, a.c, a.dd, a.ee, a + b]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertInteger(5)
                .MoveNext().AssertInteger(6)
                .MoveNext().AssertEnd();
        }
    }
}