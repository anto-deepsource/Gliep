namespace GeminiLab.Glos {
    public partial class GlosCoroutine {
        private readonly GlosStack<GlosTryStackFrame> _tryStack = new GlosStack<GlosTryStackFrame>();

        private int _slmt = -1, _clmt = -1, _dlmt = -1;
        
        private int _tptr => _tryStack.Count;

        private ref GlosTryStackFrame peekTry() => ref _tryStack.StackTop();

        private ref GlosTryStackFrame pushTry(int dest) {
            ref var pushed = ref _tryStack.PushStack();

            pushed.StackPointer = _sptr;
            pushed.CallStackPointer = _cptr;
            pushed.DelimiterStackPointer = _dptr;
            pushed.Destination = dest;

            return ref pushed;
        }

        private void popTry() {
            if (callStackTop().TryStackBase >= _tptr) {
                return;
            }

            _tryStack.PopStack();

            /*
            if (_tryStack.Count > 0) {
                ref var t = ref tryStackTop();

                _slmt = t.StackPointer;
                _clmt = t.CallStackPointer;
                _dlmt = t.DelimiterStackPointer;
            } else {
                _slmt = _clmt = _dlmt = -1;
            }
            */
        }

        private bool hasTry() {
            return callStackTop().TryStackBase < _tptr;
        }

        private void popCurrentFrameTry() {
            var tBase = callStackTop().TryStackBase;

            while (_tptr > tBase) {
                popTry();
            }
        }
    }
}