using System;

namespace GeminiLab.Glos.ViMa {
    public delegate void GlosSyscall(GlosValue[] stack, ref long sptr, GlosStackFrame[] callStack, ref long cptr);

    public partial class GlosViMa {
        private const int MaxSyscallCount = 8;
        private readonly GlosSyscall?[] _syscalls = new GlosSyscall[MaxSyscallCount];

        public void SetSyscall(int index, GlosSyscall? syscall) {
            if (index < 0 || index >= MaxSyscallCount) throw new ArgumentOutOfRangeException();
            _syscalls[index] = syscall;
        }
    }
}
