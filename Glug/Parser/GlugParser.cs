using System;
using System.Collections.Generic;
using System.Linq;
using GeminiLab.Core2;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.Parser {
    public class GlugParser {
        protected virtual string DefaultLambdaName(List<string> paramList, GlugToken tok) => $"<lambda[{paramList.JoinBy(", ")}] at {tok.Source}:{tok.Row}:{tok.Column}>";

        protected virtual bool LikelyExpr(GlugTokenType type) {
            return type.GetCategory() == GlugTokenTypeCategory.Literal
                   || IsUnOp(type)
                   || type == GlugTokenType.Identifier
                   || type == GlugTokenType.SymbolLParen
                   || type == GlugTokenType.KeywordIf
                   || type == GlugTokenType.KeywordWhile
                   || type == GlugTokenType.KeywordReturn
                   || type == GlugTokenType.KeywordFn
                   || type == GlugTokenType.SymbolLBracket
                   || type == GlugTokenType.SymbolLBrace
                   || type == GlugTokenType.SymbolBang
                   || type == GlugTokenType.SymbolBangBang
                   || type == GlugTokenType.SymbolBackquote
                ;
        }

        protected virtual GlugBiOpType BiOpFromTokenType(GlugTokenType op) => op switch {
            GlugTokenType.SymbolAssign => GlugBiOpType.Assign,
            GlugTokenType.SymbolOrr => GlugBiOpType.Orr,
            GlugTokenType.SymbolXor => GlugBiOpType.Xor,
            GlugTokenType.SymbolAnd => GlugBiOpType.And,
            GlugTokenType.SymbolEqu => GlugBiOpType.Equ,
            GlugTokenType.SymbolNeq => GlugBiOpType.Neq,
            GlugTokenType.SymbolGtr => GlugBiOpType.Gtr,
            GlugTokenType.SymbolLss => GlugBiOpType.Lss,
            GlugTokenType.SymbolGeq => GlugBiOpType.Geq,
            GlugTokenType.SymbolLeq => GlugBiOpType.Leq,
            GlugTokenType.SymbolLsh => GlugBiOpType.Lsh,
            GlugTokenType.SymbolRsh => GlugBiOpType.Rsh,
            GlugTokenType.SymbolAdd => GlugBiOpType.Add,
            GlugTokenType.SymbolSub => GlugBiOpType.Sub,
            GlugTokenType.SymbolMul => GlugBiOpType.Mul,
            GlugTokenType.SymbolDiv => GlugBiOpType.Div,
            GlugTokenType.SymbolMod => GlugBiOpType.Mod,
            GlugTokenType.SymbolDotDot => GlugBiOpType.Concat,
            GlugTokenType.SymbolDollar => GlugBiOpType.Call,
            GlugTokenType.OpCall => GlugBiOpType.Call,
            GlugTokenType.SymbolAt => GlugBiOpType.Index,
            GlugTokenType.SymbolDot => GlugBiOpType.Index,
            _ => throw new ArgumentOutOfRangeException(),
        };

        protected virtual int BiOpPrecedence(GlugTokenType op) => op switch {
            GlugTokenType.SymbolAssign => 0x05,
            GlugTokenType.SymbolOrr => 0x10,
            GlugTokenType.SymbolXor => 0x20,
            GlugTokenType.SymbolAnd => 0x30,
            GlugTokenType.SymbolEqu => 0x40,
            GlugTokenType.SymbolNeq => 0x40,
            GlugTokenType.SymbolGtr => 0x50,
            GlugTokenType.SymbolLss => 0x50,
            GlugTokenType.SymbolGeq => 0x50,
            GlugTokenType.SymbolLeq => 0x50,
            GlugTokenType.SymbolLsh => 0x60,
            GlugTokenType.SymbolRsh => 0x60,
            GlugTokenType.SymbolAdd => 0x70,
            GlugTokenType.SymbolSub => 0x70,
            GlugTokenType.SymbolMul => 0x80,
            GlugTokenType.SymbolDiv => 0x80,
            GlugTokenType.SymbolMod => 0x80,
            GlugTokenType.SymbolDotDot => 0x85,
            GlugTokenType.SymbolDollar => 0x90,
            GlugTokenType.OpCall => 0xa0,
            GlugTokenType.SymbolAt => 0xb0,
            GlugTokenType.SymbolDot => 0xc0,
            _ => -1,
        };

        // contract: all operators with same precedence have same associativity
        protected virtual bool BiOpLeftAssociativity(GlugTokenType op) => op switch {
            GlugTokenType.SymbolAssign => false,
            GlugTokenType.SymbolOrr => true,
            GlugTokenType.SymbolXor => true,
            GlugTokenType.SymbolAnd => true,
            GlugTokenType.SymbolEqu => true,
            GlugTokenType.SymbolNeq => true,
            GlugTokenType.SymbolGtr => true,
            GlugTokenType.SymbolLss => true,
            GlugTokenType.SymbolGeq => true,
            GlugTokenType.SymbolLeq => true,
            GlugTokenType.SymbolLsh => true,
            GlugTokenType.SymbolRsh => true,
            GlugTokenType.SymbolAdd => true,
            GlugTokenType.SymbolSub => true,
            GlugTokenType.SymbolMul => true,
            GlugTokenType.SymbolDiv => true,
            GlugTokenType.SymbolMod => true,
            GlugTokenType.SymbolDotDot => false, // actually it doesn't really matter, but i prefer right-associated operators, so ...
            GlugTokenType.SymbolDollar => false,
            GlugTokenType.OpCall => true,
            GlugTokenType.SymbolAt => true,
            GlugTokenType.SymbolDot => true,
            _ => throw new ArgumentOutOfRangeException(),
        };

        protected virtual bool IsUnOp(GlugTokenType op) => 
            op == GlugTokenType.SymbolSub ||
            op == GlugTokenType.SymbolNot ||
            op == GlugTokenType.SymbolQuote;

        protected virtual GlugUnOpType UnOpFromToken(GlugTokenType op) => op switch {
            GlugTokenType.SymbolSub => GlugUnOpType.Neg,
            GlugTokenType.SymbolNot => GlugUnOpType.Not,
            GlugTokenType.SymbolQuote => GlugUnOpType.Typeof,
            _ => throw new ArgumentOutOfRangeException(),
        };

        protected virtual void Consume(GlugTokenType expected) {
            if (expected != Stream.GetToken().Type) throw new ArgumentOutOfRangeException();
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

            return new Block(statements);
        }

        protected virtual Expr ReadExprGreedily() {
            var s = new List<(Expr expr, GlugTokenType op)>();
            Expr expr;
            int lastP = 0;
            bool lastDot = false;

            while (true) {
                expr = lastDot ? new LiteralString(ReadIdentifier()) : ReadExprItem();
                
                var op = Stream.NextEof() ? GlugTokenType.NotAToken : Stream.PeekToken().Type;

                if (op == GlugTokenType.SymbolRArrow) {
                    var param = new List<string>();
                    
                    if (expr is VarRef vr) {
                        param.Add(vr.Id);
                    } else if (expr is OnStackList osl && osl.List.All(x => x is VarRef { IsDef: false, IsGlobal: false })) {
                        param.AddRange(osl.List.Cast<VarRef>().Select(x => x.Id));
                    } else {
                        break;
                    }

                    var arrow = Stream.GetToken();
                    expr = new Function(DefaultLambdaName(param, arrow), false, param, ReadExprGreedily());
                }

                int p = BiOpPrecedence(op);

                if (p < 0) {
                    if (LikelyExpr(op)) {
                        p = BiOpPrecedence(op = GlugTokenType.OpCall);
                    } else {
                        break;
                    }
                } else {
                    Stream.GetToken();
                }

                if (p < lastP) {
                    PopUntil(p);
                }

                lastP = p;
                lastDot = op == GlugTokenType.SymbolDot;
                s.Add((expr, op));
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
                        for (int k = j; k < i; ++k) temp = new BiOp(BiOpFromTokenType(s[k].op), temp, s[k + 1].expr);
                        expr = new BiOp(BiOpFromTokenType(s[i].op), temp, expr);
                    } else {
                        for (int k = i; k >= j; --k) expr = new BiOp(BiOpFromTokenType(s[k].op), s[k].expr, expr);
                    }

                    i = j - 1;
                }

                s.RemoveRange(i + 1, len - i - 1);
            }
        }

        protected virtual Expr ReadExprItem() {
            var tok = Stream.PeekToken();

            if (tok.Type.GetCategory() == GlugTokenTypeCategory.Literal) {
                tok = Stream.GetToken();

                return tok.Type switch {
                    GlugTokenType.LiteralTrue => (Expr)new LiteralBool(true),
                    GlugTokenType.LiteralFalse => new LiteralBool(false),
                    GlugTokenType.LiteralInteger => new LiteralInteger(tok.ValueInt),
                    GlugTokenType.LiteralFloat => new LiteralFloat(tok.ValueInt),
                    GlugTokenType.LiteralString => new LiteralString(tok.ValueString!),
                    GlugTokenType.LiteralNil => new LiteralNil(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (IsUnOp(tok.Type)) {
                Stream.GetToken();

                if (tok.Type != GlugTokenType.SymbolSub) return new UnOp(UnOpFromToken(tok.Type), ReadExprItem());

                var item = ReadExprItem();
                if (item is LiteralInteger li) return new LiteralInteger(unchecked(-li.Value));
                return new UnOp(GlugUnOpType.Neg, item);

            }
            
            if (tok.Type == GlugTokenType.SymbolBackquote) {
                Stream.GetToken();
                return new Metatable(ReadExprItem());
            }

            VarRef? vr = null;
            if (tok.Type == GlugTokenType.SymbolBang) {
                Stream.GetToken();
                vr = new VarRef(ReadIdentifier(), isDef: true);
            } else if (tok.Type == GlugTokenType.SymbolBangBang) {
                Stream.GetToken();
                vr = new VarRef(ReadIdentifier(), isGlobal: true);
            } else if (tok.Type == GlugTokenType.Identifier) {
                vr = new VarRef(ReadIdentifier());
            }

            if (vr != null) {
                Expr rv = vr;
                while (!Stream.NextEof() && Stream.PeekToken().Type == GlugTokenType.SymbolDot) {
                    Stream.GetToken();
                    rv = new BiOp(GlugBiOpType.Index, rv, new LiteralString(ReadIdentifier()));
                }

                return rv;
            }

            return tok.Type switch {
                GlugTokenType.SymbolLParen => (Expr)ReadBlockInParen(),
                GlugTokenType.KeywordIf => ReadIf(),
                GlugTokenType.KeywordWhile => ReadWhile(),
                GlugTokenType.KeywordBreak => ReadBreak(),
                GlugTokenType.KeywordReturn => ReadReturn(),
                GlugTokenType.KeywordFn => ReadFunction(),
                GlugTokenType.SymbolLBracket => ReadOnStackList(),
                GlugTokenType.SymbolLBrace => ReadTableDef(),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        protected virtual string ReadIdentifier() {
            var tok = Stream.GetToken();

            return tok.Type == GlugTokenType.Identifier ? tok.ValueString! : throw new ArgumentOutOfRangeException();
        }

        protected virtual Block ReadBlockInParen() {
            Consume(GlugTokenType.SymbolLParen);
            var rv = ReadBlock();
            Consume(GlugTokenType.SymbolRParen);
            return rv;
        }

        protected virtual If ReadIf() {
            var branches = new List<IfBranch>();

            Consume(GlugTokenType.KeywordIf);
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

            return new If(branches, block);
        }

        protected virtual While ReadWhile() {
            Expr expr;
            Expr block;

            Consume(GlugTokenType.KeywordWhile);
            expr = ReadBlockInParen();
            block = ReadExprGreedily();

            return new While(expr, block);
        }

        protected virtual Break ReadBreak() {
            Consume(GlugTokenType.KeywordBreak);
            return new Break(ReadOptionalExpr() ?? new OnStackList(new List<Expr>()));
        }

        protected virtual Return ReadReturn() {
            Consume(GlugTokenType.KeywordReturn);
            return new Return(ReadOptionalExpr() ?? new OnStackList(new List<Expr>()));
        }

        protected virtual Expr? ReadOptionalExpr() {
            if (!Stream.NextEof() && LikelyExpr(Stream.PeekToken().Type)) return ReadExprGreedily();
            return null;
        }

        protected virtual Function ReadFunction() {
            var fn = Stream.PeekToken();
            Consume(GlugTokenType.KeywordFn);

            string? name = Stream.PeekToken().Type == GlugTokenType.Identifier ? Stream.GetToken().ValueString : null;

            var plist = ReadOptionalParamList();

            if (Stream.PeekToken().Type == GlugTokenType.SymbolRArrow) {
                Stream.GetToken();
            }

            var block = ReadExprGreedily();

            return new Function(name ?? DefaultLambdaName(plist, fn), name != null, plist, block);
        }

        protected virtual List<string> ReadOptionalParamList() {
            var rv = new List<string>();

            // Optional parameter list will never be the last part of of a legal code
            if (Stream.PeekToken().Type == GlugTokenType.SymbolLBracket) {
                Consume(GlugTokenType.SymbolLBracket);
                for (;;) {
                    var tok = Stream.PeekToken();
                    if (tok.Type == GlugTokenType.Identifier) {
                        rv.Add(tok.ValueString!);
                    } else if (tok.Type == GlugTokenType.SymbolRBracket) {
                        break;
                    } else {
                        throw new ArgumentOutOfRangeException();
                    }
                    Stream.GetToken();

                    if (Stream.PeekToken().Type != GlugTokenType.SymbolComma) {
                        break;
                    }

                    Consume(GlugTokenType.SymbolComma);
                }

                Consume(GlugTokenType.SymbolRBracket);
            }

            return rv;
        }

        protected virtual OnStackList ReadOnStackList() {
            var rv = new List<Expr>();

            Consume(GlugTokenType.SymbolLBracket);
            for (;;) {
                var tok = Stream.PeekToken();
                if (tok.Type == GlugTokenType.SymbolRBracket) break;
                rv.Add(ReadExprGreedily());

                tok = Stream.PeekToken();
                if (tok.Type != GlugTokenType.SymbolComma) {
                    break;
                }

                Consume(GlugTokenType.SymbolComma);
            }

            Consume(GlugTokenType.SymbolRBracket);

            return new OnStackList(rv);
        }

        protected virtual TableDef ReadTableDef() {
            var rv = new List<TableDefPair>();

            Consume(GlugTokenType.SymbolLBrace);

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

                tok = Stream.PeekToken();
                if (tok.Type == GlugTokenType.SymbolComma) Consume(GlugTokenType.SymbolComma);
            }

            Consume(GlugTokenType.SymbolRBrace);

            return new TableDef(rv);
        }
    }
}
