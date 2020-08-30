using GeminiLab.Glug.AST;
using GeminiLab.Glug.PostProcess;

namespace GeminiLab.Glute.Compile {
    public class GluteVarRefVisitor : VarRefVisitor {
        public GluteVarRefVisitor(VariableTable root, NodeInformation info) : base(root, info) { }

        public override void VisitVarRef(VarRef val, VariableTable currentScope, bool allowImplicitDeclaration) {
            if (!_info.Variable.TryGetValue(val, out _)) {
                if (!currentScope.TryLookupVariable(val.Id, out var v)) {
                    v = currentScope.CreateDynamicVariable(val.Id);
                }

                _info.Variable[val] = v;
            }

            _info.Variable[val].MarkUsedIn(currentScope);
        }
    }
}
