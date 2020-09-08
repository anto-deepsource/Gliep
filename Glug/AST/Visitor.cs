using System;
using GeminiLab.Core2.Collections;
using GeminiLab.Glug.PostProcess;

namespace GeminiLab.Glug.AST {
    public abstract class VisitorBase {
        protected Pass Pass { get; set; } = null!;

        protected abstract void VisitRoot(Node root);

        public virtual void Visit(Node root, Pass parentPass) {
            Pass = parentPass;
            VisitRoot(root);
            // keep it or remove it?
            // Pass = null!;
        }
    }

    public abstract class Visitor : VisitorBase {
        public abstract void VisitLiteralInteger(LiteralInteger val);
        public abstract void VisitLiteralFloat(LiteralFloat val);
        public abstract void VisitLiteralBool(LiteralBool val);
        public abstract void VisitLiteralString(LiteralString val);
        public abstract void VisitLiteralNil(LiteralNil val);

        public abstract void VisitVarRef(VarRef val);

        public abstract void VisitIf(If val);
        public abstract void VisitWhile(While val);
        public abstract void VisitFor(For val);
        public abstract void VisitReturn(Return val);
        public abstract void VisitBreak(Break val);
        public abstract void VisitFunction(Function val);

        public abstract void VisitOnStackList(OnStackList val);

        public abstract void VisitBlock(Block val);

        public abstract void VisitUnOp(UnOp val);
        public abstract void VisitBiOp(BiOp val);

        public abstract void VisitTableDef(TableDef val);
        public abstract void VisitVectorDef(VectorDef val);

        public abstract void VisitMetatable(Metatable val);

        public abstract void VisitPseudoIndex(PseudoIndex val);

        public abstract void VisitSysCall(SysCall val);
        public abstract void VisitToValue(ToValue val);


        protected virtual void PreVisit(Node node) { }
        protected virtual void PostVisit(Node node) { }

        protected virtual void OnUnknownNode(Node node) {
            throw new ArgumentOutOfRangeException();
        }

        public void VisitNode(Node node) {
            PreVisit(node);

            switch (node) {
            case LiteralInteger li:
                VisitLiteralInteger(li);
                break;
            case LiteralFloat lf:
                VisitLiteralFloat(lf);
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
            case For f:
                VisitFor(f);
                break;
            case Return ret:
                VisitReturn(ret);
                break;
            case Break b:
                VisitBreak(b);
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
            case VectorDef vdef:
                VisitVectorDef(vdef);
                break;
            case Metatable mt:
                VisitMetatable(mt);
                break;
            case PseudoIndex pi:
                VisitPseudoIndex(pi);
                break;
            case SysCall sc:
                VisitSysCall(sc);
                break;
            case ToValue tv:
                VisitToValue(tv);
                break;
            default:
                OnUnknownNode(node);
                break;
            }

            PostVisit(node);
        }

        protected override void VisitRoot(Node root) {
            VisitNode(root);
        }
    }

    public class RecursiveVisitor : Visitor {
        public override void VisitLiteralInteger(LiteralInteger val) { }
        public override void VisitLiteralFloat(LiteralFloat val) { }
        public override void VisitLiteralBool(LiteralBool val) { }
        public override void VisitLiteralString(LiteralString val) { }
        public override void VisitLiteralNil(LiteralNil val) { }

        public override void VisitVarRef(VarRef val) { }

        public override void VisitPseudoIndex(PseudoIndex val) { }

        public override void VisitIf(If val) {
            foreach (var (cond, expr) in val.Branches) {
                VisitNode(cond);
                VisitNode(expr);
            }

            if (val.ElseBranch != null) VisitNode(val.ElseBranch);
        }

        public override void VisitWhile(While val) {
            VisitNode(val.Condition);
            VisitNode(val.Body);
        }

        public override void VisitFor(For val) {
            val.IteratorVariables.ForEach(VisitNode);
            VisitNode(val.Expression);
            VisitNode(val.Body);
        }

        public override void VisitReturn(Return val) {
            VisitNode(val.Expr);
        }

