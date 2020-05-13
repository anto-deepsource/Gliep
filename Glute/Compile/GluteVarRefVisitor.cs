using GeminiLab.Glug.AST;
using GeminiLab.Glug.PostProcess;

namespace GeminiLab.Glute.Compile {
    public class GluteVarRefVisitor : VarRefVisitor {
        public GluteVarRefVisitor(VariableTable root, NodeInformation info) : base(root, info) { }

        public override void VisitVarRef(VarRef val, VarRefVisitorContext ctx) {
            var (scope, aid) = ctx;
            
            if (!_info.Variable.TryGetValue(val, out _)) {
                if (!scope.TryLookupVariable(val.Id, out var v)) {
                    v = scope.CreateDynamicVariable(val.Id);
                }

                _info.Variable[val] = v;
            }

            _info.Variable[val].MarkUsedIn(scope);
        }
    }
}
