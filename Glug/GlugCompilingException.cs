using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GeminiLab.Core2;
using GeminiLab.Glos;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug {
    public abstract class GlugCompilingException : Exception {
        public PositionInSource Position { get; }

        public GlugCompilingException(PositionInSource position, string? message = null, Exception? innerException = null)
            : base(message ?? i18n.Strings.DefaultMessageGlugCompilingException, innerException) {
            Position = position;
        }
    }

    public class GlugUnexpectedTokenException : GlugCompilingException {
        public GlugTokenType        Actual   { get; }
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

    public class GlugDanglingBreakException : GlugCompilingException {
        public GlugDanglingBreakException(PositionInSource position, string? message = null, Exception? innerException = null)
            : base(position, message ?? i18n.Strings.DefaultMessageGlugDanglingBreakException, innerException) { }
    }

    public enum PseudoExpressionType {
        VectorHead,
        VectorEnd,
        Discard,
    }

    public class GlugEvaluationOfPseudoExpressionException : GlugCompilingException {
        public PseudoExpressionType Expression { get; }

        public GlugEvaluationOfPseudoExpressionException(PseudoExpressionType expression, PositionInSource position, string? message = null, Exception? innerException = null)
            : base(position, message ?? i18n.Strings.DefaultMessageGlugEvaluationOfPseudoExpressionException, innerException) {
            Expression = expression;
        }
    }

    public class GlugAssignToUnassignableExpressionException : GlugCompilingException {
        public PositionInSource AssigneePosition { get; }

        public GlugAssignToUnassignableExpressionException(PositionInSource assignee, PositionInSource assignment, string? message = null, Exception? innerException = null)
            : base(assignment, message ?? string.Format(i18n.Strings.DefaultMessageGlugAssignToUnassignableExpressionException, assignee), innerException) {
            AssigneePosition = assignee;
        }
    }

    [ExcludeFromCodeCoverage]
    public class GlugInternalException : Exception { }
}
