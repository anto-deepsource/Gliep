using System;

namespace GeminiLab.Glos {
    public partial struct GlosValue {
        private static ulong Combine(int hi, int lo) {
            unchecked {
                return (((ulong) (uint) hi) << 32) | (uint) lo;
            }
        }

        private static ulong StringHash(string v) {
            int mid = v.Length / 2;

            // only this ugly and slow implementation
            // until string.GetHashCode(ReadOnlySpan<char>) becomes available in .net standard
            return Combine(v.Substring(0, mid).GetHashCode(), v.Substring(mid).GetHashCode());
        }

        private static ulong FunctionHash(GlosFunction fun) {
            return Combine(fun.Prototype.GetHashCode(), fun.ParentContext?.GetHashCode() ?? 0);
        }

        public readonly ulong Hash() {
            /*
            if (_hashAssigned) {
                return _hashCode;
            }
            */

            return unchecked(Type switch {
                GlosValueType.Nil            => 0ul,
                GlosValueType.Integer        => (ulong) ValueNumber.Integer,
                GlosValueType.Float          => (ulong) ValueNumber.Integer,
                GlosValueType.Boolean        => (ulong) ValueNumber.Integer,
                GlosValueType.Table          => (ulong) ValueObject!.GetHashCode(),
                GlosValueType.String         => StringHash(this.AssumeString()),
                GlosValueType.Function       => FunctionHash(this.AssumeFunction()),
                GlosValueType.EFunction      => (ulong) ValueObject!.GetHashCode(),
                GlosValueType.Vector         => (ulong) ValueObject!.GetHashCode(),
                GlosValueType.PureEFunction  => (ulong) ValueObject!.GetHashCode(),
                GlosValueType.AsyncEFunction => (ulong) ValueObject!.GetHashCode(),
                GlosValueType.Coroutine      => (ulong) ValueObject!.GetHashCode(),
                _                            => 0,
            });
        }

        /*
        public void SetHash(ulong hash) {
            _hashAssigned = true;
            _hashCode = hash;
        }
        */
    }
}
