using GeminiLab.Glos;
using GeminiLab.Glos.CodeGenerator;

namespace GeminiLab.XUnitTester.Gliep.Glos {
    public abstract class GlosTestBase {
        protected readonly GlosViMa ViMa;
        protected readonly GlosUnitBuilder Builder;

        protected GlosTestBase() {
            ViMa = new GlosViMa();
            Builder = new GlosUnitBuilder();
        }

        protected GlosValue[] Execute(GlosValue[]? args = null, GlosContext? parentContext = null) {
            using var unit = Builder.GetResult();
            
            return ViMa.ExecuteUnit(unit, args, parentContext);
        }
    }
}
