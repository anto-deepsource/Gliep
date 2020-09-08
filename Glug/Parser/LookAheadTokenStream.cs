using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.Parser {
    public class LookAheadTokenStream : IGlugTokenStream {
        private readonly IGlugTokenStream _source;

        public LookAheadTokenStream(IGlugTokenStream source) {
            _source = source;
        }

        private GlugToken? _buff;

        public bool NextEof() {
            return _buff == null && !_source.HasNext();
        }

        public GlugToken PeekToken() {
            return _buff ??= GetToken();
        }

        private GlugToken? _last = null;

        public GlugToken GetToken() {
            if (_buff != null) {
                var result = _buff;
                _buff = null;
                return result;
            }

            if (!NextEof()) {
                return _last = _source.Next();
            }

            if (_last != null) {
                throw new GlugUnexpectedEndOfTokenStreamException(_last.Position);
            }

            throw new GlugUnexpectedEndOfTokenStreamException();
        }

        public void Dispose() => _source?.Dispose();

        public bool HasNext() => !NextEof();

        public GlugToken Next() => GetToken();
    }
}
