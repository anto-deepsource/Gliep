using System;
using System.Collections.Generic;
using System.Text;

namespace GeminiLab.Glug.AST {
    public class VariableCheckVisitor : RecursiveVisitor {
        public VariableTable RootTable { get; }
        public VariableTable CurrentScope { get; set; }

        public VariableCheckVisitor() {
            RootTable = new VariableTable(null!, null!);
            CurrentScope = new VariableTable(null!, RootTable);
        }

        public override void VisitFunction(Function val) {
            var oldScope = CurrentScope;
            CurrentScope = val.Variables = new VariableTable(val, oldScope);

            base.VisitFunction(val);
            CurrentScope = oldScope;
        }

        public override void VisitBiOp(BiOp val) {
            base.VisitBiOp(val);

            if (val.Op == GlugBiOpType.Assign) {
                if (!(val.ExprL.IsOnStackList || val.ExprL.IsVarRef)) {
                    throw new ArgumentOutOfRangeException();
                }
            } else {
                if (val.ExprL.IsOnStackList) throw new ArgumentOutOfRangeException();
                if (val.Op != GlugBiOpType.Call && val.ExprR.IsOnStackList) throw new ArgumentOutOfRangeException();
            }
        }

        public override void VisitUnOp(UnOp val) {
            base.VisitUnOp(val);

            if (val.Expr.IsOnStackList) throw new ArgumentOutOfRangeException();
        }

        public override void VisitBlock(Block val) {
            base.VisitBlock(val);

            val.IsOnStackList = val.Statements.Count > 0 && val.Statements[^1].IsOnStackList;
        }

        public override void VisitVarRef(VarRef val) {
            base.VisitVarRef(val);

            val.IsVarRef = true;

            Variable v;
            if (val.IsDef) {
                if (!CurrentScope.TryLookupVariableLocally(val.Id, out v)) {
                    v = CurrentScope.CreateVariable(val.Id);
                }
            } else {
                if (!CurrentScope.TryLookupVariable(val.Id, out v)) {
                    v = RootTable.CreateVariable(val.Id);
                }
            }

            val.Var = v;
            v.HintUsedIn(CurrentScope);
        }
    }
}
