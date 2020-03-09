using System;
using System.Collections.Generic;

namespace GeminiLab.Glug.AST {
    public class VarDefVisitor : RecursiveInVisitor<VariableTable> {
        public VariableTable RootTable { get; } = new VariableTable(null!, null!);
        public IList<Function> Functions { get; } = new List<Function>();

        public override void VisitFunction(Function val, VariableTable currentScope) {
            Functions.Add(val);

            if (val.ExplicitlyNamed) {
                val.Self = currentScope.CreateVariable(val.Name);
            }

            val.VariableTable = new VariableTable(val, currentScope);

            for (int i = 0; i < val.Parameters.Count; ++i) {
                val.VariableTable.CreateVariable(val.Parameters[i], i);
            }

            base.VisitFunction(val, val.VariableTable);
        }

        public override void VisitVarRef(VarRef val, VariableTable currentScope) {
            base.VisitVarRef(val, currentScope);

            if (val.IsDef) val.Variable = currentScope.CreateVariable(val.Id);
            else if (val.IsGlobal) val.Variable = RootTable.CreateVariable(val.Id);
        }

        // place this function here temporarily, TODO: find a better place for it
        public void DetermineVariablePlace() {
            foreach (var function in Functions) {
                function.VariableTable.DetermineVariablePlace();
            }

            RootTable.DetermineVariablePlace();
        }
    }

    public class VarRefVisitor : RecursiveVisitor {
        public VariableTable RootTable { get; }
        public VariableTable CurrentScope { get; set; }

        public VarRefVisitor(VariableTable root) {
            CurrentScope = RootTable = root;
        }

        public override void VisitFunction(Function val) {
            // actually we move this to previous visitor but ... it's not bad here
            val.Self?.MarkAssigned();

            var oldScope = CurrentScope;
            CurrentScope = val.VariableTable;

            base.VisitFunction(val);
            CurrentScope = oldScope;
        }

        public override void VisitBiOp(BiOp val) {
            var oldBeingAssigned = _beingAssigned;
            _beingAssigned = false;

            if (val.Op == GlugBiOpType.Assign) {
                if (!val.ExprL.IsVarRef) {
                    throw new ArgumentOutOfRangeException();
                }

                _beingAssigned = true;
                Visit(val.ExprL);
                _beingAssigned = false;
                Visit(val.ExprR);

                if (val.ExprL is VarRef vr) {
                    vr.Variable.MarkAssigned();
                } else if (val.ExprL is OnStackList osl) {
                    foreach (var expr in osl.List) {
                        ((VarRef)expr).Variable.MarkAssigned();
                    }
                } else {
                    // throw new ArgumentOutOfRangeException();
                }
            } else {
                base.VisitBiOp(val);
            }

            _beingAssigned = oldBeingAssigned;
        }

        private bool _beingAssigned = false;

        public override void VisitVarRef(VarRef val) {
            base.VisitVarRef(val);

            if (val.Variable == null) {
                if (!CurrentScope.TryLookupVariable(val.Id, out var v)) {
                    v = (_beingAssigned ? CurrentScope : RootTable).CreateVariable(val.Id);
                }

                val.Variable = v;
            }

            val.Variable.HintUsedIn(CurrentScope);
        }
    }
}