        public override void VisitBreak(Break val) {
            VisitNode(val.Expr);
        }

        public override void VisitFunction(Function val) {
            VisitNode(val.Body);
        }

        public override void VisitOnStackList(OnStackList val) {
            val.List.ForEach(VisitNode);
        }

        public override void VisitBlock(Block val) {
            val.List.ForEach(VisitNode);
        }

        public override void VisitUnOp(UnOp val) {
            VisitNode(val.Expr);
        }

        public override void VisitBiOp(BiOp val) {
            VisitNode(val.ExprL);
            VisitNode(val.ExprR);
        }

        public override void VisitTableDef(TableDef val) {
            foreach (var (key, value) in val.Pairs) {
                VisitNode(key);
                VisitNode(value);
            }
        }

        public override void VisitVectorDef(VectorDef val) {
            foreach (var item in val.Items) {
                VisitNode(item);
            }
        }

        public override void VisitMetatable(Metatable val) {
            VisitNode(val.Table);
        }

        public override void VisitSysCall(SysCall val) {
            val.Inputs.ForEach(VisitNode);
        }

        public override void VisitToValue(ToValue val) {
            VisitNode(val.Child);
        }
    }

    public abstract class InVisitor<T0> : VisitorBase {
        public abstract void VisitLiteralInteger(LiteralInteger val, T0 arg);
        public abstract void VisitLiteralFloat(LiteralFloat val, T0 arg);
        public abstract void VisitLiteralBool(LiteralBool val, T0 arg);
        public abstract void VisitLiteralString(LiteralString val, T0 arg);
        public abstract void VisitLiteralNil(LiteralNil val, T0 arg);

        public abstract void VisitVarRef(VarRef val, T0 arg);

        public abstract void VisitIf(If val, T0 arg);
        public abstract void VisitWhile(While val, T0 arg);
        public abstract void VisitFor(For val, T0 arg);
        public abstract void VisitReturn(Return val, T0 arg);
        public abstract void VisitBreak(Break val, T0 arg);
        public abstract void VisitFunction(Function val, T0 arg);

        public abstract void VisitOnStackList(OnStackList val, T0 arg);

        public abstract void VisitBlock(Block val, T0 arg);

        public abstract void VisitUnOp(UnOp val, T0 arg);
        public abstract void VisitBiOp(BiOp val, T0 arg);

        public abstract void VisitTableDef(TableDef val, T0 arg);
        public abstract void VisitVectorDef(VectorDef val, T0 arg);

        public abstract void VisitMetatable(Metatable val, T0 arg);

        public abstract void VisitPseudoIndex(PseudoIndex val, T0 arg);

        public abstract void VisitSysCall(SysCall val, T0 arg);
        public abstract void VisitToValue(ToValue val, T0 arg);


        protected virtual void PreVisit(Node node, T0 arg) { }
        protected virtual void PostVisit(Node node, T0 arg) { }

        protected virtual void OnUnknownNode(Node node, T0 arg) {
            throw new ArgumentOutOfRangeException();
        }

        public void VisitNode(Node node, T0 arg) {
            PreVisit(node, arg);

            switch (node) {
            case LiteralInteger li:
                VisitLiteralInteger(li, arg);
                break;
            case LiteralFloat lf:
                VisitLiteralFloat(lf, arg);
                break;
            case LiteralBool lb:
                VisitLiteralBool(lb, arg);
                break;
            case LiteralString ls:
                VisitLiteralString(ls, arg);
                break;
            case LiteralNil ln:
                VisitLiteralNil(ln, arg);
                break;
            case VarRef vr:
                VisitVarRef(vr, arg);
                break;
            case If i:
                VisitIf(i, arg);
                break;
            case While w:
                VisitWhile(w, arg);
                break;
            case For f:
                VisitFor(f, arg);
                break;
            case Return ret:
                VisitReturn(ret, arg);
                break;
            case Break b:
                VisitBreak(b, arg);
                break;
            case Function fun:
                VisitFunction(fun, arg);
                break;
            case OnStackList osl:
                VisitOnStackList(osl, arg);
                break;
            case Block blk:
                VisitBlock(blk, arg);
                break;
            case UnOp uop:
                VisitUnOp(uop, arg);
                break;
            case BiOp bop:
                VisitBiOp(bop, arg);
                break;
            case TableDef tdef:
                VisitTableDef(tdef, arg);
                break;
            case VectorDef vdef:
                VisitVectorDef(vdef, arg);
                break;
            case Metatable mt:
                VisitMetatable(mt, arg);
                break;
            case PseudoIndex pi:
                VisitPseudoIndex(pi, arg);
                break;
            case SysCall sc:
                VisitSysCall(sc, arg);
                break;
            case ToValue tv:
                VisitToValue(tv, arg);
                break;
            default:
                OnUnknownNode(node, arg);
                break;
            }

            PostVisit(node, arg);
        }

