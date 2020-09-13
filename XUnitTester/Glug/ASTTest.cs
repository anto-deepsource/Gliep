using GeminiLab.Glos;
using GeminiLab.Glug;
using GeminiLab.Glug.AST;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glug {
    public class ASTTest : GlugExecutionTestBase {
        [Fact]
        public void SysCallAndToValueTest() {
            var root = new Block(
                new Expr[] {
                    new BiOp(
                        GlugBiOpType.Concat,
                        new SysCall(
                            2,
                            new Expr[] {
                                new OnStackList(
                                    new[] {
                                        new CommaExprListItem(
                                            CommaExprListItemType.Plain,
                                            new LiteralInteger(0x77)
                                        ),
                                        new CommaExprListItem(
                                            CommaExprListItemType.Plain,
                                            new LiteralString("0x88")
                                        ),
                                        new CommaExprListItem(
                                            CommaExprListItemType.Plain,
                                            new BiOp(
                                                GlugBiOpType.Sub,
                                                new LiteralInteger(0x99),
                                                new LiteralInteger(0x33)
                                            )
                                        ),
                                    }
                                )
                            },
                            SysCall.ResultType.Osl
                        ),
                        new ToValue(
                            new OnStackList(
                                new[] {
                                    new CommaExprListItem(
                                        CommaExprListItemType.Plain,
                                        new LiteralInteger(0x55)
                                    ),
                                    new CommaExprListItem(
                                        CommaExprListItemType.Plain,
                                        new LiteralInteger(0x44)
                                    ),
                                }
                            )
                        )
                    )
                }
            );

            ViMa.SetSyscall(2, (stack, callStack, delStack) => {
                    var del = delStack[^1];

                    var h = del;
                    var t = stack.Count;
                    GlosValue tmp = default;
                    tmp.SetNil();

                    while (h < t - 1) {
                        tmp = stack[h];
                        stack[h] = stack[t - 1];
                        stack[t - 1] = tmp;

                        ++h;
                        --t;
                    }
                }
            );

            GlosValueArrayChecker.Create(ViMa.ExecuteUnit(TypicalCompiler.PostProcessAndCodeGen(root)))
                .FirstOne().AssertInteger(0x66)
                .MoveNext().AssertString("0x88")
                .MoveNext().AssertInteger(0x77)
                .MoveNext().AssertInteger(0x55)
                .MoveNext().AssertEnd();

            ViMa.SetSyscall(2, null);
        }
    }
}
