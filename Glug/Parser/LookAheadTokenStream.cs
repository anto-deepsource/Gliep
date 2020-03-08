using System;
using System.Runtime.Serialization;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.Parser {
    [Serializable]
    public class UnexpectedEndOfTokenStreamException : Exception {
        public UnexpectedEndOfTokenStreamException() { }
        public UnexpectedEndOfTokenStreamException(string message) : base(message) { }
        public UnexpectedEndOfTokenStreamException(string message, Exception innerException) : base(message, innerException) { }
        protected UnexpectedEndOfTokenStreamException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
    }

    public class LookAheadTokenStream {
        private IGlugTokenStream _source;
        public LookAheadTokenStream(IGlugTokenStream source) {
            _source = source;
        }

        private GlugToken? _buff;

        public GlugToken? PeekToken() {
            return _buff ??= GetToken();
        }

        public GlugToken PeekTokenNonNull() => PeekToken() ?? throw new UnexpectedEndOfTokenStreamException();
        
        public GlugToken? GetToken() {
            if (_buff != null) {
                var rv = _buff;
                _buff = null;
                return rv;
            }

            return _source.GetToken();
        }

        public GlugToken GetTokenNonNull() => GetToken() ?? throw new UnexpectedEndOfTokenStreamException();
    }
}