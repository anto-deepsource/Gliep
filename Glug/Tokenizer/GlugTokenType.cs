namespace GeminiLab.Glug.Tokenizer {
    public enum GlugTokenTypeCategory {
        Literal     = 0x000,
        Symbol      = 0x001,
        Pseudo      = 0x002,
        Keyword     = 0x003,
        Identifier  = 0x004,
    }

    // 12-bit category id + 12-bit S/N in category + 8-bit flags
    // flags:
    // bit 7 ... bit 0
    // | not used * 5 | a pseudo-token | with a integer | with a string |
    // pseudo token: won't be used by tokenizer, used in other place
    public enum GlugTokenType : uint {
        LiteralNil      = 0x000_000_00,
        LiteralTrue     = 0x000_001_00,
        LiteralFalse    = 0x000_002_00,
        LiteralInteger  = 0x000_003_02,
        LiteralString   = 0x000_004_01,
        SymbolLParen    = 0x001_000_00,
        SymbolRParen    = 0x001_001_00,
        SymbolLBrace    = 0x001_002_00,
        SymbolRBrace    = 0x001_003_00,
        SymbolLBracket  = 0x001_004_00,
        SymbolRBracket  = 0x001_005_00,
        SymbolAssign    = 0x001_006_00,
        SymbolBackslash = 0x001_007_00,
        SymbolSemicolon = 0x001_008_00,
        SymbolComma     = 0x001_009_00,
        SymbolRArrow    = 0x001_00a_00,
        SymbolBang      = 0x001_00b_00,
        SymbolBangBang  = 0x001_00c_00,
        SymbolDot       = 0x001_00d_00,
        SymbolBackquote = 0x001_00e_00,
        SymbolColon     = 0x001_00f_00,
        SymbolAdd       = 0x001_010_00,
        SymbolSub       = 0x001_011_00,
        SymbolMul       = 0x001_012_00,
        SymbolDiv       = 0x001_013_00,
        SymbolMod       = 0x001_014_00,
        SymbolLsh       = 0x001_015_00,
        SymbolRsh       = 0x001_016_00,
        SymbolAnd       = 0x001_017_00,
        SymbolOrr       = 0x001_018_00,
        SymbolXor       = 0x001_019_00,
        SymbolNot       = 0x001_01a_00,
        SymbolGtr       = 0x001_01b_00,
        SymbolLss       = 0x001_01c_00,
        SymbolGeq       = 0x001_01d_00,
        SymbolLeq       = 0x001_01e_00,
        SymbolEqu       = 0x001_01f_00,
        SymbolNeq       = 0x001_020_00,
        SymbolDollar    = 0x001_021_00,
        SymbolAt        = 0x001_022_00,
        SymbolQuote     = 0x001_023_00,
        OpCall          = 0x002_000_04,
        KeywordIf       = 0x003_001_00,
        KeywordElif     = 0x003_002_00,
        KeywordElse     = 0x003_003_00,
        KeywordFn       = 0x003_004_00,
        KeywordReturn   = 0x003_005_00,
        KeywordWhile    = 0x003_006_00,
        Identifier      = 0x004_000_01,
        NotAToken       = 0x7ff_000_04,
    }

    public static class GlugTokenTypeExtensions {
        public static GlugTokenTypeCategory GetCategory(this GlugTokenType type) {
            return (GlugTokenTypeCategory)((uint)type >> 20);
        }

        public static bool IsPseudo(this GlugTokenType type) {
            return ((uint)type & 0x4) != 0;
        }

        public static bool HasInteger(this GlugTokenType type) {
            return ((uint)type & 0x2) != 0;
        }

        public static bool HasString(this GlugTokenType type) {
            return ((uint)type & 0x1) != 0;
        }
    }
}