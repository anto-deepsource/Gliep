using System;
using System.Runtime.InteropServices;
using GeminiLab.Glos;
using ObjectLayoutInspector;

namespace Exam {
    [StructLayout(LayoutKind.Explicit)]
    struct A {
        [FieldOffset(0)] public long   Long;
        [FieldOffset(0)] public double Double;
    }
    
    public class Program {
        public static int Main(string[] args) {
            TypeLayout.PrintLayout<GlosValue>();
            TypeLayout.PrintLayout<A>();
            TypeLayout.PrintLayout<GlosStackFrame>();
            return 0;
        }
    }
}
