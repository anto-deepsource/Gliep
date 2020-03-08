using System;
using GeminiLab.Core2.Collections;

namespace GeminiLab.Glug.AST {
    public abstract class Visitor {
        public abstract void VisitLiteralInteger(LiteralInteger val);
        public abstract void VisitLiteralBool(LiteralBool val);
        public abstract void VisitLiteralString(LiteralString val);
        public abstract void VisitLiteralNil(LiteralNil val);

        public abstract void VisitVarRef(VarRef val);

        public abstract void VisitIf(If val);
        public abstract void VisitWhile(While val);
        public abstract void VisitReturn(Return val);
        public abstract void VisitFunction(Function val);

        public abstract void VisitOnStackList(OnStackList val);

        public abstract void VisitBlock(Block val);

        public abstract void VisitUnOp(UnOp val);
        public abstract void VisitBiOp(BiOp val);

        public abstract void VisitTableDef(TableDef val);

        public abstract void VisitMetatable(Metatable val);

        public void Visit(Node node) {
            switch (node) {
                case LiteralInteger li:
                    VisitLiteralInteger(li);
                    break;
                case LiteralBool lb:
                    VisitLiteralBool(lb);
                    break;
                case LiteralString ls:
                    VisitLiteralString(ls);
                    break;
                case LiteralNil ln:
                    VisitLiteralNil(ln);
                    break;
                case VarRef vr:
                    VisitVarRef(vr);
                    break;
                case If i:
                    VisitIf(i);
                    break;
                case While w:
                    VisitWhile(w);
                    break;
                case Return ret:
                    VisitReturn(ret);
                    break;
                case Function fun:
                    VisitFunction(fun);
                    break;
                case OnStackList osl:
                    VisitOnStackList(osl);
                    break;
                case Block blk:
                    VisitBlock(blk);
                    break;
                case UnOp uop:
                    VisitUnOp(uop);
                    break;
                case BiOp bop:
                    VisitBiOp(bop);
                    break;
                case TableDef tdef:
                    VisitTableDef(tdef);
                    break;
                case Metatable mt:
                    VisitMetatable(mt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class RecursiveVisitor : Visitor {
        public override void VisitLiteralInteger(LiteralInteger val) { }

        public override void VisitLiteralBool(LiteralBool val) { }

        public override void VisitLiteralString(LiteralString val) { }

        public override void VisitLiteralNil(LiteralNil val) { }

        public override void VisitVarRef(VarRef val) { }

        public override void VisitIf(If val) {
            foreach (var (cond, expr) in val.Branches) {
                Visit(cond);
                Visit(expr);
            }

            if (val.ElseBranch != null) Visit(val.ElseBranch);
        }

        public override void VisitWhile(While val) {
            Visit(val.Condition);
            Visit(val.Body);
        }

        public override void VisitReturn(Return val) {
            Visit(val.Expr);
        }

        public override void VisitFunction(Function val) {
            Visit(val.Body);
        }

        public override void VisitOnStackList(OnStackList val) {
            val.List.ForEach(Visit);
        }

        public override void VisitBlock(Block val) {
            val.List.ForEach(Visit);
        }

        public override void VisitUnOp(UnOp val) {
            Visit(val.Expr);
        }

        public override void VisitBiOp(BiOp val) {
            Visit(val.ExprL);
            Visit(val.ExprR);
        }

        public override void VisitTableDef(TableDef val) {
            foreach (var (key, value) in val.Pairs) {
                Visit(key);
                Visit(value);
            }
        }

        public override void VisitMetatable(Metatable val) {
            Visit(val.Table);
        }
    }
}
