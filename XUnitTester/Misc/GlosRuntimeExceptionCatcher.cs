using System;
using GeminiLab.Glos;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Misc {
    public static class GlosRuntimeExceptionCatcher {
        public static TInner Catch<TInner>(Action action) where TInner : GlosException {
            var exception = Assert.Throws<GlosRuntimeException>(action);
            return Assert.IsType<TInner>(exception.InnerException);
        }
    }
}