        protected override void VisitRoot(Node root) {
            VisitNode(root, default!);
        }
    }

    public class RecursiveInVisitor<T0> : InVisitor<T0> {
        public override void VisitLiteralInteger(LiteralInteger val, T0 arg) { }

        public override void VisitLiteralFloat(LiteralFloat val, T0 arg) { }

        public override void VisitLiteralBool(LiteralBool val, T0 arg) { }

        public override void VisitLiteralString(LiteralString val, T0 arg) { }

        public override void VisitLiteralNil(LiteralNil val, T0 arg) { }

        public override void VisitVarRef(VarRef val, T0 arg) { }

        public override void VisitPseudoIndex(PseudoIndex val, T0 arg) { }

        public override void VisitIf(If val, T0 arg) {
            foreach (var (cond, expr) in val.Branches) {
                VisitNode(cond, arg);
                VisitNode(expr, arg);
            }

            if (val.ElseBranch != null) VisitNode(val.ElseBranch, arg);
        }

        public override void VisitWhile(While val, T0 arg) {
            VisitNode(val.Condition, arg);
            VisitNode(val.Body, arg);
        }

        public override void VisitFor(For val, T0 arg) {
            foreach (var varRef in val.IteratorVariables) {
                VisitNode(varRef, arg);
            }
            VisitNode(val.Expression, arg);
            VisitNode(val.Body, arg);
        }

        public override void VisitReturn(Return val, T0 arg) {
            VisitNode(val.Expr, arg);
        }

        public override void VisitBreak(Break val, T0 arg) {
            VisitNode(val.Expr, arg);
        }

        public override void VisitFunction(Function val, T0 arg) {
            VisitNode(val.Body, arg);
        }

        public override void VisitOnStackList(OnStackList val, T0 arg) {
            foreach (var expr in val.List) {
                VisitNode(expr, arg);
            }
        }

        public override void VisitBlock(Block val, T0 arg) {
            foreach (var expr in val.List) {
                VisitNode(expr, arg);
            }
        }

        public override void VisitUnOp(UnOp val, T0 arg) {
            VisitNode(val.Expr, arg);
        }

        public override void VisitBiOp(BiOp val, T0 arg) {
            VisitNode(val.ExprL, arg);
            VisitNode(val.ExprR, arg);
        }

        public override void VisitTableDef(TableDef val, T0 arg) {
            foreach (var (key, value) in val.Pairs) {
                VisitNode(key, arg);
                VisitNode(value, arg);
            }
        }

        public override void VisitVectorDef(VectorDef val, T0 arg) {
            foreach (var item in val.Items) {
                VisitNode(item, arg);
            }
        }

        public override void VisitMetatable(Metatable val, T0 arg) {
            VisitNode(val.Table, arg);
        }

        public override void VisitSysCall(SysCall val, T0 arg) {
            foreach (var input in val.Inputs) {
                VisitNode(input, arg);
            }
        }

        public override void VisitToValue(ToValue val, T0 arg) {
            VisitNode(val.Child, arg);
        }
    }

