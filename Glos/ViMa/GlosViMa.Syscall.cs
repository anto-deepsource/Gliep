using System;

namespace GeminiLab.Glos.ViMa {
    // todo: add delimiter to its params
    public delegate void GlosSyscall(GlosStack<GlosValue> stack, GlosStack<GlosStackFrame> callStack, GlosStack<int> delStack);

    public partial class GlosViMa {
        private const int MaxSyscallCount = 8;
        private readonly GlosSyscall?[] _syscalls = new GlosSyscall[MaxSyscallCount];

        public void SetSyscall(int index, GlosSyscall? syscall) {
            if (index < 0 || index >= MaxSyscallCount) throw new ArgumentOutOfRangeException();
            _syscalls[index] = syscall;
        }
    }
}
