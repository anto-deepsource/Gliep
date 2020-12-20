using System;
using System.Collections.Generic;

namespace GeminiLab.Glos {
    public partial class GlosViMa {
        // Todo: coroutine should be referred by GlosCoroutine class instead of a index
        // Todo (then): coroutine list here could be removed or be replace by a list of weak references to coroutines
        private readonly List<GlosCoroutine> _coroutines     = new List<GlosCoroutine>();
        private readonly Stack<int>          _coroutineStack = new Stack<int>();

        // TODO: unwrap coroutine stack when an exception occurs
        public void ClearCoroutines() {
            _coroutines.Clear();
            _coroutineStack.Clear();
        }
        
        public GlosValue[] ExecuteFunctionWithProvidedContext(GlosFunction function, GlosContext? context, GlosValue[]? args = null) {
            if (_coroutineStack.Count > 0) {
                throw new InvalidOperationException();
            }
            
            args ??= new GlosValue[0];
            
            _coroutineStack.Push(NewCoroutine(function, context));
            while (_coroutineStack.Count > 0) {
                var cid = _coroutineStack.Peek();
                var coroutine = _coroutines[cid];

                var result = coroutine.Resume(args);
                args = result.ReturnValues;

                switch (result.Result) {
                case GlosCoroutine.ExecResultType.Return:
                case GlosCoroutine.ExecResultType.Yield:
                    _coroutineStack.Pop();
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

        public int NewCoroutine(GlosFunction function, GlosContext? context) {
            var cor = new GlosCoroutine(this, function, context);
            var id = _coroutines.Count;

            _coroutines.Add(cor);
            return id;
        }

        public GlosCoroutine? CurrentCoroutine {
            get {
                if (!_coroutineStack.TryPeek(out int cid)) {
                    return null;
                }

                return _coroutines[cid];
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