    public abstract class InVisitor<T0, T1> : VisitorBase {
        public abstract void VisitLiteralInteger(LiteralInteger val, T0 arg0, T1 arg1);
        public abstract void VisitLiteralFloat(LiteralFloat val, T0 arg0, T1 arg1);
        public abstract void VisitLiteralBool(LiteralBool val, T0 arg0, T1 arg1);
        public abstract void VisitLiteralString(LiteralString val, T0 arg0, T1 arg1);
        public abstract void VisitLiteralNil(LiteralNil val, T0 arg0, T1 arg1);

        public abstract void VisitVarRef(VarRef val, T0 arg0, T1 arg1);

        public abstract void VisitIf(If val, T0 arg0, T1 arg1);
        public abstract void VisitWhile(While val, T0 arg0, T1 arg1);
        public abstract void VisitFor(For val, T0 arg0, T1 arg1);
        public abstract void VisitReturn(Return val, T0 arg0, T1 arg1);
        public abstract void VisitBreak(Break val, T0 arg0, T1 arg1);
        public abstract void VisitFunction(Function val, T0 arg0, T1 arg1);

        public abstract void VisitOnStackList(OnStackList val, T0 arg0, T1 arg1);

        public abstract void VisitBlock(Block val, T0 arg0, T1 arg1);

        public abstract void VisitUnOp(UnOp val, T0 arg0, T1 arg1);
        public abstract void VisitBiOp(BiOp val, T0 arg0, T1 arg1);

        public abstract void VisitTableDef(TableDef val, T0 arg0, T1 arg1);
        public abstract void VisitVectorDef(VectorDef val, T0 arg0, T1 arg1);

        public abstract void VisitMetatable(Metatable val, T0 arg0, T1 arg1);

        public abstract void VisitPseudoIndex(PseudoIndex val, T0 arg0, T1 arg1);

        public abstract void VisitSysCall(SysCall val, T0 arg0, T1 arg1);
        public abstract void VisitToValue(ToValue val, T0 arg0, T1 arg1);


        protected virtual void PreVisit(Node node, T0 arg0, T1 arg1) { }
        protected virtual void PostVisit(Node node, T0 arg0, T1 arg1) { }

        protected virtual void OnUnknownNode(Node node, T0 arg0, T1 arg1) {
            throw new ArgumentOutOfRangeException();
        }

        public void VisitNode(Node node, T0 arg0, T1 arg1) {
            PreVisit(node, arg0, arg1);

            switch (node) {
            case LiteralInteger li:
                VisitLiteralInteger(li, arg0, arg1);
                break;
            case LiteralFloat lf:
                VisitLiteralFloat(lf, arg0, arg1);
                break;
            case LiteralBool lb:
                VisitLiteralBool(lb, arg0, arg1);
                break;
            case LiteralString ls:
                VisitLiteralString(ls, arg0, arg1);
                break;
            case LiteralNil ln:
                VisitLiteralNil(ln, arg0, arg1);
                break;
            case VarRef vr:
                VisitVarRef(vr, arg0, arg1);
                break;
            case If i:
                VisitIf(i, arg0, arg1);
                break;
            case While w:
                VisitWhile(w, arg0, arg1);
                break;
            case For f:
                VisitFor(f, arg0, arg1);
                break;
            case Return ret:
                VisitReturn(ret, arg0, arg1);
                break;
            case Break b:
                VisitBreak(b, arg0, arg1);
                break;
            case Function fun:
                VisitFunction(fun, arg0, arg1);
                break;
            case OnStackList osl:
                VisitOnStackList(osl, arg0, arg1);
                break;
            case Block blk:
                VisitBlock(blk, arg0, arg1);
                break;
            case UnOp uop:
                VisitUnOp(uop, arg0, arg1);
                break;
            case BiOp bop:
                VisitBiOp(bop, arg0, arg1);
                break;
            case TableDef tdef:
                VisitTableDef(tdef, arg0, arg1);
                break;
            case VectorDef vdef:
                VisitVectorDef(vdef, arg0, arg1);
                break;
            case Metatable mt:
                VisitMetatable(mt, arg0, arg1);
                break;
            case PseudoIndex pi:
                VisitPseudoIndex(pi, arg0, arg1);
                break;
            case SysCall sc:
                VisitSysCall(sc, arg0, arg1);
                break;
            case ToValue tv:
                VisitToValue(tv, arg0, arg1);
                break;
            default:
                OnUnknownNode(node, arg0, arg1);
                break;
            }

            PostVisit(node, arg0, arg1);
        }

