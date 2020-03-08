using System;
using GeminiLab.Core2.Yielder;

namespace GeminiLab.Glug.Tokenizer {
    public interface IGlugTokenStream: IDisposable, IFiniteYielder<GlugToken> { }
}
