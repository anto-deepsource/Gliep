using GeminiLab.Glos;
using GeminiLab.Glug;

namespace GeminiLab.XUnitTester.Gliep.Glug {
    public class GlugExecutionTestBase {
        internal GlosViMa ViMa { get; } = new GlosViMa();

        public GlosValue[] Execute(string source, GlosContext? context = null) {
            using var unit = TypicalCompiler.Compile(source);

            return ViMa.ExecuteUnit(unit, null, context ?? new GlosContext(null!));
        }
    }
}