        protected override void VisitRoot(Node root) {
            VisitNode(root, default!, default!);
        }
    }

    public class RecursiveInVisitor<T0, T1> : InVisitor<T0, T1> {
        public override void VisitLiteralInteger(LiteralInteger val, T0 arg0, T1 arg1) { }

        public override void VisitLiteralFloat(LiteralFloat val, T0 arg0, T1 arg1) { }

        public override void VisitLiteralBool(LiteralBool val, T0 arg0, T1 arg1) { }

        public override void VisitLiteralString(LiteralString val, T0 arg0, T1 arg1) { }

        public override void VisitLiteralNil(LiteralNil val, T0 arg0, T1 arg1) { }

        public override void VisitVarRef(VarRef val, T0 arg0, T1 arg1) { }

        public override void VisitPseudoIndex(PseudoIndex val, T0 arg0, T1 arg1) { }

        public override void VisitIf(If val, T0 arg0, T1 arg1) {
            foreach (var (cond, expr) in val.Branches) {
                VisitNode(cond, arg0, arg1);
                VisitNode(expr, arg0, arg1);
            }

            if (val.ElseBranch != null) VisitNode(val.ElseBranch, arg0, arg1);
        }

        public override void VisitWhile(While val, T0 arg0, T1 arg1) {
            VisitNode(val.Condition, arg0, arg1);
            VisitNode(val.Body, arg0, arg1);
        }

        public override void VisitFor(For val, T0 arg0, T1 arg1) {
            foreach (var varRef in val.IteratorVariables) {
                VisitNode(varRef, arg0, arg1);
            }
            VisitNode(val.Expression, arg0, arg1);
            VisitNode(val.Body, arg0, arg1);
        }

        public override void VisitReturn(Return val, T0 arg0, T1 arg1) {
            VisitNode(val.Expr, arg0, arg1);
        }

        public override void VisitBreak(Break val, T0 arg0, T1 arg1) {
            VisitNode(val.Expr, arg0, arg1);
        }

        public override void VisitFunction(Function val, T0 arg0, T1 arg1) {
            VisitNode(val.Body, arg0, arg1);
        }

        public override void VisitOnStackList(OnStackList val, T0 arg0, T1 arg1) {
            foreach (var expr in val.List) {
                VisitNode(expr, arg0, arg1);
            }
        }

        public override void VisitBlock(Block val, T0 arg0, T1 arg1) {
            foreach (var expr in val.List) {
                VisitNode(expr, arg0, arg1);
            }
        }

        public override void VisitUnOp(UnOp val, T0 arg0, T1 arg1) {
            VisitNode(val.Expr, arg0, arg1);
        }

        public override void VisitBiOp(BiOp val, T0 arg0, T1 arg1) {
            VisitNode(val.ExprL, arg0, arg1);
            VisitNode(val.ExprR, arg0, arg1);
        }

        public override void VisitTableDef(TableDef val, T0 arg0, T1 arg1) {
            foreach (var (key, value) in val.Pairs) {
                VisitNode(key, arg0, arg1);
                VisitNode(value, arg0, arg1);
            }
        }

        public override void VisitVectorDef(VectorDef val, T0 arg0, T1 arg1) {
            foreach (var item in val.Items) {
                VisitNode(item, arg0, arg1);
            }
        }

        public override void VisitMetatable(Metatable val, T0 arg0, T1 arg1) {
            VisitNode(val.Table, arg0, arg1);
        }

        public override void VisitSysCall(SysCall val, T0 arg0, T1 arg1) {
            foreach (var input in val.Inputs) {
                VisitNode(input, arg0, arg1);
            }
        }

        public override void VisitToValue(ToValue val, T0 arg0, T1 arg1) {
            VisitNode(val.Child, arg0, arg1);
        }
    }
}
