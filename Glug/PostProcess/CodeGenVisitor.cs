using System;
using System.Collections.Generic;
using System.Linq;
using GeminiLab.Glos;
using GeminiLab.Glos.CodeGenerator;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class CodeGenVisitor : InVisitor<FunctionBuilder, bool> {
        private readonly Dictionary<Breakable, Label> _breakableEndLabel            = new Dictionary<Breakable, Label>();
        private readonly Dictionary<Breakable, bool>  _breakableResultUsed          = new Dictionary<Breakable, bool>();
        private readonly Dictionary<Breakable, int>   _breakableGuardianDelimiterId = new Dictionary<Breakable, int>();

        public UnitBuilder Builder { get; } = new UnitBuilder();

        private Dictionary<FunctionBuilder, int> _delCount = new Dictionary<FunctionBuilder, int>();

        private void visitForDiscard(Expr expr, FunctionBuilder fun) {
            VisitNode(expr, fun, false);
        }

        private void visitForValue(Expr expr, FunctionBuilder fun) {
            VisitNode(expr, fun, true);

            if (Pass.NodeInformation<NodeGenericInfo>(expr).IsOnStackList) {
                fun.AppendShpRv(1);
                --_delCount[fun];
            }
        }

        private void visitForOsl(Expr expr, FunctionBuilder fun) {
            if (!Pass.NodeInformation<NodeGenericInfo>(expr).IsOnStackList) {
                fun.AppendLdDel();
                ++_delCount[fun];
            }

            VisitNode(expr, fun, true);
        }

        private void visitForAny(Expr expr, FunctionBuilder fun) {
            VisitNode(expr, fun, true);
        }


        public override void VisitFunction(Function val, FunctionBuilder parent, bool ru) {
            var info = Pass.NodeInformation<VariableAllocationInfo>(val);
            var fun = Builder.AddFunction();

            if (parent == null) fun.SetEntry();
            fun.Name = val.Name;

            var variables = info.VariableTable.Variables.Values.ToArray();
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

            _delCount[fun] = 0;
            visitForAny(val.Body, fun);
            fun.AppendRetIfNone();

            if (parent != null) {
                var self = info.Variable;

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

        public override void VisitIf(If val, FunctionBuilder fun, bool ru) {
            var info = Pass.NodeInformation<NodeGenericInfo>(val);
            Label? nextLabel = null;
            var endLabel = fun.AllocateLabel();

            foreach (var branch in val.Branches) {
                if (nextLabel != null) fun.InsertLabel(nextLabel);
                nextLabel = fun.AllocateLabel();

                visitForValue(branch.Condition, fun);
                fun.AppendBf(nextLabel);

                if (!ru) {
                    visitForDiscard(branch.Body, fun);
                } else if (info.IsOnStackList) {
                    visitForOsl(branch.Body, fun);
                } else {
                    visitForValue(branch.Body, fun);
                }

                fun.AppendB(endLabel);
            }

            fun.InsertLabel(nextLabel!); // brc must be at least 1 when this ast is well-formed
            if (val.ElseBranch != null) {
                if (!ru) {
                    visitForDiscard(val.ElseBranch, fun);
                } else if (info.IsOnStackList) {
                    visitForOsl(val.ElseBranch, fun);
                } else {
                    visitForValue(val.ElseBranch, fun);
                }
            } else if (ru) {
                if (info.IsOnStackList) {
                    fun.AppendLdDel();
                } else {
                    fun.AppendLdNil();
                }
            }

            fun.InsertLabel(endLabel);
        }

        public override void VisitWhile(While val, FunctionBuilder fun, bool ru) {
            var info = Pass.NodeInformation<NodeGenericInfo>(val);
            var endLabel = _breakableEndLabel[val] = fun.AllocateLabel();
            _breakableResultUsed[val] = ru;

            if (ru) {
                if (info.IsOnStackList) {
                    fun.AppendLdDel();
                    ++_delCount[fun];
                } else {
                    fun.AppendLdNil();
                }
            }

            var beginLabel = fun.AllocateAndInsertLabel();

            visitForValue(val.Condition, fun);
            fun.AppendBf(endLabel);

            if (ru) {
                if (info.IsOnStackList) {
                    fun.AppendShpRv(0);
                    --_delCount[fun];
                } else {
                    fun.AppendPop();
                }
            }

            fun.AppendLdDel();
            _breakableGuardianDelimiterId[val] = _delCount[fun];
            ++_delCount[fun];

            if (!ru) {
                visitForDiscard(val.Body, fun);
            } else if (info.IsOnStackList) {
                visitForOsl(val.Body, fun);
            } else {
                visitForValue(val.Body, fun);
            }

            fun.AppendPopDel();
            --_delCount[fun];
            fun.AppendB(beginLabel);

            fun.InsertLabel(endLabel);
        }

        public override void VisitFor(For val, FunctionBuilder fun, bool ru) {
            var info = Pass.NodeInformation<NodeGenericInfo>(val);
            var valInfo = Pass.NodeInformation<VariableAllocationInfo>(val);
            var endLabel = _breakableEndLabel[val] = fun.AllocateLabel();
            _breakableResultUsed[val] = ru;

            var pvFunc = valInfo.PrivateVariables[For.PrivateVariableNameIterateFunction]!;
            var pvStatus = valInfo.PrivateVariables[For.PrivateVariableNameStatus]!;
            var pvIterator = valInfo.PrivateVariables[For.PrivateVariableNameIterator]!;
            var iterVarOsl = new OnStackList(new List<CommaExprListItem>(val.IteratorVariables.Select(x => new CommaExprListItem(CommaExprListItemType.Plain, x))));
            Pass.NodeInformation<NodeGenericInfo>(iterVarOsl).IsPseudo = true;

            if (ru) {
                if (info.IsOnStackList) {
                    fun.AppendLdDel();
                    ++_delCount[fun];
                } else {
                    fun.AppendLdNil();
                }
            }

            visitForOsl(val.Expression, fun);
            fun.AppendShpRv(3);
            --_delCount[fun];

            pvIterator.CreateStoreInstr(fun);
            pvStatus.CreateStoreInstr(fun);
            pvFunc.CreateStoreInstr(fun);

            var beginLabel = fun.AllocateAndInsertLabel();

            fun.AppendLdDel();
            ++_delCount[fun];
            pvStatus.CreateLoadInstr(fun);
            pvIterator.CreateLoadInstr(fun);
            pvFunc.CreateLoadInstr(fun);
            fun.AppendCall();

            fun.AppendDupList();
            ++_delCount[fun];
            createStoreInstr(iterVarOsl, fun);

            fun.AppendShpRv(1);
            --_delCount[fun];
            fun.AppendDup();
            pvIterator.CreateStoreInstr(fun);
            fun.AppendBn(endLabel);

            if (ru) {
                if (info.IsOnStackList) {
                    fun.AppendShpRv(0);
                    --_delCount[fun];
                } else {
                    fun.AppendPop();
                }
            }

            fun.AppendLdDel();
            _breakableGuardianDelimiterId[val] = _delCount[fun];
            ++_delCount[fun];

            if (!ru) {
                visitForDiscard(val.Body, fun);
            } else if (info.IsOnStackList) {
                visitForOsl(val.Body, fun);
            } else {
                visitForValue(val.Body, fun);
            }

            fun.AppendPopDel();
            --_delCount[fun];
            fun.AppendB(beginLabel);

            fun.InsertLabel(endLabel);
        }

        public override void VisitReturn(Return val, FunctionBuilder fun, bool ru) {
            visitForOsl(val.Expr, fun);
            fun.AppendRet();
        }

        public override void VisitBreak(Break val, FunctionBuilder fun, bool ru) {
            var info = Pass.NodeInformation<NodeGenericInfo>(val);
            var breakInfo = Pass.NodeInformation<BreakableInfo>(val);
            var target = breakInfo.Target;

            var guardian = _breakableGuardianDelimiterId[target];
            var delCnt = _delCount[fun];
            while (delCnt > guardian) {
                fun.AppendShpRv(0);
                --delCnt;
            }

            if (!_breakableResultUsed[target]) {
                visitForDiscard(val.Expr, fun);
            } else if (info.IsOnStackList) {
                visitForOsl(val.Expr, fun);
            } else {
                visitForValue(val.Expr, fun);
            }

            fun.AppendB(_breakableEndLabel[breakInfo.Target]);
        }

        protected virtual void VisitCommaExprList(IList<CommaExprListItem> list, FunctionBuilder fun) {
            fun.AppendLdDel();
            ++_delCount[fun];

            foreach (var (type, item) in list) {
                if (type == CommaExprListItemType.Plain) {
                    visitForValue(item, fun);
                } else if (type == CommaExprListItemType.OnStackListUnpack) {
                    visitForOsl(item, fun);
                    fun.AppendPopDel();
                    --_delCount[fun];
                } else if (type == CommaExprListItemType.VectorUnpack) {
                    visitForValue(item, fun);
                    fun.AppendUpv();
                    fun.AppendPopDel();
                }
            }
        }
        
        public override void VisitOnStackList(OnStackList val, FunctionBuilder fun, bool ru) {
            if (ru) {
                VisitCommaExprList(val.List, fun);
            } else {
                foreach (var (_, item) in val.List) {
                    visitForDiscard(item, fun);
                }
            }
        }

        public override void VisitBlock(Block val, FunctionBuilder fun, bool ru) {
            var count = val.List.Count;

            if (count == 0) {
                if (ru) fun.AppendLdNil();
            } else {
                for (int i = 0; i < count; ++i) {
                    if (i < count - 1) {
                        visitForDiscard(val.List[i], fun);
                    } else {
                        if (ru) {
                            visitForAny(val.List[i], fun);
                        } else {
                            visitForDiscard(val.List[i], fun);
                        }
                    }
                }
            }
        }

        public override void VisitUnOp(UnOp val, FunctionBuilder fun, bool ru) {
            visitForValue(val.Expr, fun);

            switch (val.Op) {
            case GlugUnOpType.Neg:
                fun.AppendNeg();
                break;
            case GlugUnOpType.Not:
                fun.AppendNot();
                break;
            case GlugUnOpType.Typeof:
                fun.AppendTypeof();
                break;
            case GlugUnOpType.IsNil:
                fun.AppendIsNil();
                break;
            case GlugUnOpType.Unpack:
                fun.AppendUpv();
                ++_delCount[fun];
                break;
            }

            if (!ru) fun.AppendPop();
        }

        private void createStoreInstr(Node node, FunctionBuilder fun) {
            var info = Pass.NodeInformation<NodeGenericInfo>(node);
            var valInfo = Pass.NodeInformation<VariableAllocationInfo>(node);
            switch (node) {
            // allow pseudo ast nodes
            case OnStackList { List: var list } osl when info.IsPseudo || info.IsAssignable:
                var count = list.Count;
                fun.AppendShpRv(count);
                --_delCount[fun];
                for (int i = count - 1; i >= 0; --i) createStoreInstr(list[i].Expr, fun);
                break;
            case VarRef vr:
                valInfo.Variable!.CreateStoreInstr(fun);
                break;
            case BiOp { Op: GlugBiOpType.Index, ExprL: var indexee, ExprR: PseudoIndex { IsTail: var isTail } }:
                visitForValue(indexee, fun);
                fun.AppendPshv();
                break;
            case BiOp { Op: GlugBiOpType.IndexLocal, ExprL: var indexee, ExprR: PseudoIndex { IsTail: var isTail } }:
                visitForValue(indexee, fun);
                fun.AppendPshv();
                break;
            case BiOp { Op: GlugBiOpType.Index, ExprL: var indexee, ExprR: var index }:
                visitForValue(indexee, fun);
                visitForValue(index, fun);
                fun.AppendUen();
                break;
            case BiOp { Op: GlugBiOpType.IndexLocal, ExprL: var indexee, ExprR: var index }:
                visitForValue(indexee, fun);
                visitForValue(index, fun);
                fun.AppendUenL();
                break;
            case Metatable { Table: var table }:
                visitForValue(table, fun);
                fun.AppendSmt();
                break;
            case Discard _:
                fun.AppendPop();
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        public override void VisitBiOp(BiOp val, FunctionBuilder fun, bool ru) {
            if (val.Op == GlugBiOpType.Call) {
                visitForOsl(val.ExprR, fun);
                visitForValue(val.ExprL, fun);
                fun.AppendCall();

                if (!ru) {
                    fun.AppendShpRv(0);
                    --_delCount[fun];
                }
            } else if (val.Op == GlugBiOpType.Assign) {
                if (val.ExprL is OnStackList) {
                    visitForOsl(val.ExprR, fun);
                    if (ru) {
                        fun.AppendDupList();
                        ++_delCount[fun];
                    }
                } else {
                    visitForValue(val.ExprR, fun);
                    if (ru) fun.AppendDup();
                }

                createStoreInstr(val.ExprL, fun);
            } else if (val.Op == GlugBiOpType.Concat) {
                if (ru) {
                    visitForOsl(val.ExprL, fun);
                    visitForOsl(val.ExprR, fun);
                    fun.AppendPopDel();
                    --_delCount[fun];
                } else {
                    visitForDiscard(val.ExprL, fun);
                    visitForDiscard(val.ExprR, fun);
                }
            } else if (BiOp.IsShortCircuitOp(val.Op)) {
                visitForValue(val.ExprL, fun);

                fun.AppendDup();

                var labelAnother = fun.AllocateLabel();
                var labelEnd = fun.AllocateLabel();

                if (val.Op == GlugBiOpType.ShortCircuitAnd) {
                    fun.AppendBt(labelAnother);
                    fun.AppendB(labelEnd);
                } else if (val.Op == GlugBiOpType.ShortCircuitOrr) {
                    fun.AppendBf(labelAnother);
                    fun.AppendB(labelEnd);
                } else if (val.Op == GlugBiOpType.NullCoalescing) {
                    fun.AppendBn(labelAnother);
                    fun.AppendB(labelEnd);
                }

                fun.InsertLabel(labelAnother);

                fun.AppendPop();
                visitForValue(val.ExprR, fun);

                fun.InsertLabel(labelEnd);

                if (!ru) fun.AppendPop();
            } else if (val.Op == GlugBiOpType.Index && val.ExprR is PseudoIndex { IsTail: var isTail }) {
                visitForValue(val.ExprL, fun);
                if (isTail) {
                    fun.AppendPopv();
                } else {
                    throw new NotImplementedException();
                }

                if (!ru) fun.AppendPop();
            } else {
                visitForValue(val.ExprL, fun);
                visitForValue(val.ExprR, fun);

                fun.AppendInstruction(val.Op switch {
                    GlugBiOpType.Add        => GlosOp.Add,
                    GlugBiOpType.Sub        => GlosOp.Sub,
                    GlugBiOpType.Mul        => GlosOp.Mul,
                    GlugBiOpType.Div        => GlosOp.Div,
                    GlugBiOpType.Mod        => GlosOp.Mod,
                    GlugBiOpType.Lsh        => GlosOp.Lsh,
                    GlugBiOpType.Rsh        => GlosOp.Rsh,
                    GlugBiOpType.And        => GlosOp.And,
                    GlugBiOpType.Orr        => GlosOp.Orr,
                    GlugBiOpType.Xor        => GlosOp.Xor,
                    GlugBiOpType.Gtr        => GlosOp.Gtr,
                    GlugBiOpType.Lss        => GlosOp.Lss,
                    GlugBiOpType.Geq        => GlosOp.Geq,
                    GlugBiOpType.Leq        => GlosOp.Leq,
                    GlugBiOpType.Equ        => GlosOp.Equ,
                    GlugBiOpType.Neq        => GlosOp.Neq,
                    GlugBiOpType.Index      => GlosOp.Ren,
                    GlugBiOpType.IndexLocal => GlosOp.RenL,
                    _                       => GlosOp.Nop, // Add a exception here
                });

                if (!ru) fun.AppendPop();
            }
        }

        public override void VisitLiteralInteger(LiteralInteger val, FunctionBuilder fun, bool ru) {
            if (ru) fun.AppendLd(val.Value);
        }

        public override void VisitLiteralFloat(LiteralFloat val, FunctionBuilder fun, bool ru) {
            if (ru) fun.AppendLdFlt(val.Value);
        }

        public override void VisitLiteralBool(LiteralBool val, FunctionBuilder fun, bool ru) {
            if (ru) fun.AppendLdBool(val.Value);
        }

        public override void VisitLiteralString(LiteralString val, FunctionBuilder fun, bool ru) {
            if (ru) fun.AppendLdStr(val.Value);
        }

        public override void VisitLiteralNil(LiteralNil val, FunctionBuilder fun, bool ru) {
            if (ru) fun.AppendLdNil();
        }

        public override void VisitVarRef(VarRef val, FunctionBuilder fun, bool ru) {
            var valInfo = Pass.NodeInformation<VariableAllocationInfo>(val);
            valInfo.Variable!.CreateLoadInstr(fun);
            if (!ru) fun.AppendPop();
        }

        public override void VisitTableDef(TableDef val, FunctionBuilder fun, bool ru) {
            fun.AppendLdNTbl();

            foreach (var (key, value) in val.Pairs) {
                visitForValue(key, fun);
                visitForValue(value, fun);
                fun.AppendIen();
            }

            if (!ru) fun.AppendPop();
        }

        public override void VisitVectorDef(VectorDef val, FunctionBuilder fun, bool ru) {
            if (ru) {
                VisitCommaExprList(val.Items, fun);
                fun.AppendPkv();
                --_delCount[fun];
            } else {
                foreach (var (_, item) in val.Items) {
                    visitForDiscard(item, fun);
                }
            }
        }

        public override void VisitMetatable(Metatable val, FunctionBuilder fun, bool ru) {
            // TODO: when result not used, gmt is unnecessary
            visitForValue(val.Table, fun);
            fun.AppendGmt();
            if (!ru) fun.AppendPop();
        }

        public override void VisitPseudoIndex(PseudoIndex val, FunctionBuilder fun, bool ru) {
            throw new GlugEvaluationOfPseudoExpressionException(val.IsTail ? PseudoExpressionType.VectorEnd : PseudoExpressionType.VectorHead, val.Position);
        }

        public override void VisitDiscard(Discard val, FunctionBuilder arg0, bool arg1) {
            throw new GlugEvaluationOfPseudoExpressionException(PseudoExpressionType.Discard, val.Position);
        }

        public override void VisitSysCall(SysCall val, FunctionBuilder fun, bool ru) {
            foreach (var input in val.Inputs) {
                visitForAny(input, fun);
            }

            fun.AppendSyscall(val.Id);

            if (val.Result == SysCall.ResultType.Value && !ru) fun.AppendPop();
            if (val.Result == SysCall.ResultType.Osl && !ru) fun.AppendShpRv(0);
            if (val.Result == SysCall.ResultType.None && ru) fun.AppendLdDel();
        }

        public override void VisitToValue(ToValue val, FunctionBuilder fun, bool ru) {
            visitForValue(val.Child, fun);
        }
    }
}
