using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using GeminiLab.Core2;
using GeminiLab.Core2.Collections;
using GeminiLab.Core2.IO;
using GeminiLab.Core2.Text;

namespace GeminiLab.Glug.AST {
    // we do not test this class
    [ExcludeFromCodeCoverage]
    public class DumpVisitor : Visitor {
        public DumpVisitor(IndentedWriter writer) {
            Writer = writer;
        }

        public IndentedWriter Writer { get; }

        public override void VisitLiteralInteger(LiteralInteger val) {
            Writer.WriteLine(val.Value);
        }

        public override void VisitLiteralFloat(LiteralFloat val) {
            Writer.WriteLine(val.Value);
        }

        public override void VisitLiteralString(LiteralString val) {
            Writer.WriteLine($"\"{EscapeSequenceConverter.Encode(val.Value)}\"");
        }

        public override void VisitLiteralBool(LiteralBool val) {
            Writer.WriteLine(val.Value ? "true" : "false");
        }

        public override void VisitLiteralNil(LiteralNil val) {
            Writer.WriteLine("nil");
        }

        public override void VisitVarRef(VarRef val) {
            Writer.WriteLine($"<var-{(val.IsDef ? "def" : "ref")}> {val.Id}");
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
            Visit(val.Body);
            Writer.DecreaseIndent();
            Writer.DecreaseIndent();
        }

        public override void VisitReturn(Return val) {
            Writer.WriteLine("<return>");
            Writer.IncreaseIndent();
            Visit(val.Expr);
            Writer.DecreaseIndent();
        }

        public override void VisitBreak(Break val) {
            Writer.WriteLine("<break>");
            Writer.IncreaseIndent();
            Visit(val.Expr);
            Writer.DecreaseIndent();
        }

        public override void VisitFunction(Function val) {
            Writer.WriteLine($"function \"{val.Name}\" {(val.Parameters.Count > 0 ? $"[{val.Parameters.Select(x => $"\"{x}\"").JoinBy(", ")}]" : "[]")}");
            Writer.IncreaseIndent();
            Visit(val.Body);
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
            val.List.ForEach(Visit);
            Writer.DecreaseIndent();
        }

        public override void VisitUnOp(UnOp val) {
            Writer.WriteLine(val.Op switch {
                GlugUnOpType.Not => "not",
                GlugUnOpType.Neg => "neg",
                GlugUnOpType.Typeof => "typeof",
                GlugUnOpType.IsNil => "isnil",
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
                GlugBiOpType.Index => "index",
                GlugBiOpType.IndexLocal => "index-local",
                GlugBiOpType.Concat => "concat",
                GlugBiOpType.ShortCircuitAnd => "short-circuit and",
                GlugBiOpType.ShortCircuitOrr => "short-circuit orr",
                GlugBiOpType.NullCoalescing => "coalescing",
                _ => throw new ArgumentOutOfRangeException(),
            });
            Writer.IncreaseIndent();
            Visit(val.ExprL);
            Visit(val.ExprR);
            Writer.DecreaseIndent();
        }

        public override void VisitTableDef(TableDef val) {
            Writer.WriteLine("<table-def>");
            Writer.IncreaseIndent();
            foreach (var (key, value) in val.Pairs) {
                Writer.WriteLine("<pair>");
                Writer.IncreaseIndent();
                Visit(key);
                Visit(value);
                Writer.DecreaseIndent();
            }
            Writer.DecreaseIndent();
        }

        public override void VisitMetatable(Metatable val) {
            Writer.WriteLine("<metatable>");
            Writer.IncreaseIndent();
            Visit(val.Table);
            Writer.DecreaseIndent();
        }

        public override void VisitSysCall(SysCall val) {
            Writer.WriteLine($"<syscall 0x{val.Id:x} result: {val.Result}>");
            Writer.IncreaseIndent();
            val.Inputs.ForEach(Visit);
            Writer.DecreaseIndent();
        }

        public override void VisitToValue(ToValue val) {
            Writer.WriteLine("to-value");
            Writer.IncreaseIndent();
            Visit(val.Child);
            Writer.DecreaseIndent();
        }

        public override void VisitPseudoIndex(PseudoIndex val) {
            Writer.WriteLine($"<pseudo-index-{(val.IsTail ? "tail" : "head")}>");
        }
    }
}
