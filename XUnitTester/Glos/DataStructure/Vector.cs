using GeminiLab.Glos;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glos.DataStructure {
    public class Vector {
        [Fact]
        public void VectorOperations() {
            var v = new GlosVector();

            v.Push(0);
            v.PushNil();
            v.Push(1);
            v.Push("a string");
            v.Pop();
            v.PopRef();

            v[0].SetNil();

            GlosValueArrayChecker.Create(v.AsMemory().ToArray())
                .FirstOne().AssertNil()
                .MoveNext().AssertNil()
                .MoveNext().AssertEnd();
        }
    }
}
