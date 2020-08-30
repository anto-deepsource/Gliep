using System.Linq;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class VarRefVisitor : RecursiveInVisitor<VariableTable, bool> {
        protected readonly NodeInformation _info;

        public VarRefVisitor(VariableTable root, NodeInformation info) {
            _info = info;
            RootTable = root;
        }

        public VariableTable RootTable { get; }

        public override void VisitFunction(Function val, VariableTable currentScope, bool allowImplicitDeclaration) {
            _info.Variable[val]?.MarkAssigned();

            base.VisitFunction(val, _info.VariableTable[val], false);
        }

        public override void VisitFor(For val, VariableTable currentScope, bool allowImplicitDeclaration) {
            foreach (var varRef in val.IteratorVariables) {
                Visit(varRef, currentScope, true);
            }
            
            Visit(val.Expression, currentScope, false);
            Visit(val.Body, currentScope, false);
        }

        public override void VisitBiOp(BiOp val, VariableTable currentScope, bool allowImplicitDeclaration) {
            if (val.Op == GlugBiOpType.Assign) {
                Visit(val.ExprL, currentScope, true);
                Visit(val.ExprR, currentScope, false);

                if (val.ExprL is VarRef vr) {
                    _info.Variable[vr].MarkAssigned();
                } else if (val.ExprL is OnStackList osl) {
                    foreach (var item in osl.List.OfType<VarRef>()) {
                        _info.Variable[item].MarkAssigned();
                    }
                }
            } else {
                base.VisitBiOp(val, currentScope, false);
            }
        }

        public override void VisitVarRef(VarRef val, VariableTable currentScope, bool allowImplicitDeclaration) {
            if (!_info.Variable.TryGetValue(val, out _)) {
                if (!currentScope.TryLookupVariable(val.Id, out var v)) {
                    v = (allowImplicitDeclaration ? currentScope : RootTable).CreateVariable(val.Id);
                }

                _info.Variable[val] = v;
            }

            _info.Variable[val].MarkUsedIn(currentScope);
        }

        public override void VisitMetatable(Metatable val, VariableTable currentScope, bool allowImplicitDeclaration) {
            base.VisitMetatable(val, currentScope, false);
        }
    }
}