using System;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.Parser {
    public class GlugParserUnexpectedTokenException : Exception {
        public GlugToken Actual { get; }
        public GlugTokenType Expected { get; }

        public GlugParserUnexpectedTokenException(GlugToken actual)
            : this(actual, GlugTokenType.NotAToken, string.Format(i18n.Strings.DefaultMessageGlugParserUnexpectedTokenExceptionShort, actual.Type)) { }

        public GlugParserUnexpectedTokenException(GlugToken actual, GlugTokenType expected)
            : this(actual, expected, string.Format(i18n.Strings.DefaultMessageGlugParserUnexpectedTokenExceptionLong, actual.Type, expected)) { }

        public GlugParserUnexpectedTokenException(GlugToken actual, GlugTokenType expected, string message)
            : base(message) {
            Actual = actual;
            Expected = expected;
        }
    }
}
