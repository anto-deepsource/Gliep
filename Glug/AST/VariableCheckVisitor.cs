using System;
using System.Collections.Generic;
using System.Text;

namespace GeminiLab.Glug.AST {
    public class FunctionAndVarDefVisitor : RecursiveVisitor {
        public VariableTable RootTable { get; }
        public VariableTable CurrentScope { get; set; }
        public IList<Function> Functions { get; } = new List<Function>();

        public FunctionAndVarDefVisitor() {
            CurrentScope = RootTable = new VariableTable(null!, null!);
        }

        public override void VisitFunction(Function val) {
            Functions.Add(val);

            if (val.Name != null) {
                val.Self = CurrentScope.CreateVariable(val.Name);
            }

            var oldScope = CurrentScope;
            CurrentScope = val.VariableTable = new VariableTable(val, oldScope);

            for (int i = 0; i < val.Params.Count; ++i) {
                CurrentScope.CreateVariable(val.Params[i], i);
            }
            
            base.VisitFunction(val);
            CurrentScope = oldScope;
        }

        public override void VisitVarRef(VarRef val) {
            base.VisitVarRef(val);

            if (val.IsDef) CurrentScope.CreateVariable(val.Id);
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
            base.VisitBiOp(val);

            if (val.Op == GlugBiOpType.Assign) {
                if (!val.ExprL.IsVarRef) {
                    throw new ArgumentOutOfRangeException();
                }

                if (val.ExprL is VarRef vr) {
                    vr.Var.MarkAssigned();
                } else if (val.ExprL is OnStackList osl) {
                    foreach (var expr in osl.List) {
                        ((VarRef)expr).Var.MarkAssigned();
                    }
                } else {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void VisitVarRef(VarRef val) {
            base.VisitVarRef(val);

            if (!CurrentScope.TryLookupVariable(val.Id, out var v)) {
                v = RootTable.CreateVariable(val.Id);
            }

            val.Var = v;
            v.HintUsedIn(CurrentScope);
        }
    }
}
