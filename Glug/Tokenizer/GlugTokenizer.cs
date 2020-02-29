using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GeminiLab.Core2;
using GeminiLab.Core2.Collections;
using GeminiLab.Core2.Text;

namespace GeminiLab.Glug.Tokenizer {
    public class GlugTokenizer: IGlugTokenStream {
        private static long ReadDecimalInteger(string value, int len, ref int ptr) {
            long rv = 0;

            unchecked {
                while (ptr < len) { 
                    if (value[ptr].IsDecimalDigit()) rv = rv * 10 + (value[ptr] - '0');
                    else return rv;
                    ++ptr;
                }
            }

            return rv;
        }

        private static bool IsIdentifierChar(char c) {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            return c == '_' || cat == UnicodeCategory.LowercaseLetter || cat == UnicodeCategory.UppercaseLetter ||
                   cat == UnicodeCategory.OtherLetter || cat == UnicodeCategory.DecimalDigitNumber;
        }
        private static bool IsIdentifierLeadingChar(char c) {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            return c == '_' || cat == UnicodeCategory.LowercaseLetter || cat == UnicodeCategory.UppercaseLetter ||
                   cat == UnicodeCategory.OtherLetter;
        }

        private static string ReadIdentifier(string value, int len, ref int ptr) {
            int b = ptr;

            while (ptr < len) {
                if (!IsIdentifierChar(value[ptr])) return value.Substring(b, ptr - b);
                ++ptr;
            }

            return value.Substring(b, ptr - b);
        }

