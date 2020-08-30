using GeminiLab.Core2.Text;
using GeminiLab.Glos;
using GeminiLab.Glug;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glug {
    public class BasicStructures : GlugExecutionTestBase {
        [Fact]
        public void Return0() {
            GlosValueArrayChecker.Create(Execute("0"))
                .FirstOne().AssertInteger(0)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void ReturnEmpty() {
            GlosValueArrayChecker.Create(Execute("return"))
                .FirstOne().AssertEnd();
        }

        [Fact]
        public void Evaluation() {
            var code = @"
                return [1, -2, 2--2, nil == (), nil ~= `meta {}, `neg (1 + 2), 3.14159,]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(-2)
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertTrue()
                .MoveNext().AssertFalse()
                .MoveNext().AssertInteger(-3)
                .MoveNext().AssertFloatRelativeError(3.14159, 0.01)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void String() {
            string strA = "strA", strB = "ユニコードイグザンプル";
            string strEscape = "\\n\\u4396";
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
                [!c, !d] = if (`isnil a) [3, 3, 3] elif (?b) [4, 5, 6] else 5;
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
                while (i > 0) ( if (i == 5) break; i = i - 1 );
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
        public void AwfulBreak() {
            var code = $@"
                fn range[max] (
                    !x = -1;
                    return fn[] if (x + 1 < max) x = x + 1
                );
                [1, 2] 
                .. 
                (while (true) ( 3 + 4 * [[5, 6], [[break [7, 8], 9]]] )) 
                ..
                (for (i: range[10]) if (i == 3) break [i * i])
                ..
                (while 'o (true) ( while 'i (true) break 'o 11; break 12 ))
                ..
                (while 'o (true) ( while 'i (true) break 'i 13; break 14 ))
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(7)
                .MoveNext().AssertInteger(8)
                .MoveNext().AssertInteger(9)
                .MoveNext().AssertInteger(11)
                .MoveNext().AssertInteger(14)
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
        public void Assign() {
            var code = @"
                [a, b] = [c] = [1, 2, 3];
                [d, e] = f = [4, 5, 6];

                [a, b, c, d, e, f]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertNil()
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void Curry() {
            var code = @"
                curry2 = f -> x -> y -> f[x, y];
                curry3 = f -> x -> y -> z -> f[x, y, z];
                sumc2 = curry2 [x, y] -> x + y;
                sumc3 = curry3 [x, y, z] -> x + y + z;
                mulc2 = curry2 [x, y] -> x * y;

                return [sumc2 1 2, sumc3 3 4 5, mulc2 6 7]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(3)
                .MoveNext().AssertInteger(12)
                .MoveNext().AssertInteger(42)
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
                fn a -> !x = 1;
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
        public void TableAndAssign() {
            var code = @"
                e = ""ee"";
                a = { .a: 1, .b: 2, ""dd"": 0, @e: 0 };
                [a.c, a@""dd"", b, !mt] = [3, (a@e = 5) - 1, { .a: a.ee },  { .__add: [x, y] -> x.a + y.a }];
                
                [`meta a, `meta b] = [mt, mt];

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

        [Fact]
        public void VectorBase() {
            var code = @"
                a = {| 1, 1, 2, 3, 5, 8, 13 |};
                a @ |> = 21;
                a @ |> = 34;
                b = a @ |>;
                [a, b]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertVector(vec => {
                    GlosValueArrayChecker.Create(vec.AsMemory())
                        .FirstOne().AssertInteger(1)
                        .MoveNext().AssertInteger(1)
                        .MoveNext().AssertInteger(2)
                        .MoveNext().AssertInteger(3)
                        .MoveNext().AssertInteger(5)
                        .MoveNext().AssertInteger(8)
                        .MoveNext().AssertInteger(13)
                        .MoveNext().AssertInteger(21)
                        .MoveNext().AssertEnd();
                })
                .MoveNext().AssertInteger(34)
                .MoveNext().AssertEnd();
        }

        [Fact]
        public void ProvidedEnv() {
            var code = @"
                !a = 1;
                foo = [] -> a; # make sure a is in context; 
                !!a = 2; # it should refer to the same variable
                [a, foo[]]
            ";

            GlosValueArrayChecker
                .Create(ViMa.ExecuteUnitWithProvidedContextForRootFunction(TypicalCompiler.Compile(code), new GlosContext(null)))
                .FirstOne().AssertInteger(2)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertEnd();
        }
        
        [Fact]
        public void ForLoop() {
            var code = @"
                fn range[max] (
                    !x = -1;
                    return fn[] if (x + 1 < max) x = x + 1
                );

                for (v: range[16]) v;
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertInteger(15)
                .MoveNext().AssertEnd();
        }
        
        [Fact]
        public void ShortCircuitOperators() {
            var code = @"
                !failed = false;

                fn evil_function[] (
                    failed = true;
                    nil
                );

                [
                    false && evil_function[],
                    true || evil_function[],
                    1 ?? evil_function[],
                ] .. [
                    failed
                ]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertFalse()
                .MoveNext().AssertTrue()
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertFalse()
                .MoveNext().AssertEnd();
        }
        
        [Fact]
        public void TableIndex() {
            var code = @"
                # Evil metatable
                mt = {
                    .__ren: [t, k] -> k,
                    .__uen: [t, k, v] -> (),
                };
                
                `meta(!a = { .x: 1, .y: 2 }) = mt;
                `meta(!b = { .x: 3, .y: 4 }) = mt;
                
                [rt0, rt1, rt2, rt3, rt4, rt5, rt6, rt7] = [a.x, a.y, a.!x, a.!y, b.x, b.y, b.!x, b.!y];
                
                a@""x"" = 5; a@!""y"" = 6;
                b@""x"" = 7; b@!""y"" = 8;
                
                [rt8, rt9, rta, rtb, rtc, rtd, rte, rtf] = [a.x, a.y, a.!x, a.!y, b.x, b.y, b.!x, b.!y];
                
                [rt0, rt1, rt2, rt3, rt4, rt5, rt6, rt7, rt8, rt9, rta, rtb, rtc, rtd, rte, rtf]
            ";

            GlosValueArrayChecker.Create(Execute(code))
                .FirstOne().AssertString("x")
                .MoveNext().AssertString("y")
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertString("x")
                .MoveNext().AssertString("y")
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(4)
                .MoveNext().AssertString("x")
                .MoveNext().AssertString("y")
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(6)
                .MoveNext().AssertString("x")
                .MoveNext().AssertString("y")
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(8)
                .MoveNext().AssertEnd();
        }
    }
}
