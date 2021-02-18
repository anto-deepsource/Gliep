using GeminiLab.Glos;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glos.DataStructure {
    public class Table {
        [Fact]
        public void TableOperations() {
            var x = new GlosTable();

            x.UpdateEntry(0, 0);
            x.UpdateEntry(1, 1);
            x.UpdateEntry(0, 2);
            x.UpdateEntry(1, 3);

            GlosTableChecker.Create(x)
                .Has(0, 2)
                .Has(1, 3)
                .AssertAllKeyChecked();
        }
    }
}
