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
            x.UpdateEntry(true, false);
            x.UpdateEntry(false, true);
            x.UpdateEntry("x", "y");
            x.UpdateEntry("y", "x");
            x.UpdateEntry(7.0, 8.0);
            x.UpdateEntry(8.0, 7.0);
            x.UpdateEntry(GlosValue.NewNil(), GlosValue.NewNil());

            GlosTableChecker.Create(x)
                .Has(0, 2)
                .Has(1, 3)
                .Has(true, false)
                .Has(false, true)
                .Has("x", "y")
                .Has("y", "x")
                .Has(7.0, 8.0)
                .Has(8.0, 7.0)
                .Has(GlosValue.NewNil(), v => v.IsNil())
                .AssertAllKeyChecked();
        }
    }
}
