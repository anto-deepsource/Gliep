using System;
using System.Collections.Generic;
using System.Linq;

using GeminiLab.Glug.AST;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.Parser {
    public static class GlugParser {
        private static bool LikelyExpr(GlugTokenType type) {
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

        private static GlugBiOpType TokenToBiOp(GlugTokenType op) => op switch {
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

        private static int Precedence(GlugTokenType op) => op switch {
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
        private static bool LeftAssociate(GlugTokenType op) => op switch {
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

        private static bool IsUnOp(GlugTokenType op) => 
            op == GlugTokenType.SymbolSub ||
            op == GlugTokenType.SymbolNot ||
            op == GlugTokenType.SymbolQuote;

        private static GlugUnOpType TokenToUnOp(GlugTokenType op) => op switch {
            GlugTokenType.SymbolSub => GlugUnOpType.Neg,
            GlugTokenType.SymbolNot => GlugUnOpType.Not,
            GlugTokenType.SymbolQuote => GlugUnOpType.Typeof,
            _ => throw new ArgumentOutOfRangeException(),
        };

        private static void Consume(LookAheadTokenStream stream, GlugTokenType expected) {
            if (expected != stream.GetToken().Type) throw new ArgumentOutOfRangeException();
        }



        public static Block Parse(IGlugTokenStream source) => ReadBlock(new LookAheadTokenStream(source));

        private static Block ReadBlock(LookAheadTokenStream stream) {
            var statements = new List<Expr>();

            while (true) {
                if (stream.NextEof() || stream.PeekToken().Type == GlugTokenType.SymbolRParen) break;

                statements.Add(ReadExprGreedily(stream));

                if (!stream.NextEof() && stream.PeekToken().Type == GlugTokenType.SymbolSemicolon) Consume(stream, GlugTokenType.SymbolSemicolon);
            }

            return new Block(statements);
        }

        private static Expr ReadExprGreedily(LookAheadTokenStream stream) {
            var s = new List<(Expr expr, GlugTokenType op)>();
            Expr expr;
            int lastP = 0;
            bool lastDot = false;

            while (true) {
                expr = lastDot ? new LiteralString(ReadIdentifier(stream)) : ReadExprItem(stream);
                
                var op = stream.NextEof() ? GlugTokenType.NotAToken : stream.PeekToken().Type;

                if (op == GlugTokenType.SymbolRArrow) {
                    if (expr is VarRef vr) {
                        stream.GetToken();
                        expr = new Function(null, new List<string>(new[] { vr.Id }), ReadExprGreedily(stream));
                    } else if (expr is OnStackList osl && osl.IsVarRef) {
                        stream.GetToken();
                        expr = new Function(null, osl.List.Cast<VarRef>().Select(x => x.Id).ToList(), ReadExprGreedily(stream));
                    } else {
                        // let it crash elsewhere
                        break;
                    }
                }

                int p = Precedence(op);

                if (p < 0) {
                    if (LikelyExpr(op)) {
                        p = Precedence(op = GlugTokenType.OpCall);
                    } else {
                        break;
                    }
                } else {
                    stream.GetToken();
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
                    int p = Precedence(s[i].op);
                    if (p <= limit) break;
                    int j = i;
                    while (j > 0 && Precedence(s[j - 1].op) == p) --j;

                    if (LeftAssociate(s[j].op)) {
                        var temp = s[j].expr;
                        for (int k = j; k < i; ++k) temp = new BiOp(TokenToBiOp(s[k].op), temp, s[k + 1].expr);
                        expr = new BiOp(TokenToBiOp(s[i].op), temp, expr);
                    } else {
                        for (int k = i; k >= j; --k) expr = new BiOp(TokenToBiOp(s[k].op), s[k].expr, expr);
                    }

                    i = j - 1;
                }

                s.RemoveRange(i + 1, len - i - 1);
            }
        }

        private static Expr ReadExprItem(LookAheadTokenStream stream) {
            var tok = stream.PeekToken();

            if (tok.Type.GetCategory() == GlugTokenTypeCategory.Literal) {
                tok = stream.GetToken();

                return tok.Type switch {
                    GlugTokenType.LiteralTrue => (Expr)new LiteralBool(true),
                    GlugTokenType.LiteralFalse => new LiteralBool(false),
                    GlugTokenType.LiteralInteger => new LiteralInteger(tok.ValueInt),
                    GlugTokenType.LiteralString => new LiteralString(tok.ValueString!),
                    GlugTokenType.LiteralNil => new LiteralNil(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (IsUnOp(tok.Type)) {
                stream.GetToken();

                if (tok.Type != GlugTokenType.SymbolSub) return new UnOp(TokenToUnOp(tok.Type), ReadExprItem(stream));

                var item = ReadExprItem(stream);
                if (item is LiteralInteger li) return new LiteralInteger(unchecked(-li.Value));
                return new UnOp(GlugUnOpType.Neg, item);

            }
            
            if (tok.Type == GlugTokenType.SymbolBackquote) {
                stream.GetToken();
                return new Metatable(ReadExprItem(stream));
            }

            if (tok.Type == GlugTokenType.SymbolBang) {
                stream.GetToken();
                return new VarRef(ReadIdentifier(stream)) { IsDef = true };
            }

            if (tok.Type == GlugTokenType.SymbolBangBang) {
                stream.GetToken();
                return new VarRef(ReadIdentifier(stream)) { IsGlobal = true };
            }

            if (tok.Type == GlugTokenType.Identifier) {
                return new VarRef(ReadIdentifier(stream));
            }

            return tok.Type switch {
                GlugTokenType.SymbolLParen => (Expr)ReadBlockInParen(stream),
                GlugTokenType.KeywordIf => ReadIf(stream),
                GlugTokenType.KeywordWhile => ReadWhile(stream),
                GlugTokenType.KeywordReturn => ReadReturn(stream),
                GlugTokenType.KeywordFn => ReadFunction(stream),
                GlugTokenType.SymbolLBracket => ReadOnStackList(stream),
                GlugTokenType.SymbolLBrace => ReadTableDef(stream),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private static string ReadIdentifier(LookAheadTokenStream stream) {
            var tok = stream.GetToken();

            return tok.Type == GlugTokenType.Identifier ? tok.ValueString! : throw new ArgumentOutOfRangeException();
        }
        
        private static Block ReadBlockInParen(LookAheadTokenStream stream) {
            Consume(stream, GlugTokenType.SymbolLParen);
            var rv = ReadBlock(stream);
            Consume(stream, GlugTokenType.SymbolRParen);
            return rv;
        }

        private static If ReadIf(LookAheadTokenStream stream) {
            var branches = new List<IfBranch>();

            Consume(stream, GlugTokenType.KeywordIf);
            var expr = ReadBlockInParen(stream);
            Expr? block = ReadExprGreedily(stream);
            branches.Add(new IfBranch(expr, block));

            while (!stream.NextEof() && stream.PeekToken().Type == GlugTokenType.KeywordElif) {
                Consume(stream, GlugTokenType.KeywordElif);
                expr = ReadBlockInParen(stream);
                block = ReadExprGreedily(stream);
                branches.Add(new IfBranch(expr, block));
            }

            if (!stream.NextEof() && stream.PeekToken().Type == GlugTokenType.KeywordElse) {
                Consume(stream, GlugTokenType.KeywordElse);
                block = ReadExprGreedily(stream);
            } else {
                block = null;
            }

            return new If(branches, block);
        }

        private static While ReadWhile(LookAheadTokenStream stream) {
            Expr expr;
            Expr block;

            Consume(stream, GlugTokenType.KeywordWhile);
            expr = ReadBlockInParen(stream);
            block = ReadExprGreedily(stream);

            return new While(expr, block);
        }

        private static Return ReadReturn(LookAheadTokenStream stream) {
            Consume(stream, GlugTokenType.KeywordReturn);
            return new Return(ReadExprGreedily(stream));
        }

        private static Function ReadFunction(LookAheadTokenStream stream, bool skipFn = false) {
            if (!skipFn) Consume(stream, GlugTokenType.KeywordFn);
            string? name = null;
            if (stream.PeekToken().Type == GlugTokenType.Identifier) {
                name = stream.GetToken().ValueString!;
            }

            var plist = ReadOptionalParamList(stream);

            if (stream.PeekToken().Type == GlugTokenType.SymbolRArrow) {
                stream.GetToken();
            }

            var block = ReadExprGreedily(stream);

            return new Function(name, plist, block);
        }

        private static List<string> ReadOptionalParamList(LookAheadTokenStream stream) {
            var rv = new List<string>();

            // Optional parameter list will never be the last part of of a legal code
            if (stream.PeekToken().Type == GlugTokenType.SymbolLBracket) {
                Consume(stream, GlugTokenType.SymbolLBracket);
                for (;;) {
                    var tok = stream.PeekToken();
                    if (tok.Type == GlugTokenType.Identifier) {
                        rv.Add(tok.ValueString!);
                    } else if (tok.Type == GlugTokenType.SymbolRBracket) {
                        break;
                    } else {
                        throw new ArgumentOutOfRangeException();
                    }
                    stream.GetToken();

                    if (stream.PeekToken().Type != GlugTokenType.SymbolComma) {
                        break;
                    }

                    Consume(stream, GlugTokenType.SymbolComma);
                }

                Consume(stream, GlugTokenType.SymbolRBracket);
            }

            return rv;
        }

        private static OnStackList ReadOnStackList(LookAheadTokenStream stream) {
            var rv = new List<Expr>();

            Consume(stream, GlugTokenType.SymbolLBracket);
            for (;;) {
                var tok = stream.PeekToken();
                if (tok.Type == GlugTokenType.SymbolRBracket) break;
                rv.Add(ReadExprGreedily(stream));

                tok = stream.PeekToken();
                if (tok.Type != GlugTokenType.SymbolComma) {
                    break;
                }

                Consume(stream, GlugTokenType.SymbolComma);
            }

            Consume(stream, GlugTokenType.SymbolRBracket);

            return new OnStackList(rv);
        }

        private static TableDef ReadTableDef(LookAheadTokenStream stream) {
            var rv = new List<TableDefPair>();

            Consume(stream, GlugTokenType.SymbolLBrace);

            while (true) {
                var tok = stream.PeekToken();
                if (tok.Type == GlugTokenType.SymbolRBrace) break;

                Expr key;
                if (tok.Type == GlugTokenType.SymbolDot) {
                    stream.GetToken();
                    key = new LiteralString(ReadIdentifier(stream));
                } else {
                    if (tok.Type == GlugTokenType.SymbolAt) stream.GetToken();
                    key = ReadExprGreedily(stream);
                }

                Consume(stream, GlugTokenType.SymbolColon);

                rv.Add(new TableDefPair(key, ReadExprGreedily(stream)));

                tok = stream.PeekToken();
                if (tok.Type == GlugTokenType.SymbolComma) Consume(stream, GlugTokenType.SymbolComma);
            }

            Consume(stream, GlugTokenType.SymbolRBrace);

            return new TableDef(rv);
        }
    }
}
