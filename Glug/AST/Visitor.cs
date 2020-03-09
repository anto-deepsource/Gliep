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
        public abstract void VisitBreak(Break val);
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

        public override void VisitBreak(Break val) {
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


    public abstract class InVisitor<TParameter> {
        public abstract void VisitLiteralInteger(LiteralInteger val, TParameter arg);
        public abstract void VisitLiteralBool(LiteralBool val, TParameter arg);
        public abstract void VisitLiteralString(LiteralString val, TParameter arg);
        public abstract void VisitLiteralNil(LiteralNil val, TParameter arg);

        public abstract void VisitVarRef(VarRef val, TParameter arg);

        public abstract void VisitIf(If val, TParameter arg);
        public abstract void VisitWhile(While val, TParameter arg);
        public abstract void VisitReturn(Return val, TParameter arg);
        public abstract void VisitBreak(Break val, TParameter arg);
        public abstract void VisitFunction(Function val, TParameter arg);

        public abstract void VisitOnStackList(OnStackList val, TParameter arg);

        public abstract void VisitBlock(Block val, TParameter arg);

        public abstract void VisitUnOp(UnOp val, TParameter arg);
        public abstract void VisitBiOp(BiOp val, TParameter arg);

        public abstract void VisitTableDef(TableDef val, TParameter arg);

        public abstract void VisitMetatable(Metatable val, TParameter arg);

        public void Visit(Node node, TParameter arg) {
            switch (node) {
                case LiteralInteger li:
                    VisitLiteralInteger(li, arg);
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
                case Metatable mt:
                    VisitMetatable(mt, arg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class RecursiveInVisitor<TParameter> : InVisitor<TParameter> {
        public override void VisitLiteralInteger(LiteralInteger val, TParameter arg) { }

        public override void VisitLiteralBool(LiteralBool val, TParameter arg) { }

        public override void VisitLiteralString(LiteralString val, TParameter arg) { }

        public override void VisitLiteralNil(LiteralNil val, TParameter arg) { }

        public override void VisitVarRef(VarRef val, TParameter arg) { }

        public override void VisitIf(If val, TParameter arg) {
            foreach (var (cond, expr) in val.Branches) {
                Visit(cond, arg);
                Visit(expr, arg);
            }

            if (val.ElseBranch != null) Visit(val.ElseBranch, arg);
        }

        public override void VisitWhile(While val, TParameter arg) {
            Visit(val.Condition, arg);
            Visit(val.Body, arg);
        }

        public override void VisitReturn(Return val, TParameter arg) {
            Visit(val.Expr, arg);
        }

        public override void VisitBreak(Break val, TParameter arg) {
            Visit(val.Expr, arg);
        }

        public override void VisitFunction(Function val, TParameter arg) {
            Visit(val.Body, arg);
        }

        public override void VisitOnStackList(OnStackList val, TParameter arg) {
            foreach (var expr in val.List) {
                Visit(expr, arg);
            }
        }

        public override void VisitBlock(Block val, TParameter arg) {
            foreach (var expr in val.List) {
                Visit(expr, arg);
            }
        }

        public override void VisitUnOp(UnOp val, TParameter arg) {
            Visit(val.Expr, arg);
        }

        public override void VisitBiOp(BiOp val, TParameter arg) {
            Visit(val.ExprL, arg);
            Visit(val.ExprR, arg);
        }

        public override void VisitTableDef(TableDef val, TParameter arg) {
            foreach (var (key, value) in val.Pairs) {
                Visit(key, arg);
                Visit(value, arg);
            }
        }

        public override void VisitMetatable(Metatable val, TParameter arg) {
            Visit(val.Table, arg);
        }
    }

    /*
    public abstract class InOutVisitor<TParameter, TResult> {
        public abstract TResult VisitLiteralInteger(LiteralInteger val, TParameter arg);
        public abstract TResult VisitLiteralBool(LiteralBool val, TParameter arg);
        public abstract TResult VisitLiteralString(LiteralString val, TParameter arg);
        public abstract TResult VisitLiteralNil(LiteralNil val, TParameter arg);

        public abstract TResult VisitVarRef(VarRef val, TParameter arg);

        public abstract TResult VisitIf(If val, TParameter arg);
        public abstract TResult VisitWhile(While val, TParameter arg);
        public abstract TResult VisitReturn(Return val, TParameter arg);
        public abstract TResult VisitFunction(Function val, TParameter arg);

        public abstract TResult VisitOnStackList(OnStackList val, TParameter arg);

        public abstract TResult VisitBlock(Block val, TParameter arg);

        public abstract TResult VisitUnOp(UnOp val, TParameter arg);
        public abstract TResult VisitBiOp(BiOp val, TParameter arg);

        public abstract TResult VisitTableDef(TableDef val, TParameter arg);

        public abstract TResult VisitMetatable(Metatable val, TParameter arg);

        public TResult Visit(Node node, TParameter arg) {
            return node switch {
                LiteralInteger li => VisitLiteralInteger(li, arg),
                LiteralBool lb => VisitLiteralBool(lb, arg),
                LiteralString ls => VisitLiteralString(ls, arg),
                LiteralNil ln => VisitLiteralNil(ln, arg),
                VarRef vr => VisitVarRef(vr, arg),
                If i => VisitIf(i, arg),
                While w => VisitWhile(w, arg),
                Return ret => VisitReturn(ret, arg),
                Function fun => VisitFunction(fun, arg),
                OnStackList osl => VisitOnStackList(osl, arg),
                Block blk => VisitBlock(blk, arg),
                UnOp uop => VisitUnOp(uop, arg),
                BiOp bop => VisitBiOp(bop, arg),
                TableDef tdef => VisitTableDef(tdef, arg),
                Metatable mt => VisitMetatable(mt, arg),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public class RecursiveInOutVisitor<TParameter, TResult> : InOutVisitor<TParameter, TResult> {
        public override TResult VisitLiteralInteger(LiteralInteger val, TParameter arg) => default!;

        public override TResult VisitLiteralBool(LiteralBool val, TParameter arg) => default!;

        public override TResult VisitLiteralString(LiteralString val, TParameter arg) => default!;

        public override TResult VisitLiteralNil(LiteralNil val, TParameter arg) => default!;

        public override TResult VisitVarRef(VarRef val, TParameter arg) => default!;

        public override TResult VisitIf(If val, TParameter arg) {
            foreach (var (cond, expr) in val.Branches) {
                Visit(cond, arg);
                Visit(expr, arg);
            }

            if (val.ElseBranch != null) Visit(val.ElseBranch, arg);

            return default!;
        }

        public override TResult VisitWhile(While val, TParameter arg) {
            Visit(val.Condition, arg);
            Visit(val.Body, arg);

            return default!;
        }

        public override TResult VisitReturn(Return val, TParameter arg) {
            Visit(val.Expr, arg);

            return default!;
        }

        public override TResult VisitFunction(Function val, TParameter arg) {
            Visit(val.Body, arg);

            return default!;
        }

        public override TResult VisitOnStackList(OnStackList val, TParameter arg) {
            foreach (var expr in val.List) {
                Visit(expr, arg);
            }

            return default!;
        }

        public override TResult VisitBlock(Block val, TParameter arg) {
            foreach (var expr in val.List) {
                Visit(expr, arg);
            }

            return default!;
        }

        public override TResult VisitUnOp(UnOp val, TParameter arg) {
            Visit(val.Expr, arg);

            return default!;
        }

        public override TResult VisitBiOp(BiOp val, TParameter arg) {
            Visit(val.ExprL, arg);
            Visit(val.ExprR, arg);

            return default!;
        }

        public override TResult VisitTableDef(TableDef val, TParameter arg) {
            foreach (var (key, value) in val.Pairs) {
                Visit(key, arg);
                Visit(value, arg);
            }

            return default!;
        }

        public override TResult VisitMetatable(Metatable val, TParameter arg) {
            Visit(val.Table, arg);

            return default!;
        }
    }
    */
}
