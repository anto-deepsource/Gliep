using System;
using System.Linq;

using GeminiLab.Glos.CodeGenerator;
using GeminiLab.Glos.ViMa;

namespace GeminiLab.Glug.AST {
    public struct CodeGenContext {
        public GlosFunctionBuilder CurrentFunction;
        public bool ResultUsed;
        public CodeGenContext(GlosFunctionBuilder currentFunction, bool resultUsed) {
            CurrentFunction = currentFunction;
            ResultUsed = resultUsed;
        }


        public void Deconstruct(out GlosFunctionBuilder currentFunction, out bool resultUsed) => (currentFunction, resultUsed) = (CurrentFunction, ResultUsed);
    }

    public class CodeGenVisitor : RecursiveInVisitor<CodeGenContext> {
        public GlosUnitBuilder Builder { get; } = new GlosUnitBuilder();


        private void visitForDiscard(Expr expr, GlosFunctionBuilder parent) {
            Visit(expr, new CodeGenContext(parent, false));
        }

        private void visitForValue(Expr expr, GlosFunctionBuilder parent) {
            Visit(expr, new CodeGenContext(parent, true));

            if (expr.IsOnStackList) parent.AppendShpRv(1);
        }
        
        private void visitForOsl(Expr expr, GlosFunctionBuilder parent) {
            if (!expr.IsOnStackList) parent.AppendLdDel();

            Visit(expr, new CodeGenContext(parent, true));
        }

        private void visitForAny(Expr expr, GlosFunctionBuilder parent) {
            Visit(expr, new CodeGenContext(parent, true));
        }


        public override void VisitFunction(Function val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            var fun = Builder.AddFunction();
            
            if (parent == null) fun.SetEntry();
            fun.Name = val.Name;

            var variables = val.VariableTable.Variables.Values.ToArray();
            fun.VariableInContext = variables.Where(x => x.Place == VariablePlace.Context).Select(x => x.Name).ToArray();

            foreach (var variable in variables.Where(x => x.Place == VariablePlace.LocalVariable)) {
                variable.LocalVariable = fun.AllocateLocalVariable();
            }

            foreach (var variable in variables.Where(x => x.IsArgument && x.Place != VariablePlace.Argument)) {
                fun.AppendLdArg(variable.ArgumentId);
                if (variable.Place == VariablePlace.LocalVariable) {
                    fun.AppendStLoc(variable.LocalVariable!);
                } else if (variable.Place == VariablePlace.Context) {
                    fun.AppendLdStr(variable.Name);
                    fun.AppendUvc();
                }
            }

            visitForAny(val.Body, fun);
            fun.AppendRetIfNone();

            if (parent != null) {
                if (val.Self != null || ru) {
                    parent.AppendLdFun(fun);
                    parent.AppendBind();
                }

                if (val.Self != null) {
                    if (ru) parent.AppendDup();
                    val.Self.CreateStoreInstr(parent);
                }
            }
        }

        public override void VisitIf(If val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            Label? nextLabel = null;
            var endLabel = parent.AllocateLabel();

            foreach (var branch in val.Branches) {
                if (nextLabel != null) parent.InsertLabel(nextLabel);
                nextLabel = parent.AllocateLabel();

                visitForValue(branch.Condition, parent);
                parent.AppendBf(nextLabel);

                if (!ru) visitForDiscard(branch.Body, parent);
                else if (val.IsOnStackList) visitForOsl(branch.Body, parent);
                else visitForValue(branch.Body, parent);

                parent.AppendB(endLabel);
            }

            parent.InsertLabel(nextLabel!); // brc must be at least 1 when this ast is well-formed
            if (val.ElseBranch != null) {
                if (!ru) visitForDiscard(val.ElseBranch, parent);
                else if (val.IsOnStackList) visitForOsl(val.ElseBranch, parent);
                else visitForValue(val.ElseBranch, parent);
            } else if (ru) {
                if (val.IsOnStackList) parent.AppendLdDel();
                else parent.AppendLdNil();
            }

            parent.InsertLabel(endLabel);
        }

        public override void VisitWhile(While val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            val.EndLabel = parent.AllocateLabel();

            val.ResultUsed = ru;

            if (ru) {
                if (val.IsOnStackList) parent.AppendLdDel();
                else parent.AppendLdNil();
            }

            var beginLabel = parent.AllocateAndInsertLabel();

            visitForValue(val.Condition, parent);
            parent.AppendBf(val.EndLabel);

            if (ru) {
                if (val.IsOnStackList) parent.AppendShpRv(0);
                else parent.AppendPop();
            }

            parent.AppendLdDel();
            
            if (!ru) visitForDiscard(val.Body, parent);
            else if (val.IsOnStackList) visitForOsl(val.Body, parent);
            else visitForValue(val.Body, parent);

            parent.AppendPopDel();
            parent.AppendB(beginLabel);

            parent.InsertLabel(val.EndLabel);
        }

        public override void VisitReturn(Return val, CodeGenContext ctx) {
            var (parent, _) = ctx;

            visitForOsl(val.Expr, parent);
            parent.AppendRet();
        }

        public override void VisitBreak(Break val, CodeGenContext ctx) {
            var (parent, _) = ctx;

            parent.AppendShpRv(0);
            if (!val.Parent.ResultUsed) visitForDiscard(val.Expr, parent);
            else if (val.Parent.IsOnStackList) visitForOsl(val.Expr, parent);
            else visitForValue(val.Expr, parent);
            parent.AppendB(val.Parent.EndLabel);
        }

