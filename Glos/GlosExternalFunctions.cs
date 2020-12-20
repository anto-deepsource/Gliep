using System;

namespace GeminiLab.Glos {
    public delegate GlosValue[] GlosExternalPureFunction(GlosValue[] arg);
    
    public delegate GlosValue[] GlosExternalFunction(GlosCoroutine coroutine, GlosValue[] args);

    public enum AsyncFunctionResumeResultType {
        Return,
        Resume,
        Yield,
        Call,
    }
    
    public struct AsyncFunctionResumeResult {
        public AsyncFunctionResumeResultType Type;
        public GlosValue                     Value;
        public GlosValue[]                   Arguments;
    }
    
    public interface IGlosExternalAsyncFunctionCall {
        AsyncFunctionResumeResult Resume(ReadOnlySpan<GlosValue> arguments);
    }
    
    public interface IGlosExternalAsyncFunction {
        IGlosExternalAsyncFunctionCall Call(GlosCoroutine coroutine);
    }
}
