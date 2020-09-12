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

        public enum PositionDisplayPolicy {
            None,
            Default,
            Full,
        }

        public PositionDisplayPolicy PositionDisplay { get; set; } = PositionDisplayPolicy.None;
        
        PositionInSource _last = PositionInSource.NotAPosition();

        private string GetPositionPrefix(PositionInSource position) {
            if (PositionDisplay == PositionDisplayPolicy.None) return "";
            
            if (position.IsNotAPosition()) return " @ <unknown-position>";

            var rv = " @ " + (position.Source != _last.Source || PositionDisplay == PositionDisplayPolicy.Full ? position.Source : "..") + ":" + position.Row + ":" + position.Column;
            _last = position;
            return rv;
        }
        
        public override void VisitLiteralInteger(LiteralInteger val) {
            Writer.Write(val.Value);
            Writer.WriteLine(GetPositionPrefix(val.Position));
        }

        public override void VisitLiteralFloat(LiteralFloat val) {
            Writer.Write(val.Value);
            Writer.WriteLine(GetPositionPrefix(val.Position));
        }

        public override void VisitLiteralString(LiteralString val) {
            Writer.Write($"\"{EscapeSequenceConverter.Encode(val.Value)}\"");
            Writer.WriteLine(GetPositionPrefix(val.Position));
        }

        public override void VisitLiteralBool(LiteralBool val) {
            Writer.Write(val.Value ? "true" : "false");
            Writer.WriteLine(GetPositionPrefix(val.Position));
        }

        public override void VisitLiteralNil(LiteralNil val) {
            Writer.Write("nil");
            Writer.WriteLine(GetPositionPrefix(val.Position));
        }

        public override void VisitVarRef(VarRef val) {
            Writer.Write($"<var-{(val.IsDef ? "def" : "ref")}> {val.Id}");
            Writer.WriteLine(GetPositionPrefix(val.Position));
        }

        public override void VisitIf(If val) {
            Writer.Write("if");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();

            foreach (var (cond, expr) in val.Branches) {
                Writer.WriteLine("<branch>");
                Writer.IncreaseIndent();
                Writer.WriteLine("<cond>");
                Writer.IncreaseIndent();
                VisitNode(cond);
                Writer.DecreaseIndent();
                Writer.WriteLine("<expr>");
                Writer.IncreaseIndent();
                VisitNode(expr);
                Writer.DecreaseIndent();
                Writer.DecreaseIndent();
            }

            if (val.ElseBranch != null) {
                Writer.WriteLine("<else>");
                Writer.IncreaseIndent();
                VisitNode(val.ElseBranch);
                Writer.DecreaseIndent();
            }

            Writer.DecreaseIndent();
        }

        public override void VisitWhile(While val) {
            Writer.Write("while");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            Writer.WriteLine("<cond>");
            Writer.IncreaseIndent();
            VisitNode(val.Condition);
            Writer.DecreaseIndent();
            Writer.WriteLine("<expr>");
            Writer.IncreaseIndent();
            VisitNode(val.Body);
            Writer.DecreaseIndent();
            Writer.DecreaseIndent();
        }

        public override void VisitFor(For val) {
            Writer.Write("for");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            Writer.WriteLine("<iter-vals>");
            Writer.IncreaseIndent();
            foreach (var varRef in val.IteratorVariables) {
                VisitNode(varRef);
            }
            Writer.DecreaseIndent();
            Writer.WriteLine("<expr>");
            Writer.IncreaseIndent();
            VisitNode(val.Expression);
            Writer.DecreaseIndent();
            Writer.WriteLine("<body>");
            Writer.IncreaseIndent();
            VisitNode(val.Body);
            Writer.DecreaseIndent();
            Writer.DecreaseIndent();
        }

        public override void VisitReturn(Return val) {
            Writer.Write("<return>");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            VisitNode(val.Expr);
            Writer.DecreaseIndent();
        }

        public override void VisitBreak(Break val) {
            Writer.Write("<break>");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            VisitNode(val.Expr);
            Writer.DecreaseIndent();
        }

        public override void VisitFunction(Function val) {
            Writer.Write($"function \"{val.Name}\" {(val.Parameters.Count > 0 ? $"[{val.Parameters.Select(x => $"\"{x}\"").JoinBy(", ")}]" : "[]")}");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            VisitNode(val.Body);
            Writer.DecreaseIndent();
        }

        public override void VisitOnStackList(OnStackList val) {
            Writer.Write("<on-stack-list>");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            val.List.ForEach(VisitNode);
            Writer.DecreaseIndent();
        }

        public override void VisitBlock(Block val) {
            Writer.Write("<block>");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            val.List.ForEach(VisitNode);
            Writer.DecreaseIndent();
        }

        public override void VisitUnOp(UnOp val) {
            Writer.Write(val.Op switch {
                GlugUnOpType.Not    => "not",
                GlugUnOpType.Neg    => "neg",
                GlugUnOpType.Typeof => "typeof",
                GlugUnOpType.IsNil  => "isnil",
                _                   => throw new ArgumentOutOfRangeException(),
            });
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            VisitNode(val.Expr);
            Writer.DecreaseIndent();
        }

        public override void VisitBiOp(BiOp val) {
            Writer.Write(val.Op switch {
                GlugBiOpType.Add             => "add",
                GlugBiOpType.Sub             => "sub",
                GlugBiOpType.Mul             => "mul",
                GlugBiOpType.Div             => "div",
                GlugBiOpType.Mod             => "mod",
                GlugBiOpType.Lsh             => "lsh",
                GlugBiOpType.Rsh             => "rsh",
                GlugBiOpType.And             => "and",
                GlugBiOpType.Orr             => "orr",
                GlugBiOpType.Xor             => "xor",
                GlugBiOpType.Gtr             => "gtr",
                GlugBiOpType.Lss             => "lss",
                GlugBiOpType.Geq             => "geq",
                GlugBiOpType.Leq             => "leq",
                GlugBiOpType.Equ             => "equ",
                GlugBiOpType.Neq             => "neq",
                GlugBiOpType.Call            => "call",
                GlugBiOpType.Assign          => "assign",
                GlugBiOpType.Index           => "index",
                GlugBiOpType.IndexLocal      => "index-local",
                GlugBiOpType.Concat          => "concat",
                GlugBiOpType.ShortCircuitAnd => "short-circuit and",
                GlugBiOpType.ShortCircuitOrr => "short-circuit orr",
                GlugBiOpType.NullCoalescing  => "coalescing",
                _                            => throw new ArgumentOutOfRangeException(),
            });
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            VisitNode(val.ExprL);
            VisitNode(val.ExprR);
            Writer.DecreaseIndent();
        }

        public override void VisitTableDef(TableDef val) {
            Writer.Write("<table-def>");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            foreach (var (key, value) in val.Pairs) {
                Writer.WriteLine("<pair>");
                Writer.IncreaseIndent();
                VisitNode(key);
                VisitNode(value);
                Writer.DecreaseIndent();
            }

            Writer.DecreaseIndent();
        }

        public override void VisitVectorDef(VectorDef val) {
            Writer.Write("<vector-def>");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            foreach (var item in val.Items) {
                VisitNode(item);
            }
            Writer.DecreaseIndent();
        }

        public override void VisitPseudoIndex(PseudoIndex val) {
            Writer.Write($"<pseudo-index-{(val.IsTail ? "tail" : "head")}>");
            Writer.WriteLine(GetPositionPrefix(val.Position));
        }

        public override void VisitMetatable(Metatable val) {
            Writer.Write("metatable");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            VisitNode(val.Table);
            Writer.DecreaseIndent();
        }

        public override void VisitDiscard(Discard val) {
            Writer.Write("<discard>");
            Writer.WriteLine(GetPositionPrefix(val.Position));
        }

        public override void VisitSysCall(SysCall val) {
            Writer.Write($"<syscall 0x{val.Id:x} result: {val.Result}>");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            val.Inputs.ForEach(VisitNode);
            Writer.DecreaseIndent();
        }

        public override void VisitToValue(ToValue val) {
            Writer.Write("to-value");
            Writer.WriteLine(GetPositionPrefix(val.Position));
            Writer.IncreaseIndent();
            VisitNode(val.Child);
            Writer.DecreaseIndent();
        }
    }
}
