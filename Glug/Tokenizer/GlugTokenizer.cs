using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using GeminiLab.Core2;
using GeminiLab.Core2.Collections;
using GeminiLab.Core2.Text;
using GeminiLab.Glos;

namespace GeminiLab.Glug.Tokenizer {
    // carefully make all these classes stateless so they can work lock-free. 
    internal class AsciiTrie {
        private const int DefaultNodeSize = 256;
        private const int MaxAscii = 128;
        private const int Occupied = sizeof(GlugTokenType) / sizeof(short) + 1;

        private unsafe struct Node {
            public GlugTokenType Type;
            public short Parent;
            public fixed short Next[MaxAscii - Occupied];

            public ref short this[char c] => ref Next[c - Occupied];
        }

        private readonly GlosStack<Node> _nodes = new GlosStack<Node>(DefaultNodeSize);


        public GlugTokenType Read(string str, int len, ref int ptr) {
            if (ptr >= len) return GlugTokenType.NotAToken;

            short nptr = 0;
            while (ptr < len) {
                char c = str[ptr];
                if (c < Occupied || c >= MaxAscii || _nodes[nptr][c] == 0) break;
                nptr = _nodes[nptr][c];
                ++ptr;
            }

            while (nptr > 0 && _nodes[nptr].Type == GlugTokenType.NotAToken) {
                nptr = _nodes[nptr].Parent;
                --ptr;
            }

            return _nodes[nptr].Type;
        }

        public void Insert(string str, GlugTokenType type) {
            if (str.Length <= 0) throw new ArgumentOutOfRangeException(nameof(str));

            short ptr = 0;
            foreach (var c in str) {
                if (c < Occupied || c >= MaxAscii) throw new ArgumentOutOfRangeException(nameof(str));
                if (_nodes[ptr][c] == 0) {
                    _nodes[ptr][c] = (short)_nodes.Count;
                    ref var nxt = ref _nodes.PushStack();
                    nxt.Type = GlugTokenType.NotAToken;
                    nxt.Parent = ptr;
                }

                ptr = _nodes[ptr][c];
            }

            _nodes[ptr].Type = type;
        }

        public AsciiTrie(Dictionary<string, GlugTokenType> values) {
            _nodes.PushStack().Type = GlugTokenType.NotAToken;
            foreach (var (str, type) in values) Insert(str, type);
        }
    }

    public class GlugTokenizer: IGlugTokenStream {
        private static long ReadDecimalInteger(string value, int len, ref int ptr) {
            long rv = 0;

            if (len - ptr >= 3 && value[ptr] == '0' && (value[ptr + 1] == 'x' || value[ptr + 1] == 'X') && value[ptr + 2].IsHexadecimalDigit()) {
                ptr += 2;
                while (ptr < len) {
                    unchecked {
                        char c = value[ptr];
                        if (c.IsHexadecimalDigit()) {
                            if (c.IsDecimalDigit()) rv = rv * 16 + (c - '0');
                            else rv = rv * 16 + ((c & 0xdf) - 'A' + 10);
                        } else {
                            break;
                        }

                        ++ptr;
                    }
                }
            } else {
                while (ptr < len) {
                    unchecked {
                        char c = value[ptr];
                        if (c.IsDecimalDigit()) rv = rv * 10 + (c - '0');
                        else break;
                        ++ptr;
                    }
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

        private static string ReadIdentifier(string str, int len, ref int ptr) {
            int b = ptr;

            while (ptr < len) {
                if (!IsIdentifierChar(str[ptr])) break;
                ++ptr;
            }

            return str.Substring(b, ptr - b);
        }

        private static readonly AsciiTrie Trie;

        static GlugTokenizer() {
            Trie = new AsciiTrie(new Dictionary<string, GlugTokenType> {
                ["+"] = GlugTokenType.SymbolAdd,
                ["-"] = GlugTokenType.SymbolSub,
                ["*"] = GlugTokenType.SymbolMul,
                ["/"] = GlugTokenType.SymbolDiv,
                ["%"] = GlugTokenType.SymbolMod,
                ["<<"] = GlugTokenType.SymbolLsh,
                [">>"] = GlugTokenType.SymbolRsh,
                ["&"] = GlugTokenType.SymbolAnd,
                ["|"] = GlugTokenType.SymbolOrr,
                ["^"] = GlugTokenType.SymbolXor,
                ["~"] = GlugTokenType.SymbolNot,
                [">"] = GlugTokenType.SymbolGtr,
                ["<"] = GlugTokenType.SymbolLss,
                [">="] = GlugTokenType.SymbolGeq,
                ["<="] = GlugTokenType.SymbolLeq,
                ["=="] = GlugTokenType.SymbolEqu,
                ["~="] = GlugTokenType.SymbolNeq,

                ["("] = GlugTokenType.SymbolLParen,
                [")"] = GlugTokenType.SymbolRParen,
                ["{"] = GlugTokenType.SymbolLBrace,
                ["}"] = GlugTokenType.SymbolRBrace,
                ["["] = GlugTokenType.SymbolLBracket,
                ["]"] = GlugTokenType.SymbolRBracket,
                ["="] = GlugTokenType.SymbolAssign,
                ["\\"] = GlugTokenType.SymbolBackslash,
                [";"] = GlugTokenType.SymbolSemicolon,
                [","] = GlugTokenType.SymbolComma,
                ["->"] = GlugTokenType.SymbolRArrow,
                ["!"] = GlugTokenType.SymbolBang,
                ["!!"] = GlugTokenType.SymbolBangBang,
                ["."] = GlugTokenType.SymbolDot,
                ["`"] = GlugTokenType.SymbolBackquote,
                [":"] = GlugTokenType.SymbolColon,

                ["$"] = GlugTokenType.SymbolDollar,
                ["@"] = GlugTokenType.SymbolAt,
                ["'"] = GlugTokenType.SymbolQuote,
                [".."] = GlugTokenType.SymbolDotDot,
            });
        }

        private static IEnumerable<GlugToken> GetTokensFromLine(string line) {
            int len = line.Length;
            int ptr = 0;

            while (ptr < len) {
                while (ptr < len && line[ptr].IsWhitespace()) ++ptr;
                if (ptr >= len) yield break;

                var type = Trie.Read(line, len, ref ptr);
                if (type != GlugTokenType.NotAToken) {
                    yield return new GlugToken { Type = type };
                    continue;
                }

                char c = line[ptr];
                if (c.IsDecimalDigit()) {
                    yield return new GlugToken { Type = GlugTokenType.LiteralInteger, ValueInt = ReadDecimalInteger(line, len, ref ptr) };
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
                } else if (c == '\"') {
                    var begin = ptr;
                    ++ptr;
                    while (ptr < len) {
                        if (line[ptr] == '\"' && line[ptr - 1] != '\\') break;
                        ++ptr;
                    }

                    yield return new GlugToken { Type = GlugTokenType.LiteralString, ValueString = EscapeSequenceConverter.Decode(line.AsSpan(begin + 1, ptr - begin - 1)) };
                    ++ptr;
                } else if (c == '#') {
                    yield break;
                } else {
                    // TODO: WARN here
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
