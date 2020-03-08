using System;

namespace GeminiLab.Glug.Tokenizer {
    public interface IGlugTokenStream: IDisposable {
        GlugToken? GetToken();
    }
}
