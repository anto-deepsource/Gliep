using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeminiLab.Glug.Tokenizer;
using GeminiLab.Core2.Yielder;

namespace GeminiLab.Glute.Compile {
    public enum GlugTokenTypeGluteExtension {
        GlutePlainText              = GluteTokenizer.GlutePrivateTokenCategory << 20 | 0x000_01,
        GluteInterpolationBegin     = GluteTokenizer.GlutePrivateTokenCategory << 20 | 0x001_00,
        GluteInterpolationEnd       = GluteTokenizer.GlutePrivateTokenCategory << 20 | 0x002_00,
    }

    public class GluteTokenizer : IGlugTokenStream {
        public const GlugTokenTypeCategory GlutePrivateTokenCategory = GlugTokenTypeCategory.PrivateUseHead + 0x6e;

        private readonly TextReader _source;
        private readonly IFiniteYielder<GlugToken> _stream;
        public GluteTokenizer(TextReader source) {
            _source = source;
            _stream = FragmentsToStream(SourceToFragments(_source)).AsFiniteYielder().Concat();
        }


        private enum FragmentType {
            Text,
            Code,
            SuppressedCode,
        }

        private static IEnumerable<(string Content, FragmentType Type)> SourceToFragments(TextReader source) {
            bool inCode = false;
            bool currentSuppressed = false;
            bool currentComment = false;
            var sb = new StringBuilder();

            string? line;
            while ((line = source.ReadLine()) != null) {
                var len = line.Length;
                int ptr = 0, idx;
                bool nlIgnored = false;

                while ((idx = line.IndexOf(inCode ? @"~>" : @"<~", ptr, StringComparison.Ordinal)) >= 0) {
                    int newPtr = idx + 2;
                    bool newCC = false;
                    bool newCS = false;
                    if (inCode) {
                        if (idx == len - 2 && ptr < idx && line[idx - 1] == '~') {
                            nlIgnored = true;
                            idx -= 1;
                            newPtr = idx + 3;
                        }
                    } else {
                        if (idx < len - 2) {
                            if (line[idx + 2] == '#') {
                                newCC = true;
                                newPtr = idx + 3;
                            } else if (line[idx + 2] == '~') {
                                newCS = true;
                                newPtr = idx + 3;
                            }
                        }
                    }

                    sb.Append(line[ptr..idx]);
                    if (!currentComment && !(!inCode && sb.Length == 0)) yield return (sb.ToString(), !inCode ? FragmentType.Text : currentSuppressed ? FragmentType.SuppressedCode : FragmentType.Code);

                    inCode = !inCode;
                    ptr = newPtr;
                    currentComment = newCC;
                    currentSuppressed = newCS;

                    sb.Clear();
                }

                if (ptr >= len) {
                    if (!nlIgnored) {
                        sb.Append('\n');
                    }
                } else {
                    sb.Append(line[ptr..]);
                    sb.Append('\n');
                }
            }

            if (sb.Length > 0) {
                if (!inCode) {
                    yield return (sb.ToString(), FragmentType.Text);
                } else {
                    throw new InvalidOperationException();
                }
            }
        }

        private static IEnumerable<IFiniteYielder<GlugToken>> FragmentsToStream(IEnumerable<(string Content, FragmentType Type)> frags) {
            foreach (var frag in frags) {
                yield return frag.Type switch {
                    FragmentType.Text => FiniteYielderG.SingleYielder(new GlugToken { Type = (GlugTokenType)GlugTokenTypeGluteExtension.GlutePlainText, ValueString = frag.Content }),
                    FragmentType.Code => new[] {
                        FiniteYielderG.SingleYielder(new GlugToken { Type = (GlugTokenType)GlugTokenTypeGluteExtension.GluteInterpolationBegin }),
                        new GlugTokenizer(new StringReader(frag.Content)),
                        FiniteYielderG.SingleYielder(new GlugToken { Type = (GlugTokenType)GlugTokenTypeGluteExtension.GluteInterpolationEnd }),
                    }.AsFiniteYielder().Concat(),
                    FragmentType.SuppressedCode => new GlugTokenizer(new StringReader(frag.Content)),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public bool HasNext() => _stream.HasNext();

        public GlugToken Next() {
            var tok = _stream.Next();
            tok.Source = "";
            tok.Column = tok.Row = 0;
            return tok;
        }

        #region IDisposable Support
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _source?.Dispose();
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

    internal static class FiniteYielderG {
        internal class FiniteYielderConcat<T> : IFiniteYielder<T> {
            public FiniteYielderConcat(IFiniteYielder<IFiniteYielder<T>> source) {
                _source = source;
                _current = null;
            }

            private readonly IFiniteYielder<IFiniteYielder<T>> _source;
            private IFiniteYielder<T>? _current;

            public bool HasNext() {
                if (_current == null || !_current.HasNext()) {
                    if (_source.HasNext()) {
                        _current = _source.Next();
                    } else {
                        return false;
                    }
                }

                return _current.HasNext();
            }

            public T Next() {
                while (_current == null || !_current.HasNext()) {
                    _current = _source.Next();
                }

                return _current.Next();
            }
        }
        
        public static IFiniteYielder<T> Concat<T>(this IFiniteYielder<IFiniteYielder<T>> source) {
            return new FiniteYielderConcat<T>(source);
        }

        public static IFiniteYielder<T> SingleYielder<T>(T value) => FiniteYielder.Const(value, 1);

        internal class Enumerable2FiniteYielderAdapter<T> : IFiniteYielder<T> {
            private readonly IEnumerator<T> _en;

            public Enumerable2FiniteYielderAdapter(IEnumerable<T> source) {
                _en = source.GetEnumerator();
            }

            private bool _moved = false;

            public bool HasNext() {
                if (_moved) return true;

                if (_en.MoveNext()) {
                    _moved = true;
                    return true;
                }

                return false;
            }

            public T Next() {
                T rv;

                if (_moved) {
                    _moved = false;
                    return _en.Current;
                }

                if (_en.MoveNext()) throw new InvalidOperationException();

                return _en.Current;
            }
        }

        public static IFiniteYielder<T> AsFiniteYielder<T>(this IEnumerable<T> source) {
            return new Enumerable2FiniteYielderAdapter<T>(source);
        }
    }
}
