using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using GeminiLab.Core2;
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
        // true means int
        private static bool ReadNumeric(string value, int len, ref int ptr, out long resultInt, out double resultFloat) {
            if (len - ptr >= 3 && value[ptr] == '0' && (value[ptr + 1] == 'x' || value[ptr + 1] == 'X') && value[ptr + 2].IsHexadecimalDigit()) {
                long rv = 0;
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

                resultInt = rv;
                resultFloat = double.NaN;

                return true;
            } else {
                long rv = 0;
                var begin = ptr;

                while (ptr < len) {
                    unchecked {
                        char c = value[ptr];
                        if (c.IsDecimalDigit()) rv = rv * 10 + (c - '0');
                        else break;
                        ++ptr;
                    }
                }

                if (ptr + 1 < len && value[ptr] == '.' && value[ptr + 1].IsDecimalDigit()) {
                    var dot = ptr;
                    ++ptr;
                    while (ptr < len && value[ptr].IsDecimalDigit()) ++ptr;

                    if (double.TryParse(value.AsSpan(begin, ptr - begin), out var flt)) {
                        resultInt = 0;
                        resultFloat = flt;

                        return false;
                    } else {
                        ptr = dot;
                        resultInt = rv;
                        resultFloat = double.NaN;

                        return true;
                    }
                } else {
                    resultInt = rv;
                    resultFloat = double.NaN;

                    return true;
                }
            }
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
                ["?"] = GlugTokenType.SymbolQuery,
                
                ["&&"] = GlugTokenType.SymbolAndAnd,
                ["||"] = GlugTokenType.SymbolOrrOrr,
                ["??"] = GlugTokenType.SymbolQueryQuery,
                
                [".!"] = GlugTokenType.SymbolDotBang,
                ["@!"] = GlugTokenType.SymbolAtBang,
                
                ["<|"] = GlugTokenType.SymbolBra,
                ["|>"] = GlugTokenType.SymbolKet,
                ["{|"] = GlugTokenType.SymbolVecBegin,
                ["|}"] = GlugTokenType.SymbolVecEnd,
            });
        }

        private static GlugToken? ReadNextToken(string line, ref int ptr, string source, int row) {
            int len = line.Length;

            for (;;) {
                while (ptr < len && line[ptr].IsWhitespace()) ++ptr;
                if (ptr >= len) return null;

                var rv = new GlugToken { Source = source, Row = row, Column = ptr + 1 };

                var type = Trie.Read(line, len, ref ptr);
                if (type != GlugTokenType.NotAToken) {
                    rv.Type = type;
                    return rv;
                }

                char c = line[ptr];
                if (c.IsDecimalDigit()) {
                    if (ReadNumeric(line, len, ref ptr, out var ri, out var rf)) {
                        rv.Type = GlugTokenType.LiteralInteger;
                        rv.ValueInt = ri;
                    } else {
                        IntegerFloatUnion ifu = default;
                        ifu.Float = rf;
                        rv.Type = GlugTokenType.LiteralFloat;
                        rv.ValueInt = ifu.Integer;
                    }

                    return rv;
                }

                if (IsIdentifierLeadingChar(c)) {
                    var id = ReadIdentifier(line, len, ref ptr);

                    rv.Type = id switch {
                        "nil" => GlugTokenType.LiteralNil,
                        "true" => GlugTokenType.LiteralTrue,
                        "false" => GlugTokenType.LiteralFalse,
                        "if" => GlugTokenType.KeywordIf,
                        "else" => GlugTokenType.KeywordElse,
                        "elif" => GlugTokenType.KeywordElif,
                        "fn" => GlugTokenType.KeywordFn,
                        "return" => GlugTokenType.KeywordReturn,
                        "while" => GlugTokenType.KeywordWhile,
                        "break" => GlugTokenType.KeywordBreak,
                        "for" => GlugTokenType.KeywordFor,
                        _ => GlugTokenType.Identifier,
                    };

                    if (rv.Type == GlugTokenType.Identifier) rv.ValueString = id;
                    return rv;
                }

                if (c == '\"') {
                    var begin = ptr;
                    ++ptr;
                    while (ptr < len) {
                        if (line[ptr] == '\"' && line[ptr - 1] != '\\') break;
                        ++ptr;
                    }

                    // TODO: throw if close quote not found

                    rv.Type = GlugTokenType.LiteralString;
                    rv.ValueString = EscapeSequenceConverter.Decode(line.AsSpan(begin + 1, ptr - begin - 1));
                    ++ptr;

                    return rv;
                }

                if (c == '#') return null;

                // TODO: WARN here
                ++ptr;
            }
        }
        
        public TextReader Source { get; }
        
        public string SourceName { get; }

        public GlugTokenizer(TextReader source, string? sourceName = null) {
            Source = source;
            SourceName = sourceName ?? "<anonymous>";
            _eof = false;
            _currLine = null;
            _row = 0;
        }

        private bool _eof;
        private string? _currLine;
        private int _row;
        private int _ptr;
        private GlugToken? _buffer = null;

        private bool readNewLine() {
            _currLine = Source.ReadLine();
            ++_row;
            _ptr = 0;

            _eof = _currLine == null;
            return !_eof;
        }

        private GlugToken? readNextToken() {
            if (_eof) return null;
            if ((_currLine == null || _ptr >= _currLine.Length) && !readNewLine()) return null;

            GlugToken? tok = null;
            do tok = ReadNextToken(_currLine!, ref _ptr, SourceName, _row); while (tok == null && readNewLine());
            return tok;
        }

        public bool HasNext() {
            if (_disposed) throw new ObjectDisposedException(nameof(GlugTokenizer));
            return (_buffer ??= readNextToken()) != null;
        }

        public GlugToken Next() {
            if (_disposed) throw new ObjectDisposedException(nameof(GlugTokenizer));
            
            if (_buffer != null) {
                var rv = _buffer;
                _buffer = null;
                return rv;
            }

            return readNextToken() ?? throw new InvalidOperationException();
        }

        #region IDisposable Support
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    Source?.Dispose();
                }

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
