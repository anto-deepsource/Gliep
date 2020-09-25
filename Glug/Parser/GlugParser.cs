using System;
using System.Collections.Generic;
using System.Linq;
using GeminiLab.Core2;
using GeminiLab.Glos;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.Parser {
    public class GlugParser {
        protected virtual string DefaultLambdaName(List<string> paramList, GlugToken tok) => $"<lambda[{paramList.JoinBy(", ")}] at {tok.Position.ToString()}>";

        protected virtual bool LikelyExpr(GlugTokenType type) =>
            type.GetCategory() == GlugTokenTypeCategory.Literal
         || IsSymbolUnOp(type)
         || type == GlugTokenType.Identifier
         || type == GlugTokenType.SymbolLParen
         || type == GlugTokenType.KeywordIf
         || type == GlugTokenType.KeywordWhile
         || type == GlugTokenType.KeywordFor
         || type == GlugTokenType.KeywordReturn
         || type == GlugTokenType.KeywordBreak
         || type == GlugTokenType.KeywordFn
         || type == GlugTokenType.SymbolLBracket
         || type == GlugTokenType.SymbolLBrace
         || type == GlugTokenType.SymbolBang
         || type == GlugTokenType.SymbolBangBang
         || type == GlugTokenType.SymbolBackquote
         || type == GlugTokenType.SymbolVecBegin
         || type == GlugTokenType.SymbolBra
         || type == GlugTokenType.SymbolKet
         || type == GlugTokenType.SymbolDiscard
         || type == GlugTokenType.SymbolDotDotDot;

        protected virtual bool LikelyVarRef(GlugTokenType type) =>
            type == GlugTokenType.Identifier
         || type == GlugTokenType.SymbolBang
         || type == GlugTokenType.SymbolBangBang;

        protected virtual GlugBiOpType BiOpFromTokenType(GlugTokenType op) =>
            op switch {
                GlugTokenType.SymbolAssign     => GlugBiOpType.Assign,
                GlugTokenType.SymbolQueryQuery => GlugBiOpType.NullCoalescing,
                GlugTokenType.SymbolOrrOrr     => GlugBiOpType.ShortCircuitOrr,
                GlugTokenType.SymbolAndAnd     => GlugBiOpType.ShortCircuitAnd,
                GlugTokenType.SymbolOrr        => GlugBiOpType.Orr,
                GlugTokenType.SymbolXor        => GlugBiOpType.Xor,
                GlugTokenType.SymbolAnd        => GlugBiOpType.And,
                GlugTokenType.SymbolEqu        => GlugBiOpType.Equ,
                GlugTokenType.SymbolNeq        => GlugBiOpType.Neq,
                GlugTokenType.SymbolGtr        => GlugBiOpType.Gtr,
                GlugTokenType.SymbolLss        => GlugBiOpType.Lss,
                GlugTokenType.SymbolGeq        => GlugBiOpType.Geq,
                GlugTokenType.SymbolLeq        => GlugBiOpType.Leq,
                GlugTokenType.SymbolLsh        => GlugBiOpType.Lsh,
                GlugTokenType.SymbolRsh        => GlugBiOpType.Rsh,
                GlugTokenType.SymbolAdd        => GlugBiOpType.Add,
                GlugTokenType.SymbolSub        => GlugBiOpType.Sub,
                GlugTokenType.SymbolMul        => GlugBiOpType.Mul,
                GlugTokenType.SymbolDiv        => GlugBiOpType.Div,
                GlugTokenType.SymbolMod        => GlugBiOpType.Mod,
                GlugTokenType.SymbolDotDot     => GlugBiOpType.Concat,
                GlugTokenType.SymbolDollar     => GlugBiOpType.Call,
                GlugTokenType.OpCall           => GlugBiOpType.Call,
                GlugTokenType.SymbolAt         => GlugBiOpType.Index,
                GlugTokenType.SymbolAtBang     => GlugBiOpType.IndexLocal,
                GlugTokenType.SymbolDot        => GlugBiOpType.Index,
                GlugTokenType.SymbolDotBang    => GlugBiOpType.IndexLocal,
                _                              => throw new ArgumentOutOfRangeException(),
            };

        protected virtual int BiOpPrecedence(GlugTokenType op) =>
            op switch {
                GlugTokenType.SymbolAssign     => 0x08,
                GlugTokenType.SymbolQueryQuery => 0x0a,
                GlugTokenType.SymbolOrrOrr     => 0x0c,
                GlugTokenType.SymbolAndAnd     => 0x0e,
                GlugTokenType.SymbolOrr        => 0x10,
                GlugTokenType.SymbolXor        => 0x20,
                GlugTokenType.SymbolAnd        => 0x30,
                GlugTokenType.SymbolEqu        => 0x40,
                GlugTokenType.SymbolNeq        => 0x40,
                GlugTokenType.SymbolGtr        => 0x50,
                GlugTokenType.SymbolLss        => 0x50,
                GlugTokenType.SymbolGeq        => 0x50,
                GlugTokenType.SymbolLeq        => 0x50,
                GlugTokenType.SymbolLsh        => 0x60,
                GlugTokenType.SymbolRsh        => 0x60,
                GlugTokenType.SymbolAdd        => 0x70,
                GlugTokenType.SymbolSub        => 0x70,
                GlugTokenType.SymbolMul        => 0x80,
                GlugTokenType.SymbolDiv        => 0x80,
                GlugTokenType.SymbolMod        => 0x80,
                GlugTokenType.SymbolDotDot     => 0x88,
                GlugTokenType.SymbolDollar     => 0x90,
                GlugTokenType.OpCall           => 0xa0,
                GlugTokenType.SymbolAt         => 0xb0,
                GlugTokenType.SymbolAtBang     => 0xb0,
                GlugTokenType.SymbolDot        => 0xc0,
                GlugTokenType.SymbolDotBang    => 0xc0,
                _                              => -1,
            };

        // contract: all operators with same precedence have same associativity
        protected virtual bool BiOpLeftAssociativity(GlugTokenType op) =>
            op switch {
                GlugTokenType.SymbolAssign     => false,
                GlugTokenType.SymbolQueryQuery => false,
                GlugTokenType.SymbolOrrOrr     => true,
                GlugTokenType.SymbolAndAnd     => true,
                GlugTokenType.SymbolOrr        => true,
                GlugTokenType.SymbolXor        => true,
                GlugTokenType.SymbolAnd        => true,
                GlugTokenType.SymbolEqu        => true,
                GlugTokenType.SymbolNeq        => true,
                GlugTokenType.SymbolGtr        => true,
                GlugTokenType.SymbolLss        => true,
                GlugTokenType.SymbolGeq        => true,
                GlugTokenType.SymbolLeq        => true,
                GlugTokenType.SymbolLsh        => true,
                GlugTokenType.SymbolRsh        => true,
                GlugTokenType.SymbolAdd        => true,
                GlugTokenType.SymbolSub        => true,
                GlugTokenType.SymbolMul        => true,
                GlugTokenType.SymbolDiv        => true,
                GlugTokenType.SymbolMod        => true,
                GlugTokenType.SymbolDotDot     => false, // actually it doesn't really matter, but i prefer right-associated operators, so ...
                GlugTokenType.SymbolDollar     => false,
                GlugTokenType.OpCall           => true,
                GlugTokenType.SymbolAt         => true,
                GlugTokenType.SymbolAtBang     => true,
                GlugTokenType.SymbolDot        => true,
                GlugTokenType.SymbolDotBang    => true,
                _                              => throw new ArgumentOutOfRangeException(),
            };

        protected virtual bool IsSymbolUnOp(GlugTokenType op) =>
            op == GlugTokenType.SymbolSub
         || op == GlugTokenType.SymbolNot
         || op == GlugTokenType.SymbolQuery
         || op == GlugTokenType.SymbolDotDotDot;

        protected virtual GlugUnOpType UnOpFromToken(GlugTokenType op) =>
            op switch {
                GlugTokenType.SymbolSub       => GlugUnOpType.Neg,
                GlugTokenType.SymbolNot       => GlugUnOpType.Not,
                GlugTokenType.SymbolQuery     => GlugUnOpType.IsNil,
                GlugTokenType.SymbolDotDotDot => GlugUnOpType.Unpack,
                _                             => throw new ArgumentOutOfRangeException(),
            };

        protected virtual void Consume(GlugTokenType expected) {
            var tok = Stream.GetToken();
            if (expected != tok.Type) throw new GlugUnexpectedTokenException(tok.Type, new[] { expected }, tok.Position);
        }

        protected virtual PositionInSource ConsumeButPosition(GlugTokenType expected) {
            var tok = Stream.GetToken();
            if (expected != tok.Type) throw new GlugUnexpectedTokenException(tok.Type, new[] { expected }, tok.Position);

            return tok.Position;
        }


        protected readonly LookAheadTokenStream Stream;

        public GlugParser(IGlugTokenStream source) {
            Stream = new LookAheadTokenStream(source);
        }


        public virtual Block Parse() => ReadBlock();

        protected virtual Block ReadBlock() {
            var statements = new List<Expr>();

            while (true) {
                if (Stream.NextEof() || Stream.PeekToken().Type == GlugTokenType.SymbolRParen) break;

                statements.Add(ReadExprGreedily());

                if (!Stream.NextEof() && Stream.PeekToken().Type == GlugTokenType.SymbolSemicolon) Consume(GlugTokenType.SymbolSemicolon);
            }

            return new Block(statements).WithPosition(statements.Count > 0 ? statements[0].Position : PositionInSource.NotAPosition());
        }

        protected virtual Expr ReadExprGreedily() {
            var s = new List<(Expr expr, GlugTokenType op, PositionInSource pos)>();
            Expr expr;
            int lastP = 0;
            bool lastDot = false;

            while (true) {
                expr = lastDot ? new LiteralString(ReadIdentifier()) : ReadExprItem();

                var op = Stream.NextEof() ? GlugTokenType.NotAToken : Stream.PeekToken().Type;

                if (op == GlugTokenType.SymbolRArrow) {
                    var arrow = Stream.GetToken();
                    var param = new List<string>();

                    if (expr is VarRef { IsDef: false, IsGlobal: false } vr) {
                        param.Add(vr.Id);
                    } else if (expr is OnStackList osl && osl.List.All(x => x.Type == CommaExprListItemType.Plain && x.Expr is VarRef { IsDef: false, IsGlobal: false })) {
                        param.AddRange(osl.List.Select(x => ((VarRef) x.Expr).Id));
                    } else {
                        throw new GlugUnexpectedTokenException(op, new List<GlugTokenType>(), arrow.Position);
                    }

                    expr = new Function(DefaultLambdaName(param, arrow), false, param, ReadExprGreedily()).WithPosition(arrow.Position);
                }

                int p = BiOpPrecedence(op);
                var pos = PositionInSource.NotAPosition();

                if (p < 0) {
                    if (LikelyExpr(op)) {
                        p = BiOpPrecedence(op = GlugTokenType.OpCall);
                    } else {
                        break;
                    }
                } else {
                    pos = Stream.GetToken().Position;
                }

                if (p < lastP) {
                    PopUntil(p);
                }

                lastP = p;
                lastDot = op == GlugTokenType.SymbolDot || op == GlugTokenType.SymbolDotBang;
                s.Add((expr, op, pos)); // TODO: a position for call
            }

            PopUntil(0);
            return expr;

            void PopUntil(int limit) {
                int len = s.Count;
                int i = len - 1;
                while (i >= 0) {
                    int p = BiOpPrecedence(s[i].op);
                    if (p <= limit) break;

                    int j = i;
                    while (j > 0 && BiOpPrecedence(s[j - 1].op) == p) --j;

                    if (BiOpLeftAssociativity(s[j].op)) {
                        var temp = s[j].expr;
                        for (int k = j; k < i; ++k) temp = new BiOp(BiOpFromTokenType(s[k].op), temp, s[k + 1].expr).WithPosition(s[k].pos);
                        expr = new BiOp(BiOpFromTokenType(s[i].op), temp, expr).WithPosition(s[i].pos);
                    } else {
                        for (int k = i; k >= j; --k) expr = new BiOp(BiOpFromTokenType(s[k].op), s[k].expr, expr).WithPosition(s[k].pos);
                    }

                    i = j - 1;
                }

                s.RemoveRange(i + 1, len - i - 1);
            }
        }

        protected virtual Expr ReadExprItem() {
            var tok = Stream.PeekToken();

            if (tok.Type.GetCategory() == GlugTokenTypeCategory.Literal) {
                Stream.GetToken();

                return (tok.Type switch {
                    GlugTokenType.LiteralTrue    => (Expr) new LiteralBool(true),
                    GlugTokenType.LiteralFalse   => new LiteralBool(false),
                    GlugTokenType.LiteralInteger => new LiteralInteger(tok.ValueInt),
                    GlugTokenType.LiteralFloat   => new LiteralFloat(tok.ValueInt),
                    GlugTokenType.LiteralString  => new LiteralString(tok.ValueString!),
                    GlugTokenType.LiteralNil     => new LiteralNil(),
                    _                            => throw new ArgumentOutOfRangeException()
                }).WithPosition(tok.Position);
            }

            if (IsSymbolUnOp(tok.Type)) {
                Stream.GetToken();

                var item = ReadExprItem();

                return (tok.Type switch {
                    GlugTokenType.SymbolSub when item is LiteralInteger li => (Expr) new LiteralInteger(unchecked(-li.Value)),
                    _                                                      => new UnOp(UnOpFromToken(tok.Type), item),
                }).WithPosition(tok.Position);
            }

            if (tok.Type == GlugTokenType.SymbolBackquote) {
                Stream.GetToken();

                return (ReadIdentifier() switch {
                    "meta"   => (Expr) new Metatable(ReadExprItem()),
                    "type"   => new UnOp(GlugUnOpType.Typeof, ReadExprItem()),
                    "isnil"  => new UnOp(GlugUnOpType.IsNil, ReadExprItem()),
                    "neg"    => new UnOp(GlugUnOpType.Neg, ReadExprItem()),
                    "not"    => new UnOp(GlugUnOpType.Not, ReadExprItem()),
                    "unpack" => new UnOp(GlugUnOpType.Unpack, ReadExprItem()),
                    _        => throw new ArgumentOutOfRangeException(), // TODO: add a custom exception class
                }).WithPosition(tok.Position);
            }

            if (LikelyVarRef(tok.Type)) {
                Expr rv = ReadVarRef();
                while (!Stream.NextEof() && Stream.PeekToken().Type == GlugTokenType.SymbolDot) {
                    tok = Stream.GetToken();
                    rv = new BiOp(GlugBiOpType.Index, rv, new LiteralString(ReadIdentifier())).WithPosition(tok.Position);
                }

                return rv;
            }

            if (tok.Type == GlugTokenType.SymbolBra || tok.Type == GlugTokenType.SymbolKet) {
                return new PseudoIndex(tok.Type == GlugTokenType.SymbolKet).WithPosition(Stream.GetToken().Position);
            }

            if (tok.Type == GlugTokenType.SymbolDiscard) {
                return new Discard().WithPosition(Stream.GetToken().Position);
            }

            return tok.Type switch {
                GlugTokenType.SymbolLParen   => (Expr) ReadBlockInParen(),
                GlugTokenType.KeywordIf      => ReadIf(),
                GlugTokenType.KeywordWhile   => ReadWhile(),
                GlugTokenType.KeywordFor     => ReadFor(),
                GlugTokenType.KeywordBreak   => ReadBreak(),
                GlugTokenType.KeywordReturn  => ReadReturn(),
                GlugTokenType.KeywordFn      => ReadFunction(),
                GlugTokenType.SymbolLBracket => ReadOnStackList(),
                GlugTokenType.SymbolLBrace   => ReadTableDef(),
                GlugTokenType.SymbolVecBegin => ReadVectorDef(),
                _                            => throw new ArgumentOutOfRangeException(), // TODO: custom exception
            };
        }

        protected virtual VarRef ReadVarRef() {
            var tok = Stream.PeekToken();

            if (tok.Type != GlugTokenType.Identifier) Stream.GetToken();

            return (tok.Type switch {
                GlugTokenType.SymbolBang     => new VarRef(ReadIdentifier(), isDef: true),
                GlugTokenType.SymbolBangBang => new VarRef(ReadIdentifier(), isGlobal: true),
                GlugTokenType.Identifier     => new VarRef(ReadIdentifier()),
                _                            => throw new ArgumentOutOfRangeException(),
            }).WithPosition(tok.Position);
        }

        protected virtual string ReadIdentifier() {
            var tok = Stream.GetToken();

            return tok.Type == GlugTokenType.Identifier ? tok.ValueString! : throw new GlugUnexpectedTokenException(tok.Type, new[] { GlugTokenType.Identifier }, tok.Position);
        }

        protected virtual Block ReadBlockInParen() {
            Consume(GlugTokenType.SymbolLParen);
            var rv = ReadBlock();
            Consume(GlugTokenType.SymbolRParen);
            return rv;
        }

        protected virtual If ReadIf() {
            var branches = new List<IfBranch>();
            var pos = ConsumeButPosition(GlugTokenType.KeywordIf);
            var expr = ReadBlockInParen();
            Expr? block = ReadExprGreedily();
            branches.Add(new IfBranch(expr, block));

            while (!Stream.NextEof() && Stream.PeekToken().Type == GlugTokenType.KeywordElif) {
                Consume(GlugTokenType.KeywordElif);
                expr = ReadBlockInParen();
                block = ReadExprGreedily();
                branches.Add(new IfBranch(expr, block));
            }

            if (!Stream.NextEof() && Stream.PeekToken().Type == GlugTokenType.KeywordElse) {
                Consume(GlugTokenType.KeywordElse);
                block = ReadExprGreedily();
            } else {
                block = null;
            }

            return new If(branches, block).WithPosition(pos);
        }

        protected virtual While ReadWhile() {
            var pos = ConsumeButPosition(GlugTokenType.KeywordWhile);

            var label = ReadOptionalControlFlowLabel();
            var expr = ReadBlockInParen();
            var block = ReadExprGreedily();

            return new While(expr, block, label).WithPosition(pos);
        }

        protected virtual string? ReadOptionalControlFlowLabel() {
            if (Stream.PeekToken().Type == GlugTokenType.SymbolQuote) {
                Consume(GlugTokenType.SymbolQuote);
                return ReadIdentifier();
            }

            return null;
        }

        protected virtual For ReadFor() {
            var iv = new List<VarRef>();
            Expr expr, body;

            var pos = ConsumeButPosition(GlugTokenType.KeywordFor);
            var label = ReadOptionalControlFlowLabel();
            Consume(GlugTokenType.SymbolLParen);

            iv.Add(ReadVarRef());
            while (Stream.PeekToken().Type == GlugTokenType.SymbolComma) {
                Consume(GlugTokenType.SymbolComma);
                iv.Add(ReadVarRef());
            }

            Consume(GlugTokenType.SymbolColon);
            expr = ReadExprGreedily();
            Consume(GlugTokenType.SymbolRParen);

            body = ReadExprGreedily();
            return new For(iv, expr, body, label).WithPosition(pos);
        }

        protected virtual Return ReadReturn() {
            var pos = ConsumeButPosition(GlugTokenType.KeywordReturn);
            return new Return(ReadOptionalExpr() ?? new OnStackList()).WithPosition(pos);
        }

        protected virtual Break ReadBreak() {
            var pos = ConsumeButPosition(GlugTokenType.KeywordBreak);
            var label = ReadOptionalControlFlowLabel();
            return new Break(ReadOptionalExpr() ?? new OnStackList(), label).WithPosition(pos);
        }

        protected virtual Expr? ReadOptionalExpr() {
            if (!Stream.NextEof() && LikelyExpr(Stream.PeekToken().Type)) return ReadExprGreedily();

            return null;
        }

        protected virtual Function ReadFunction() {
            var fn = Stream.PeekToken();
            var pos = ConsumeButPosition(GlugTokenType.KeywordFn);

            string? name = Stream.PeekToken().Type == GlugTokenType.Identifier ? Stream.GetToken().ValueString : null;

            var plist = ReadOptionalParamList();

            if (Stream.PeekToken().Type == GlugTokenType.SymbolRArrow) {
                Stream.GetToken();
            }

            var block = ReadExprGreedily();

            return new Function(name ?? DefaultLambdaName(plist, fn), name != null, plist, block).WithPosition(pos);
        }

        protected virtual List<string> ReadOptionalParamList() {
            var rv = new List<string>();

            // Optional parameter list will never be the last part of of a legal code
            if (Stream.PeekToken().Type == GlugTokenType.SymbolLBracket) {
                Consume(GlugTokenType.SymbolLBracket);

                if (Stream.PeekToken().Type != GlugTokenType.SymbolRBracket) {
                    rv.Add(ReadIdentifier());
                    while (Stream.PeekToken().Type == GlugTokenType.SymbolComma) {
                        Consume(GlugTokenType.SymbolComma);
                        rv.Add(ReadIdentifier());
                    }
                }

                Consume(GlugTokenType.SymbolRBracket);
            }

            return rv;
        }

        protected virtual OnStackList ReadOnStackList() {
            var pos = ConsumeButPosition(GlugTokenType.SymbolLBracket);

            var rv = ReadCommaExprList();

            Consume(GlugTokenType.SymbolRBracket);

            return new OnStackList(rv).WithPosition(pos);
        }

        protected virtual IList<CommaExprListItem> ReadCommaExprList() {
            var rv = new List<CommaExprListItem>();

            while (true) {
                var tok = Stream.PeekToken();

                if (tok.Type == GlugTokenType.SymbolDotDot) {
                    Stream.GetToken();
                    rv.Add(new CommaExprListItem(CommaExprListItemType.OnStackListUnpack, ReadExprGreedily()));
                } else if (tok.Type == GlugTokenType.SymbolDotDotDot) {
                    Stream.GetToken();
                    rv.Add(new CommaExprListItem(CommaExprListItemType.VectorUnpack, ReadExprGreedily()));
                } else {
                    if (!LikelyExpr(tok.Type)) break;

                    rv.Add(new CommaExprListItem(CommaExprListItemType.Plain, ReadExprGreedily()));
                }
                
                if (Stream.PeekToken().Type == GlugTokenType.SymbolComma) Consume(GlugTokenType.SymbolComma);
            }

            return rv;
        }
        
        protected virtual TableDef ReadTableDef() {
            var rv = new List<TableDefPair>();

            var pos = ConsumeButPosition(GlugTokenType.SymbolLBrace);

            while (true) {
                var tok = Stream.PeekToken();
                if (tok.Type == GlugTokenType.SymbolRBrace) break;

                Expr key;
                if (tok.Type == GlugTokenType.SymbolDot) {
                    Stream.GetToken();
                    key = new LiteralString(ReadIdentifier());
                } else {
                    if (tok.Type == GlugTokenType.SymbolAt) Stream.GetToken();
                    key = ReadExprGreedily();
                }

                Consume(GlugTokenType.SymbolColon);

                rv.Add(new TableDefPair(key, ReadExprGreedily()));

                if (Stream.PeekToken().Type == GlugTokenType.SymbolComma) Consume(GlugTokenType.SymbolComma);
            }

            Consume(GlugTokenType.SymbolRBrace);

            return new TableDef(rv).WithPosition(pos);
        }

        protected virtual VectorDef ReadVectorDef() {
            var pos = ConsumeButPosition(GlugTokenType.SymbolVecBegin);
            
            var rv = ReadCommaExprList();

            Consume(GlugTokenType.SymbolVecEnd);

            return new VectorDef(rv).WithPosition(pos);
        }
    }
}
