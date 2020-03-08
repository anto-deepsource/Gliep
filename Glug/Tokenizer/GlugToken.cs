namespace GeminiLab.Glug.Tokenizer {
    public class GlugToken {
        public GlugTokenType Type { get; set; }
        public long ValueInt { get; set; } = 0;
        public string? ValueString { get; set; } = null;

        public string Source { get; set; } = null!;
        public int Row { get; set; }
        public int Column { get; set; }
    }
}
