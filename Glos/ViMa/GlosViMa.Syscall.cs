using System;
using System.Diagnostics;

namespace GeminiLab.Glos.ViMa {
    // todo: add delimiter to its params
    public delegate void GlosSyscall(GlosStack<GlosValue> _stack, GlosStack<GlosStackFrame> _callStack, GlosStack<int> _delStack);

    public partial class GlosViMa {
        private const int MaxSyscallCount = 8;
        private readonly GlosSyscall?[] _syscalls = new GlosSyscall[MaxSyscallCount];

        public void SetSyscall(int index, GlosSyscall? syscall) {
            if (index < 0 || index >= MaxSyscallCount) throw new ArgumentOutOfRangeException();
            _syscalls[index] = syscall;
        }
    }
}
