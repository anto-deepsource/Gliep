using System;
using System.Collections.Generic;

namespace GeminiLab.Glos {
    public partial class GlosViMa {
        private readonly Stack<GlosCoroutine> _coroutineStack = new();

        // TODO: unwrap coroutine stack when an exception occurs
        public void ClearCoroutines() {
            _coroutineStack.Clear();
        }

        public GlosValue[] ExecuteFunctionWithProvidedContext(GlosFunction function, GlosContext? context, GlosValue[]? args = null) {
            if (_coroutineStack.Count > 0) {
                throw new InvalidOperationException();
            }

            args ??= new GlosValue[0];

            _coroutineStack.Push(NewCoroutine(function, context));
            while (_coroutineStack.Count > 0) {
                var cor = _coroutineStack.Peek();

                var result = cor.Resume(args);
                args = result.ReturnValues;

                switch (result.Result) {
                case GlosCoroutine.ExecResultType.Return:
                case GlosCoroutine.ExecResultType.Yield:
                    _coroutineStack.Pop();

                    if (_coroutineStack.Count > 0) {
                        _coroutineStack.Peek().ClearToResume();
                    } else if (result.Result == GlosCoroutine.ExecResultType.Yield) {
                        throw new InvalidOperationException();
                    }

                    break;
                case GlosCoroutine.ExecResultType.Resume:
                    _coroutineStack.Push(result.CoroutineToResume);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }

            return args;
        }

        public GlosCoroutine NewCoroutine(GlosFunction function, GlosContext? context) {
            return new(this, function, context);
        }

        public GlosCoroutine? CurrentCoroutine {
            get {
                if (_coroutineStack.TryPeek(out var cor)) {
                    return cor;
                }

                return null;
            }
        }

        public GlosValue[] ExecuteFunction(GlosFunction function, GlosValue[]? args = null) {
            return ExecuteFunctionWithProvidedContext(function, null, args);
        }

        // though ViMa shouldn't manage units, this function is necessary
        public GlosValue[] ExecuteUnit(IGlosUnit unit, GlosValue[]? args = null, GlosContext? parentContext = null) {
            return ExecuteFunction(new GlosFunction(unit.FunctionTable[unit.Entry], parentContext ?? new GlosContext(null), unit), args);
        }

        public GlosValue[] ExecuteUnitWithProvidedContextForRootFunction(IGlosUnit unit, GlosContext context, GlosValue[]? args = null) {
            return ExecuteFunctionWithProvidedContext(new GlosFunction(unit.FunctionTable[unit.Entry], context.Parent!, unit), context, args);
        }
    }
}
