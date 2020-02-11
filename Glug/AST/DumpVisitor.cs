using System;
using System.Collections.Generic;
using System.Linq;
using GeminiLab.Core2;
using GeminiLab.Core2.IO;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.AST {
    public class DumpVisitor : VisitorBase {
        public DumpVisitor(IndentedWriter writer) {
            Writer = writer;
        }

        public IndentedWriter Writer { get; }

        public override void VisitLiteralInteger(LiteralInteger val) {
            Writer.WriteLine(val.Value);
        }

        public override void VisitLiteralBool(LiteralBool val) {
            Writer.WriteLine(val.Value ? "true" : "false");
        }

        public override void VisitLiteralNil(LiteralNil val) {
            Writer.WriteLine("nil");
        }

        public override void VisitVarRef(VarRef val) {
            Writer.WriteLine($"<val-{(val.IsDef ? "def" : "ref")}> {val.Id}");
        }

        public override void VisitIf(If val) {
            Writer.WriteLine("if");
            Writer.IncreaseIndent();
            
            foreach (var (cond, expr) in val.Branches) {
                Writer.WriteLine("<branch>");
                Writer.IncreaseIndent();
                Writer.WriteLine("<cond>");
                Writer.IncreaseIndent();
                Visit(cond);
                Writer.DecreaseIndent();
                Writer.WriteLine("<expr>");
                Writer.IncreaseIndent();
                Visit(expr);
                Writer.DecreaseIndent();
                Writer.DecreaseIndent();
            }

            if (val.ElseBranch != null) {
                Writer.WriteLine("<else>");
                Writer.IncreaseIndent();
                Visit(val.ElseBranch);
                Writer.DecreaseIndent();
            }

            Writer.DecreaseIndent();
        }

        public override void VisitWhile(While val) {
            Writer.WriteLine("while");
            Writer.IncreaseIndent();
            Writer.WriteLine("<cond>");
            Writer.IncreaseIndent();
            Visit(val.Condition);
            Writer.DecreaseIndent();
            Writer.WriteLine("<expr>");
            Writer.IncreaseIndent();
            Visit(val.Expression);
            Writer.DecreaseIndent();
            Writer.DecreaseIndent();
        }

        public override void VisitReturn(Return val) {
            Writer.WriteLine("<return>");
            Writer.IncreaseIndent();
            Visit(val.Expression);
            Writer.DecreaseIndent();
        }

        public override void VisitFunction(Function val) {
            Writer.WriteLine($"function \"{val.Name}\" {(val.Params.Count > 0 ? $"[{val.Params.Select(x => $"\"{x}\"").JoinBy(", ")}]" : "[]")}");
            Writer.IncreaseIndent();
            Visit(val.Expression);
            Writer.DecreaseIndent();
        }

        public override void VisitOnStackList(OnStackList val) {
            Writer.WriteLine("<on-stack-list>");
            Writer.IncreaseIndent();
            val.List.ForEach(Visit);
            Writer.DecreaseIndent();
        }

        public override void VisitBlock(Block val) {
            Writer.WriteLine("<block>");
            Writer.IncreaseIndent();
            val.Statements.ForEach(Visit);
            Writer.DecreaseIndent();
        }

        public override void VisitUnOp(UnOp val) {
            Writer.WriteLine(val.Op switch {
                GlugTokenType.OpNot => "not",
                GlugTokenType.OpNeg => "neg",
                _ => throw new ArgumentOutOfRangeException(),
            });
            Writer.IncreaseIndent();
            Visit(val.Expr);
            Writer.DecreaseIndent();
        }

        public override void VisitBiOp(BiOp val) {
            Writer.WriteLine(val.Op switch {
                GlugBiOpType.Add => "add",
                GlugBiOpType.Sub => "sub",
                GlugBiOpType.Mul => "mul",
                GlugBiOpType.Div => "div",
                GlugBiOpType.Mod => "mod",
                GlugBiOpType.Lsh => "lsh",
                GlugBiOpType.Rsh => "rsh",
                GlugBiOpType.And => "and",
                GlugBiOpType.Orr => "orr",
                GlugBiOpType.Xor => "xor",
                GlugBiOpType.Gtr => "gtr",
                GlugBiOpType.Lss => "lss",
                GlugBiOpType.Geq => "geq",
                GlugBiOpType.Leq => "leq",
                GlugBiOpType.Equ => "equ",
                GlugBiOpType.Neq => "neq",
                GlugBiOpType.Call => "call",
                GlugBiOpType.Assign => "assign",
                _ => throw new ArgumentOutOfRangeException(),
            });
            Writer.IncreaseIndent();
            Visit(val.ExprL);
            Visit(val.ExprR);
            Writer.DecreaseIndent();
        }
    }
}
