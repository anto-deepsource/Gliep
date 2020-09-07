using GeminiLab.Glug.AST;
using GeminiLab.Glug.PostProcess;

namespace GeminiLab.Glute.Compile {
    public class GluteVarRefVisitor : VarRefVisitor {
        public override void VisitVarRef(VarRef val, VariableTable currentScope, bool allowImplicitDeclaration) {
            var info = Pass.NodeInformation<VariableAllocationInfo>(val);

            if (info.Variable == null) {
                if (!currentScope.TryLookupVariable(val.Id, out var v)) {
                    v = currentScope.CreateDynamicVariable(val.Id);
                }

                info.Variable = v;
            }

            info.Variable.MarkUsedIn(currentScope);
        }
    }
}