        // this method is getting more and more nasty, TODO: refactor
        private static IEnumerable<GlugToken> GetTokensFromLine(string line) {
            int len = line.Length;
            int ptr = 0;

            while (ptr < len) {
                while (ptr < len && line[ptr].IsWhitespace()) ++ptr;
                if (ptr >= len) yield break;

                char c = line[ptr];
                bool last = ptr == (len - 1);
                char next = last ? '\0' : line[ptr + 1];
                if (c.IsDecimalDigit()) {
                    yield return new GlugToken { Type = GlugTokenType.LiteralInteger, ValueInt = ReadDecimalInteger(line, len, ref ptr) };
                } else if (c == '-') {
                    if (!last) {
                        if (next.IsDecimalDigit()) {
                            ++ptr;
                            yield return new GlugToken { Type = GlugTokenType.LiteralInteger, ValueInt = unchecked(-ReadDecimalInteger(line, len, ref ptr)) };
                            continue;
                        }
                    }

                    ++ptr;
                    if (!last && next == '>') { ++ptr; yield return new GlugToken {Type = GlugTokenType.SymbolRArrow}; }
                    else yield return new GlugToken { Type = GlugTokenType.OpSub };
                } else if (c == '+') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.OpAdd };
                } else if (c == '*') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.OpMul };
                } else if (c == '/') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.OpDiv };
                } else if (c == '%') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.OpMod };
                } else if (c == '<') {
                    ++ptr;

                    if (last) yield return new GlugToken { Type = GlugTokenType.OpLss };
                    else if (next == '<') { ++ptr; yield return new GlugToken { Type = GlugTokenType.OpLsh }; }
                    else if (next == '=') { ++ptr; yield return new GlugToken { Type = GlugTokenType.OpLeq }; }
                    else yield return new GlugToken { Type = GlugTokenType.OpLss };
                } else if (c == '>') {
                    ++ptr;

                    if (last) yield return new GlugToken { Type = GlugTokenType.OpGtr };
                    else if (next == '>') { ++ptr; yield return new GlugToken { Type = GlugTokenType.OpRsh }; } 
                    else if (next == '=') { ++ptr; yield return new GlugToken { Type = GlugTokenType.OpGeq }; } 
                    else yield return new GlugToken { Type = GlugTokenType.OpGtr };
                } else if (c == '&') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.OpAnd };
                } else if (c == '|') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.OpOrr };
                } else if (c == '^') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.OpXor };
                } else if (c == '~') {
                    ++ptr;

                    if (!last && next == '=') { ++ptr; yield return new GlugToken { Type = GlugTokenType.OpNeq }; }
                    else yield return new GlugToken { Type = GlugTokenType.OpNot };
                } else if (c == '=') {
                    ++ptr;

                    if (!last && next == '=') { ++ptr; yield return new GlugToken {Type = GlugTokenType.OpEqu}; }
                    else yield return new GlugToken { Type = GlugTokenType.SymbolAssign };
                } else if (IsIdentifierLeadingChar(c)) {
                    var id = ReadIdentifier(line, len, ref ptr);

                    yield return id switch {
                        "nil" => new GlugToken { Type = GlugTokenType.LiteralNil },
                        "true" => new GlugToken { Type = GlugTokenType.LiteralTrue },
                        "false" => new GlugToken { Type = GlugTokenType.LiteralFalse },
                        "if" => new GlugToken { Type = GlugTokenType.KeywordIf },
                        "else" => new GlugToken { Type = GlugTokenType.KeywordElse },
                        "elif" => new GlugToken { Type = GlugTokenType.KeywordElif },
                        "fn" => new GlugToken { Type = GlugTokenType.KeywordFn },
                        "return" => new GlugToken { Type = GlugTokenType.KeywordReturn },
                        "while" => new GlugToken { Type = GlugTokenType.KeywordWhile },
                        _ => new GlugToken { Type = GlugTokenType.Identifier, ValueString = id },
                    };
                } else if (c == '{') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolLBrace };
                } else if (c == '}') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolRBrace };
                } else if (c == '(') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolLParen };
                } else if (c == ')') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolRParen };
                } else if (c == '[') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolLBracket };
                } else if (c == ']') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolRBracket };
                } else if (c == ';') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolSemicolon };
                } else if (c == '$') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.OpDollar };
                } else if (c == ',') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolComma };
                } else if (c == '\\') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolBackslash };
                } else if (c == '@') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.OpAt };
                } else if (c == '#') {
                    yield break;
                } else if (c == '\"') {
                    var begin = ptr;
                    ++ptr;
                    while (ptr < len) {
                        if (line[ptr] == '\"' && line[ptr - 1] != '\\') break;
                        ++ptr;
                    }

                    yield return new GlugToken { Type = GlugTokenType.LiteralString, ValueString = EscapeSequenceConverter.Decode(line.AsSpan(begin + 1, ptr - begin - 1)) };
                    ++ptr;
                } else if (c == '!') {
                    ++ptr;
                    if (next == '!') {
                        ++ptr;
                        yield return new GlugToken { Type = GlugTokenType.SymbolBangBang };
                    } else {
                        yield return new GlugToken { Type = GlugTokenType.SymbolBang };
                    }
                } else if (c == '`') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolBackquote };
                } else if (c == '.') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolDot };
                } else if (c == ':') {
                    ++ptr;
                    yield return new GlugToken { Type = GlugTokenType.SymbolColon };
                } else {
                    ++ptr;
                }
            }
        }
        
        private static IEnumerable<string> ReadLines(TextReader reader) {
            string line;
            while ((line = reader.ReadLine()) != null) yield return line;
        }

        private static IEnumerable<GlugToken> GetTokens(TextReader reader) {
            return ReadLines(reader).Select(GetTokensFromLine).Flatten();
        }

        private readonly TextReader _source;
        private IEnumerator<GlugToken> _en;

        public GlugToken? GetToken() {
            if (_disposed) throw new InvalidOperationException();
            return !_en.MoveNext() ? null : _en.Current;
        }

        public GlugTokenizer(TextReader source) {
            _source = source;
            _en = GetTokens(source).GetEnumerator();
        }

        #region IDisposable Support
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _source?.Dispose();
                }

                _en = null!;
                _disposed = true;
            }
        }
        
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
