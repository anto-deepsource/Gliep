using System;
using System.Collections.Generic;
using System.Linq;
using GeminiLab.Glos;
using GeminiLab.Glos.CodeGenerator;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public struct CodeGenContext {
        public readonly GlosFunctionBuilder CurrentFunction;
        public readonly bool ResultUsed;

        public CodeGenContext(GlosFunctionBuilder currentFunction, bool resultUsed) {
            CurrentFunction = currentFunction;
            ResultUsed = resultUsed;
        }


        public void Deconstruct(out GlosFunctionBuilder currentFunction, out bool resultUsed) => (currentFunction, resultUsed) = (CurrentFunction, ResultUsed);
    }

    public class CodeGenVisitor : InVisitor<CodeGenContext> {
        private readonly NodeInformation _info;
        private readonly Dictionary<While, Label> _whileEndLabel = new Dictionary<While, Label>();
        private readonly Dictionary<While, bool> _whileResultUsed = new Dictionary<While, bool>();

        public CodeGenVisitor(NodeInformation info) {
            _info = info;
        }

        public GlosUnitBuilder Builder { get; } = new GlosUnitBuilder();


        private void visitForDiscard(Expr expr, GlosFunctionBuilder parent) {
            Visit(expr, new CodeGenContext(parent, false));
        }

        private void visitForValue(Expr expr, GlosFunctionBuilder parent) {
            Visit(expr, new CodeGenContext(parent, true));

            if (_info.IsOnStackList[expr]) parent.AppendShpRv(1);
        }
        
        private void visitForOsl(Expr expr, GlosFunctionBuilder parent) {
            if (!_info.IsOnStackList[expr]) parent.AppendLdDel();

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

            var variables = _info.VariableTable[val].Variables.Values.ToArray();
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
                var self = _info.Variable[val];

                if (self != null || ru) {
                    parent.AppendLdFun(fun);
                    parent.AppendBind();
                }

                if (self != null) {
                    if (ru) parent.AppendDup();
                    self.CreateStoreInstr(parent);
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
                else if (_info.IsOnStackList[val]) visitForOsl(branch.Body, parent);
                else visitForValue(branch.Body, parent);

                parent.AppendB(endLabel);
            }

            parent.InsertLabel(nextLabel!); // brc must be at least 1 when this ast is well-formed
            if (val.ElseBranch != null) {
                if (!ru) visitForDiscard(val.ElseBranch, parent);
                else if (_info.IsOnStackList[val]) visitForOsl(val.ElseBranch, parent);
                else visitForValue(val.ElseBranch, parent);
            } else if (ru) {
                if (_info.IsOnStackList[val]) parent.AppendLdDel();
                else parent.AppendLdNil();
            }

            parent.InsertLabel(endLabel);
        }

        public override void VisitWhile(While val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            var endLabel = _whileEndLabel[val] = parent.AllocateLabel();
            _whileResultUsed[val] = ru;
            
            if (ru) {
                if (_info.IsOnStackList[val]) parent.AppendLdDel();
                else parent.AppendLdNil();
            }

            var beginLabel = parent.AllocateAndInsertLabel();

            visitForValue(val.Condition, parent);
            parent.AppendBf(endLabel);

            if (ru) {
                if (_info.IsOnStackList[val]) parent.AppendShpRv(0);
                else parent.AppendPop();
            }

            parent.AppendLdDel();
            
            if (!ru) visitForDiscard(val.Body, parent);
            else if (_info.IsOnStackList[val]) visitForOsl(val.Body, parent);
            else visitForValue(val.Body, parent);

            parent.AppendPopDel();
            parent.AppendB(beginLabel);

            parent.InsertLabel(endLabel);
        }

        public override void VisitFor(For val, CodeGenContext arg) {
            throw new NotImplementedException();
        }

        public override void VisitReturn(Return val, CodeGenContext ctx) {
            var (parent, _) = ctx;

            visitForOsl(val.Expr, parent);
            parent.AppendRet();
        }

        public override void VisitBreak(Break val, CodeGenContext ctx) {
            var (parent, _) = ctx;

            parent.AppendShpRv(0);
            if (!_whileResultUsed[_info.BreakParent[val]]) visitForDiscard(val.Expr, parent);
            else if (_info.IsOnStackList[_info.BreakParent[val]]) visitForOsl(val.Expr, parent);
            else visitForValue(val.Expr, parent);
            parent.AppendB(_whileEndLabel[_info.BreakParent[val]]);
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
                case GlugUnOpType.IsNil:
                    parent.AppendIsNil();
                    break;
            }

            if (!ru) parent.AppendPop();
        }

        private void createStoreInstr(Node node, GlosFunctionBuilder parent) {
            switch (node) {
                case OnStackList { List: var list } osl when _info.IsAssignable[osl]:
                    var count = list.Count;
                    parent.AppendShpRv(count);
                    for (int i = count - 1; i >= 0; --i) createStoreInstr(list[i], parent);
                    break;
                case VarRef vr:
                    _info.Variable[vr].CreateStoreInstr(parent);
                    break;
                case BiOp { Op: GlugBiOpType.Index, ExprL: var indexee, ExprR: PseudoIndex { IsTail: var isTail } }:
                    visitForValue(indexee, parent);
                    parent.AppendPshv();
                    break;
                case BiOp { Op: GlugBiOpType.Index, ExprL: var indexee, ExprR: var index }:
                    visitForValue(indexee, parent);
                    visitForValue(index, parent);
                    parent.AppendUen();
                    break;
                case BiOp { Op: GlugBiOpType.IndexLocal, ExprL: var indexee, ExprR: var index }:
                    visitForValue(indexee, parent);
                    visitForValue(index, parent);
                    parent.AppendUenL();
                    break;
                case Metatable { Table: var table }:
                    visitForValue(table, parent);
                    parent.AppendSmt();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void VisitBiOp(BiOp val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            if (val.Op == GlugBiOpType.Call) {
                visitForOsl(val.ExprR, parent);
                visitForValue(val.ExprL, parent);
                parent.AppendCall();

                if (!ru) parent.AppendShpRv(0);
            } else if (val.Op == GlugBiOpType.Assign) {
                if (val.ExprL is OnStackList) {
                    visitForOsl(val.ExprR, parent);
                    if (ru) parent.AppendDupList();
                } else {
                    visitForValue(val.ExprR, parent);
                    if (ru) parent.AppendDup();
                }

                createStoreInstr(val.ExprL, parent);
            } else if (val.Op == GlugBiOpType.Concat) {
                if (ru) {
                    visitForOsl(val.ExprL, parent);
                    visitForOsl(val.ExprR, parent);
                    parent.AppendPopDel();
                } else {
                    visitForDiscard(val.ExprL, parent);
                    visitForDiscard(val.ExprR, parent);
                }
            }  else if (BiOp.IsShortCircuitOp(val.Op)) {
                visitForValue(val.ExprL, parent);
                
                parent.AppendDup();

                var labelAnother = parent.AllocateLabel();
                var labelEnd = parent.AllocateLabel();

                if (val.Op == GlugBiOpType.ShortCircuitAnd) {
                    parent.AppendBt(labelAnother);
                    parent.AppendB(labelEnd);
                } else if (val.Op == GlugBiOpType.ShortCircuitOrr) {
                    parent.AppendBf(labelAnother);
                    parent.AppendB(labelEnd);
                } else if (val.Op == GlugBiOpType.NullCoalescing) {
                    parent.AppendBn(labelAnother);
                    parent.AppendB(labelEnd);
                }
                
                parent.InsertLabel(labelAnother);
                
                parent.AppendPop();
                visitForValue(val.ExprR, parent);
                
                parent.InsertLabel(labelEnd);

                if (!ru) parent.AppendPop();
            } else if (val.Op == GlugBiOpType.Index && val.ExprR is PseudoIndex { IsTail: var isTail }) {
                visitForValue(val.ExprL, parent);
                if (isTail) {
                    parent.AppendPopv();
                } else {
                    throw new NotImplementedException();
                }
                
                if (!ru) parent.AppendPop();
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
                    GlugBiOpType.IndexLocal => GlosOp.RenL,
                    _ => GlosOp.Nop, // Add a exception here
                });

                if (!ru) parent.AppendPop();
            }
        }

        public override void VisitLiteralInteger(LiteralInteger val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            if (ru) parent.AppendLd(val.Value);
        }

        public override void VisitLiteralFloat(LiteralFloat val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            if (ru) parent.AppendLdFlt(val.Value);
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
            _info.Variable[val].CreateLoadInstr(parent);
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

        public override void VisitVectorDef(VectorDef val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            
            parent.AppendLdNVec();

            foreach (var item in val.Items) {
                visitForValue(item, parent);
                parent.AppendIniv();
            }
            
            if (!ru) parent.AppendPop();
        }
        
        public override void VisitMetatable(Metatable val, CodeGenContext ctx) {
            var (parent, ru) = ctx;
            visitForValue(val.Table, parent);
            parent.AppendGmt();
            if (!ru) parent.AppendPop();
        }
        
        public override void VisitPseudoIndex(PseudoIndex val, CodeGenContext arg) {
            throw new InvalidOperationException();
        }
        
        public override void VisitSysCall(SysCall val, CodeGenContext ctx) {
            var (parent, ru) = ctx;

            foreach (var input in val.Inputs) {
                visitForAny(input, parent);
            }

            parent.AppendSyscall(val.Id);

            if (val.Result == SysCall.ResultType.Value && !ru) parent.AppendPop();
            if (val.Result == SysCall.ResultType.Osl && !ru) parent.AppendShpRv(0);
            if (val.Result == SysCall.ResultType.None && ru) parent.AppendLdDel();
        }

        public override void VisitToValue(ToValue val, CodeGenContext arg) {
            visitForValue(val.Child, arg.CurrentFunction);
        }
    }
}
