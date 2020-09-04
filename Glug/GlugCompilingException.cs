using System;
using System.Collections.Generic;
using System.Linq;
using GeminiLab.Core2;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug {
    public abstract class GlugCompilingException : Exception {
        public PositionInSource Position { get; }

        public GlugCompilingException(PositionInSource position, string? message = null, Exception? innerException = null)
            : base(message ?? String.Format(i18n.Strings.DefaultMessageGlugCompilingException, position), innerException) {
            Position = position;
        }
    }

    public class GlugUnexpectedTokenException : GlugCompilingException {
        public GlugTokenType Actual { get; }
        public IList<GlugTokenType> Expected { get; }

        public GlugUnexpectedTokenException(GlugTokenType actual, IList<GlugTokenType> expected, PositionInSource position, string? message = null, Exception? innerException = null)
            : base(position, message ?? String.Format(i18n.Strings.DefaultMessageGlugUnexpectedTokenException, actual, expected.Select(e => e.ToString()).JoinBy(", ")), innerException) {
            Actual = actual;
            Expected = expected;
        }
    }

    public class GlugUnexpectedEndOfTokenStreamException : GlugCompilingException {
        public GlugUnexpectedEndOfTokenStreamException(PositionInSource position, string? message = null, Exception? innerException = null)
            : base(position, message ?? String.Format(i18n.Strings.DefaultMessageGlugUnexpectedEndOfTokenStreamException, position), innerException) { }

        public GlugUnexpectedEndOfTokenStreamException(string? message = null, Exception? innerException = null)
            : base(PositionInSource.NotAPosition(), message ?? i18n.Strings.DefaultMessageGlugUnexpectedEndOfTokenStreamExceptionNoLastToken, innerException) { }
    }
}
