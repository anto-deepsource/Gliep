using GeminiLab.Glos;
using GeminiLab.Glug;

namespace XUnitTester.Glug {
    public class GlugExecutionTestBase {
        internal GlosViMa ViMa { get; } = new GlosViMa();

        public GlosValue[] Execute(string source, GlosContext? context = null) {
            var unit = TypicalCompiler.Compile(source);

            return ViMa.ExecuteUnit(unit, null, context ?? new GlosContext(null!));
        }
    }
}
