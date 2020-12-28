using System;

namespace GeminiLab.Glos {
    public delegate GlosValue[] GlosPureEFunction(GlosValue[] arg);

    public delegate GlosValue[] GlosEFunction(GlosCoroutine coroutine, GlosValue[] args);

    public enum AsyncEFunctionResumeResultType {
        Return,
        Resume,
        Yield,
        Call,
    }

    public struct AsyncEFunctionResumeResult {
        public AsyncEFunctionResumeResultType Type;
        public GlosValue                     Value;
        public GlosValue[]                   Arguments;
    }

    public interface IGlosAsyncEFunctionCall {
        AsyncEFunctionResumeResult Resume(ReadOnlySpan<GlosValue> arguments);
    }

    public interface IGlosAsyncEFunction {
        IGlosAsyncEFunctionCall Call(GlosCoroutine coroutine);
    }
}