        public override void VisitOnStackList(OnStackList val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            
            if (ru) parent.AppendLdDel();

            foreach (var item in val.List) {
                if (ru) visitForValue(item, parent);
                else visitForDiscard(item, parent);
            }
        }

        public override void VisitBlock(Block val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            var count = val.List.Count;

            if (count == 0) {
                if (ru) parent.AppendLdNil();
            } else {
                for (int i = 0; i < count; ++i) {
                    if (i < count - 1) {
                        visitForDiscard(val.List[i], parent);
                    } else {
                        if (ru) visitForAny(val.List[i], parent);
                        else visitForDiscard(val.List[i], parent);
                    }
                }
            }
        }

        public override void VisitUnOp(UnOp val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            visitForValue(val.Expr, parent);

            switch (val.Op) {
                case GlugUnOpType.Neg:
                    parent.AppendNeg();
                    break;
                case GlugUnOpType.Not:
                    parent.AppendNot();
                    break;
                case GlugUnOpType.Typeof:
                    parent.AppendTypeof();
                    break;
            }

            if (!ru) parent.AppendPop();
        }

        public override void VisitBiOp(BiOp val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            if (val.Op == GlugBiOpType.Call) {
                visitForOsl(val.ExprR, parent);
                visitForValue(val.ExprL, parent);
                parent.AppendCall();

                if (!ru) parent.AppendShpRv(0);
            } else if (val.Op == GlugBiOpType.Assign) {
                if (val.ExprL.IsOnStackList) {
                    visitForOsl(val.ExprR, parent);

                    var list = ((OnStackList)(val.ExprL)).List;
                    var count = list.Count;

                    parent.AppendShpRv(count);
                    for (int i = count - 1; i >= 0; --i) ((VarRef)(list[i])).Variable.CreateStoreInstr(parent);

                    if (ru) parent.AppendLdNil();
                } else if (val.ExprL is VarRef vr) {
                    visitForValue(val.ExprR, parent);
                    if (ru) parent.AppendDup();
                    vr.Variable.CreateStoreInstr(parent);
                } else if (val.ExprL is BiOp { Op: GlugBiOpType.Index } ind) {
                    visitForValue(val.ExprR, parent);
                    if (ru) parent.AppendDup();
                    visitForValue(ind.ExprL, parent);
                    visitForValue(ind.ExprR, parent);
                    parent.AppendUen();
                } else if (val.ExprL is Metatable mt) {
                    visitForValue(val.ExprR, parent);
                    if (ru) parent.AppendDup();
                    visitForValue(mt.Table, parent);
                    parent.AppendSmt();
                } else {
                    throw new ArgumentOutOfRangeException();
                }
            } else if (val.Op == GlugBiOpType.Concat) {
                if (ru) {
                    visitForOsl(val.ExprL, parent);
                    visitForOsl(val.ExprR, parent);
                    parent.AppendPopDel();
                } else {
                    visitForDiscard(val.ExprL, parent);
                    visitForDiscard(val.ExprR, parent);
                }
            } else {
                visitForValue(val.ExprL, parent);
                visitForValue(val.ExprR, parent);

                parent.AppendInstruction(val.Op switch {
                    GlugBiOpType.Add => GlosOp.Add,
                    GlugBiOpType.Sub => GlosOp.Sub,
                    GlugBiOpType.Mul => GlosOp.Mul,
                    GlugBiOpType.Div => GlosOp.Div,
                    GlugBiOpType.Mod => GlosOp.Mod,
                    GlugBiOpType.Lsh => GlosOp.Lsh,
                    GlugBiOpType.Rsh => GlosOp.Rsh,
                    GlugBiOpType.And => GlosOp.And,
                    GlugBiOpType.Orr => GlosOp.Orr,
                    GlugBiOpType.Xor => GlosOp.Xor,
                    GlugBiOpType.Gtr => GlosOp.Gtr,
                    GlugBiOpType.Lss => GlosOp.Lss,
                    GlugBiOpType.Geq => GlosOp.Geq,
                    GlugBiOpType.Leq => GlosOp.Leq,
                    GlugBiOpType.Equ => GlosOp.Equ,
                    GlugBiOpType.Neq => GlosOp.Neq,
                    GlugBiOpType.Index => GlosOp.Ren,
                    _ => GlosOp.Nop, // Add a exception here
                });

                if (!ru) parent.AppendPop();
            }
        }

        public override void VisitLiteralInteger(LiteralInteger val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            if (ru) parent.AppendLd(val.Value);
        }

        public override void VisitLiteralBool(LiteralBool val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            if (ru) parent.AppendLdBool(val.Value);
        }

        public override void VisitLiteralString(LiteralString val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            if (ru) parent.AppendLdStr(val.Value);
        }

        public override void VisitLiteralNil(LiteralNil val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            if (ru) parent.AppendLdNil();
        }

        public override void VisitVarRef(VarRef val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            val.Variable.CreateLoadInstr(parent);
            if (!ru) parent.AppendPop();
        }

        public override void VisitTableDef(TableDef val, CodeGenContext ctx) {
            var (parent, ru) = ctx;

            parent.AppendLdNTbl();

            foreach (var (key, value) in val.Pairs) {
                visitForValue(key, parent);
                visitForValue(value, parent);
                parent.AppendIen();
            }

            if (!ru) parent.AppendPop();
        }

        public override void VisitMetatable(Metatable val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            visitForValue(val.Table, parent);
            parent.AppendGmt();
            if (!ru) parent.AppendPop();
        }
    }
}
