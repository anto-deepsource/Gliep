namespace GeminiLab.Glos {
    public partial struct GlosValue {
        private long getHash(GlosViMa viMa) {
            if (_hashCodeCalculated) return _hashCode;

            GlosValue v = default;
            _hashCode = TryInvokeMetamethod(ref v, this, viMa, GlosMetamethodNames.Hash) ? v.getHash(viMa) : ValueObject!.GetHashCode();
            
            _hashCodeCalculated = true;
            return _hashCode;
        }
        
        private void preEvalHash() {
            if (_type == GlosValueType.Table) {
                _hashCodeCalculated = false;
                return;
            }

            _hashCode = _type switch {
                GlosValueType.Nil => 0,
                GlosValueType.Integer => ValueNumber.Integer,
                GlosValueType.Float => ValueNumber.Integer,
                GlosValueType.Boolean => ValueNumber.Integer,
                GlosValueType.String => StringHash((string)ValueObject!),
                GlosValueType.Function => FunctionHash((GlosFunction)ValueObject!),
                GlosValueType.ExternalFunction => ValueObject!.GetHashCode(),
                _ => 0,
            };

            _hashCodeCalculated = true;
        }

        private static long Combine(int hi, int lo) {
            unchecked {
                return (long)(((ulong)(uint)hi) << 32 | (uint)lo);
            }
        }

        private static long StringHash(string v) {
            int mid = v.Length / 2;

            // only this ugly and slow implementation
            // until string.GetHashCode(ReadOnlySpan<char>) becomes available in .net standard
            return Combine(v.Substring(0, mid).GetHashCode(), v.Substring(mid).GetHashCode());
        }

        private static long FunctionHash(GlosFunction fun) {
            return Combine(fun.Prototype.GetHashCode(), fun.ParentContext?.GetHashCode() ?? 0);
        }
    }
}
