using System.Collections.Generic;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Parser;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glute.Compile {
    public class GluteParser : GlugParser {
        public GluteParser(IGlugTokenStream source) : base(source) { }

        protected override bool LikelyExpr(GlugTokenType type) {
            if ((uint)type == (uint)GlugTokenTypeGluteExtension.GlutePlainText || (uint)type == (uint)GlugTokenTypeGluteExtension.GluteInterpolationBegin) return true;
            return base.LikelyExpr(type);
        }

        protected override Expr ReadExprGreedily() {
            var type = (GlugTokenTypeGluteExtension)Stream.PeekToken().Type;

            if (type == GlugTokenTypeGluteExtension.GlutePlainText) {
                var tok = Stream.GetToken();
                return new SysCall(0, new List<Expr> { new LiteralString(tok.ValueString!) }, SysCall.ResultType.None );
            }

            if (type == GlugTokenTypeGluteExtension.GluteInterpolationBegin) {
                Consume((GlugTokenType)GlugTokenTypeGluteExtension.GluteInterpolationBegin);
                var expr = ReadExprGreedily();
                Consume((GlugTokenType)GlugTokenTypeGluteExtension.GluteInterpolationEnd);

                return new SysCall(0, new List<Expr> { expr }, SysCall.ResultType.None);

            }

            return base.ReadExprGreedily();
        }
    }
}
