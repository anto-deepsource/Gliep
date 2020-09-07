using System.Linq;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class VarRefVisitor : RecursiveInVisitor<VariableTable, bool> {
        // place this function here temporarily, TODO: find a better place for it
        private void DetermineVariablePlace() {
            foreach (var function in Pass.GlobalInformation<VariableAllocationGlobalInfo>().Functions) {
                Pass.NodeInformation<VariableAllocationInfo>(function).VariableTable.DetermineVariablePlace();
            }

            _rootTable.DetermineVariablePlace();
        }
        
        protected override void VisitRoot(Node root) {
            _rootTable = Pass.GlobalInformation<VariableAllocationGlobalInfo>().RootTable;
            
            base.VisitRoot(root);
            
            DetermineVariablePlace();
        }

        private VariableTable _rootTable = null!;

        public override void VisitFunction(Function val, VariableTable currentScope, bool allowImplicitDeclaration) {
            var info = Pass.NodeInformation<VariableAllocationInfo>(val);
            
            info.Variable?.MarkAssigned();

            base.VisitFunction(val, info.VariableTable, false);
        }

        public override void VisitFor(For val, VariableTable currentScope, bool allowImplicitDeclaration) {
            foreach (var varRef in val.IteratorVariables) {
                VisitNode(varRef, currentScope, true);
            }

            VisitNode(val.Expression, currentScope, false);
            VisitNode(val.Body, currentScope, false);
        }

        public override void VisitBiOp(BiOp val, VariableTable currentScope, bool allowImplicitDeclaration) {
            var info = Pass.NodeInformation<VariableAllocationInfo>(val);
            
            if (val.Op == GlugBiOpType.Assign) {
                VisitNode(val.ExprL, currentScope, true);
                VisitNode(val.ExprR, currentScope, false);

                if (val.ExprL is VarRef vr) {
                    Pass.NodeInformation<VariableAllocationInfo>(vr).Variable?.MarkAssigned();
                } else if (val.ExprL is OnStackList osl) {
                    foreach (var item in osl.List.OfType<VarRef>()) {
                        Pass.NodeInformation<VariableAllocationInfo>(item).Variable?.MarkAssigned();
                    }
                }
            } else {
                base.VisitBiOp(val, currentScope, false);
            }
        }

        public override void VisitVarRef(VarRef val, VariableTable currentScope, bool allowImplicitDeclaration) {
            var info = Pass.NodeInformation<VariableAllocationInfo>(val);
            
            if (info.Variable == null) {
                if (!currentScope.TryLookupVariable(val.Id, out var v)) {
                    v = (allowImplicitDeclaration ? currentScope : _rootTable).CreateVariable(val.Id);
                }

                info.Variable = v;
            }

            info.Variable.MarkUsedIn(currentScope);
        }

        public override void VisitMetatable(Metatable val, VariableTable currentScope, bool allowImplicitDeclaration) {
            base.VisitMetatable(val, currentScope, false);
        }
    }
}
