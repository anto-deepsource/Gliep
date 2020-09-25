using GeminiLab.Glos;

namespace GeminiLab.Glug.Tokenizer {
    public class GlugToken {
        public GlugTokenType    Type;
        public long             ValueInt    = 0;
        public string?          ValueString = null;
        public PositionInSource Position;
    }
}
