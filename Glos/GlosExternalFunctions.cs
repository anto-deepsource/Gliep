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
        public GlosValue                      Value;
        public GlosValue[]                    Arguments;

        public static AsyncEFunctionResumeResult Return(params GlosValue[] values) {
            return new AsyncEFunctionResumeResult { Type = AsyncEFunctionResumeResultType.Return, Arguments = values };
        }

        public static AsyncEFunctionResumeResult Resume(GlosCoroutine coroutine, params GlosValue[] arguments) {
            return new AsyncEFunctionResumeResult { Type = AsyncEFunctionResumeResultType.Resume, Value = coroutine, Arguments = arguments };
        }

        public static AsyncEFunctionResumeResult Yield(params GlosValue[] values) {
            return new AsyncEFunctionResumeResult { Type = AsyncEFunctionResumeResultType.Yield, Arguments = values };
        }

        public static AsyncEFunctionResumeResult Call(GlosValue function, params GlosValue[] values) {
            return new AsyncEFunctionResumeResult { Type = AsyncEFunctionResumeResultType.Call, Value = function, Arguments = values };
        }
    }

    public interface IGlosAsyncEFunctionCall {
        AsyncEFunctionResumeResult Resume(ReadOnlySpan<GlosValue> arguments);
    }

    public interface IGlosAsyncEFunction {
        IGlosAsyncEFunctionCall Call(GlosCoroutine coroutine);
    }
}
