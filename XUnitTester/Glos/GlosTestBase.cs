using GeminiLab.Glos;
using GeminiLab.Glos.CodeGenerator;

namespace GeminiLab.XUnitTester.Gliep.Glos {
    public abstract class GlosTestBase {
        protected readonly GlosViMa ViMa;
        protected readonly UnitBuilder Builder;

        protected GlosTestBase() {
            ViMa = new GlosViMa();
            Builder = new UnitBuilder();
        }

        protected GlosValue[] Execute(GlosValue[]? args = null, GlosContext? parentContext = null) {
            using var unit = Builder.GetResult();
            
            ViMa.ClearCoroutines();
            return ViMa.ExecuteUnit(unit, args, parentContext);
        }
    }
}
