using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Schema;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.Parser {
    public static class GlugParser {
        private static bool IsLiteral(GlugTokenType type) {
            return ((uint)type >> 20) == 0x000;
        }

        private static bool LikelyExpr(GlugTokenType type) {
            return IsLiteral(type)
                   || type == GlugTokenType.Identifier
                   || type == GlugTokenType.OpSub
                   || type == GlugTokenType.OpNeg
                   || type == GlugTokenType.SymbolLParen
                   || type == GlugTokenType.SymbolLBrace
                   || type == GlugTokenType.KeywordIf
                   || type == GlugTokenType.KeywordWhile
                   || type == GlugTokenType.KeywordReturn
                   || type == GlugTokenType.KeywordFn
                   || type == GlugTokenType.SymbolLBracket
                ;
        }

        private static GlugBiOpType TokenToBiOp(GlugTokenType op) => op switch {
            GlugTokenType.SymbolAssign => GlugBiOpType.Assign,
            GlugTokenType.OpOrr => GlugBiOpType.Orr,
            GlugTokenType.OpXor => GlugBiOpType.Xor,
            GlugTokenType.OpAnd => GlugBiOpType.And,
            GlugTokenType.OpEqu => GlugBiOpType.Equ,
            GlugTokenType.OpNeq => GlugBiOpType.Neq,
            GlugTokenType.OpGtr => GlugBiOpType.Gtr,
            GlugTokenType.OpLss => GlugBiOpType.Lss,
            GlugTokenType.OpGeq => GlugBiOpType.Geq,
            GlugTokenType.OpLeq => GlugBiOpType.Leq,
            GlugTokenType.OpLsh => GlugBiOpType.Lsh,
            GlugTokenType.OpRsh => GlugBiOpType.Rsh,
            GlugTokenType.OpAdd => GlugBiOpType.Add,
            GlugTokenType.OpSub => GlugBiOpType.Sub,
            GlugTokenType.OpMul => GlugBiOpType.Mul,
            GlugTokenType.OpDiv => GlugBiOpType.Div,
            GlugTokenType.OpMod => GlugBiOpType.Mod,
            GlugTokenType.OpAt => GlugBiOpType.Call,
            GlugTokenType.OpCall => GlugBiOpType.Call,
            _ => throw new ArgumentOutOfRangeException(),
        };

        private static GlugUnOpType TokenToUnOp(GlugTokenType op) => op switch {
            GlugTokenType.OpSub => GlugUnOpType.Neg,
            GlugTokenType.OpNot => GlugUnOpType.Not,
            _ => throw new ArgumentOutOfRangeException(),
        };

        private static int Precedence(GlugTokenType op) => op switch {
            GlugTokenType.SymbolAssign => 0x05,
            GlugTokenType.OpOrr => 0x10,
            GlugTokenType.OpXor => 0x20,
            GlugTokenType.OpAnd => 0x30,
            GlugTokenType.OpEqu => 0x40,
            GlugTokenType.OpNeq => 0x40,
            GlugTokenType.OpGtr => 0x50,
            GlugTokenType.OpLss => 0x50,
            GlugTokenType.OpGeq => 0x50,
            GlugTokenType.OpLeq => 0x50,
            GlugTokenType.OpLsh => 0x60,
            GlugTokenType.OpRsh => 0x60,
            GlugTokenType.OpAdd => 0x70,
            GlugTokenType.OpSub => 0x70,
            GlugTokenType.OpMul => 0x80,
            GlugTokenType.OpDiv => 0x80,
            GlugTokenType.OpMod => 0x80,
            GlugTokenType.OpAt => 0x90,
            GlugTokenType.OpCall => 0xa0,
            _ => -1,
        };

        // contract: all operators with same precedence have same associativity
        private static bool LeftAssociate(GlugTokenType op) => op switch {
            GlugTokenType.SymbolAssign => false,
            GlugTokenType.OpOrr => true,
            GlugTokenType.OpXor => true,
            GlugTokenType.OpAnd => true,
            GlugTokenType.OpEqu => true,
            GlugTokenType.OpNeq => true,
            GlugTokenType.OpGtr => true,
            GlugTokenType.OpLss => true,
            GlugTokenType.OpGeq => true,
            GlugTokenType.OpLeq => true,
            GlugTokenType.OpLsh => true,
            GlugTokenType.OpRsh => true,
            GlugTokenType.OpAdd => true,
            GlugTokenType.OpSub => true,
            GlugTokenType.OpMul => true,
            GlugTokenType.OpDiv => true,
            GlugTokenType.OpMod => true,
            GlugTokenType.OpAt => false,
            GlugTokenType.OpCall => true,
            _ => throw new ArgumentOutOfRangeException(),
        };

        private static void Consume(LookAheadTokenStream stream, GlugTokenType expected) {
            if (expected != stream.GetToken()?.Type) throw new ArgumentOutOfRangeException();
        }



        public static Block Parse(IGlugTokenStream source) => ReadBlock(new LookAheadTokenStream(source));

        private static Block ReadBlock(LookAheadTokenStream stream) {
            var statements = new List<Expr>();

            while (true) {
                var tok = stream.PeekToken();
                if (tok == null || tok.Type == GlugTokenType.SymbolRBrace) break;

                statements.Add(ReadExprGreedily(stream));

                tok = stream.PeekToken();
                if (tok?.Type == GlugTokenType.SymbolSemicolon) Consume(stream, GlugTokenType.SymbolSemicolon);
            }

            return new Block(statements);
        }

        private static Expr ReadExprGreedily(LookAheadTokenStream stream) {
            var s = new List<(Expr expr, GlugTokenType op)>();
            Expr expr;
            int lastP = 0;

            while (true) {
                expr = ReadExprItem(stream);
                var tok = stream.PeekToken();
                var op = tok?.Type ?? GlugTokenType.NotAToken;

                if (op == GlugTokenType.SymbolRArrow) {
                    if (expr is VarRef vr) {
                        stream.GetToken();
                        expr = new Function(null, new List<string>(new[] { vr.Id }), ReadExprGreedily(stream));
                    } else if (expr is OnStackList osl && osl.IsVarRef) {
                        stream.GetToken();
                        expr = new Function(null, osl.List.Cast<VarRef>().Select(x => x.Id).ToList(), ReadExprGreedily(stream));
                    } else {
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
            var tok = stream.PeekTokenNonNull();

            if (IsLiteral(tok.Type)) {
                tok = stream.GetTokenNonNull();

                return tok.Type switch {
                    GlugTokenType.LiteralTrue => (Expr)new LiteralBool(true),
                    GlugTokenType.LiteralFalse => new LiteralBool(false),
                    GlugTokenType.LiteralInteger => new LiteralInteger(tok.ValueInt),
                    GlugTokenType.LiteralString => new LiteralString(tok.ValueString!),
                    GlugTokenType.LiteralNil => new LiteralNil(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (tok.Type == GlugTokenType.OpSub) {
                stream.GetToken();
                return new UnOp(GlugUnOpType.Neg, ReadExprItem(stream));
            }

            if (tok.Type == GlugTokenType.OpNot) {
                stream.GetToken();
                return new UnOp(GlugUnOpType.Not, ReadExprItem(stream));
            }

            if (tok.Type == GlugTokenType.OpDollar) {
                stream.GetToken();
                return new VarRef(ReadIdentifier(stream)) { IsDef = true };
            }

            if (tok.Type == GlugTokenType.Identifier) {
                return new VarRef(ReadIdentifier(stream));
            }

            return tok.Type switch {
                GlugTokenType.SymbolLParen => ReadExprInParen(stream),
                GlugTokenType.SymbolLBrace => ReadBlockInBra(stream),
                GlugTokenType.KeywordIf => ReadIf(stream),
                GlugTokenType.KeywordWhile => ReadWhile(stream),
                GlugTokenType.KeywordReturn => ReadReturn(stream),
                GlugTokenType.KeywordFn => ReadFunction(stream),
                GlugTokenType.SymbolLBracket => ReadOnStackList(stream),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private static string ReadIdentifier(LookAheadTokenStream stream) {
            var tok = stream.GetTokenNonNull();

            return tok.Type == GlugTokenType.Identifier ? tok.ValueString! : throw new ArgumentOutOfRangeException();
        }


        private static Expr ReadExprInParen(LookAheadTokenStream stream, bool skipLParen = false) {
            if (!skipLParen) Consume(stream, GlugTokenType.SymbolLParen);
            var expr = ReadExprGreedily(stream);
            Consume(stream, GlugTokenType.SymbolRParen);
            return expr;
        }

        private static Block ReadBlockInBra(LookAheadTokenStream stream) {
            Consume(stream, GlugTokenType.SymbolLBrace);
            var rv = ReadBlock(stream);
            Consume(stream, GlugTokenType.SymbolRBrace);
            return rv;
        }

        private static If ReadIf(LookAheadTokenStream stream) {
            var branches = new List<IfBranch>();

            Consume(stream, GlugTokenType.KeywordIf);
            var expr = ReadExprInParen(stream);
            Expr? block = ReadExprGreedily(stream);
            branches.Add(new IfBranch(expr, block));

            while (stream.PeekToken()?.Type == GlugTokenType.KeywordElif) {
                Consume(stream, GlugTokenType.KeywordElif);
                expr = ReadExprInParen(stream);
                block = ReadExprGreedily(stream);
                branches.Add(new IfBranch(expr, block));
            }

            if (stream.PeekToken()?.Type == GlugTokenType.KeywordElse) {
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
            expr = ReadExprInParen(stream);
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
            if (stream.PeekTokenNonNull().Type == GlugTokenType.Identifier) {
                name = stream.GetTokenNonNull().ValueString!;
            }

            var plist = ReadOptionalParamList(stream);

            if (stream.PeekTokenNonNull().Type == GlugTokenType.SymbolRArrow) {
                stream.GetToken();
            }

            var block = ReadExprGreedily(stream);

            return new Function(name, plist, block);
        }

        private static List<string> ReadOptionalParamList(LookAheadTokenStream stream) {
            var rv = new List<string>();

            if (stream.PeekTokenNonNull().Type == GlugTokenType.SymbolLBracket) {
                Consume(stream, GlugTokenType.SymbolLBracket);
                GlugToken? tok;
                while (((tok = stream.PeekToken()) != null)) {
                    if (tok.Type == GlugTokenType.Identifier) rv.Add(tok.ValueString!);
                    else throw new ArgumentOutOfRangeException();
                    stream.GetToken();
                    tok = stream.PeekToken();
                    if (tok?.Type == GlugTokenType.SymbolComma) {
                        Consume(stream, GlugTokenType.SymbolComma);
                        continue;
                    } else {
                        break;
                    }
                }

                Consume(stream, GlugTokenType.SymbolRBracket);
            }

            return rv;
        }

        private static OnStackList ReadOnStackList(LookAheadTokenStream stream, bool skipLBracket = false) {
            var rv = new List<Expr>();

            if (!skipLBracket) Consume(stream, GlugTokenType.SymbolLBracket);
            GlugToken tok;
            while (((tok = stream.PeekToken()) != null)) {
                if (tok.Type == GlugTokenType.SymbolRBracket) break;
                rv.Add(ReadExprGreedily(stream));

                tok = stream.PeekTokenNonNull();
                if (tok.Type == GlugTokenType.SymbolComma) {
                    Consume(stream, GlugTokenType.SymbolComma);
                    continue;
                } else {
                    break;
                }
            }

            Consume(stream, GlugTokenType.SymbolRBracket);

            return new OnStackList(rv);
        }
    }
}
