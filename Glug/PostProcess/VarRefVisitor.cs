using System.Linq;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public struct VarRefVisitorContext {
        public readonly VariableTable CurrentScope;
        public readonly bool AllowImplicitDeclaration;

        public VarRefVisitorContext(VariableTable currentScope, bool allowImplicitDeclaration) {
            CurrentScope = currentScope;
            AllowImplicitDeclaration = allowImplicitDeclaration;
        }
        
        public void Deconstruct(out VariableTable currentScope, out bool implicitDeclaration) =>
            (currentScope, implicitDeclaration) = (CurrentScope, AllowImplicitDeclaration);
    }

    public class VarRefVisitor : RecursiveInVisitor<VarRefVisitorContext> {
        protected readonly NodeInformation _info;

        public VarRefVisitor(VariableTable root, NodeInformation info) {
            _info = info;
            RootTable = root;
        }

        public VariableTable RootTable { get; }

        public override void VisitFunction(Function val, VarRefVisitorContext ctx) {
            _info.Variable[val]?.MarkAssigned();

            base.VisitFunction(val, new VarRefVisitorContext(_info.VariableTable[val], false));
        }

        public override void VisitFor(For val, VarRefVisitorContext ctx) {
            var (scope, _) = ctx;

            foreach (var varRef in val.IteratorVariables) {
                Visit(varRef, new VarRefVisitorContext(scope, true));
            }
            
            Visit(val.Expression, ctx);
            Visit(val.Body, ctx);
        }

        public override void VisitBiOp(BiOp val, VarRefVisitorContext ctx) {
            var (scope, _) = ctx;

            if (val.Op == GlugBiOpType.Assign) {
                Visit(val.ExprL, new VarRefVisitorContext(scope, true));
                Visit(val.ExprR, new VarRefVisitorContext(scope, false));

                if (val.ExprL is VarRef vr) {
                    _info.Variable[vr].MarkAssigned();
                } else if (val.ExprL is OnStackList osl) {
                    foreach (var item in osl.List.OfType<VarRef>()) {
                        _info.Variable[item].MarkAssigned();
                    }
                }
            } else {
                base.VisitBiOp(val, new VarRefVisitorContext(scope, false));
            }
        }

        public override void VisitVarRef(VarRef val, VarRefVisitorContext ctx) {
            var (scope, aid) = ctx;

            if (!_info.Variable.TryGetValue(val, out _)) {
                if (!scope.TryLookupVariable(val.Id, out var v)) {
                    v = (aid ? scope : RootTable).CreateVariable(val.Id);
                }

                _info.Variable[val] = v;
            }

            _info.Variable[val].MarkUsedIn(scope);
        }

        public override void VisitMetatable(Metatable val, VarRefVisitorContext arg) {
            base.VisitMetatable(val, new VarRefVisitorContext(arg.CurrentScope, false));
        }
    }
}