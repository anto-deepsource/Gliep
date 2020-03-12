using System;
using System.Collections.Generic;

namespace GeminiLab.Glug.AST {
    public class VarDefVisitor : RecursiveInVisitor<VariableTable> {
        private readonly NodeInformation _info;

        public VarDefVisitor(NodeInformation info) {
            _info = info;
        }

        public VariableTable RootTable { get; } = new VariableTable(null!, null!);
        public IList<Function> Functions { get; } = new List<Function>();

        public override void VisitFunction(Function val, VariableTable currentScope) {
            Functions.Add(val);

            _info.Variable[val] = val.ExplicitlyNamed ? currentScope.CreateVariable(val.Name) : null!;

            var table = _info.VariableTable[val] = new VariableTable(val, currentScope);

            for (int i = 0; i < val.Parameters.Count; ++i) {
                table.CreateVariable(val.Parameters[i], i);
            }

            base.VisitFunction(val, table);
        }

        public override void VisitVarRef(VarRef val, VariableTable currentScope) {
            base.VisitVarRef(val, currentScope);

            if (val.IsDef) _info.Variable[val] = currentScope.CreateVariable(val.Id);
            else if (val.IsGlobal) _info.Variable[val] = RootTable.CreateVariable(val.Id);
        }

        // place this function here temporarily, TODO: find a better place for it
        public void DetermineVariablePlace() {
            foreach (var function in Functions) {
                _info.VariableTable[function].DetermineVariablePlace();
            }

            RootTable.DetermineVariablePlace();
        }
    }

    public class VarRefVisitor : RecursiveVisitor {
        private readonly NodeInformation _info;

        public VarRefVisitor(VariableTable root, NodeInformation info) {
            _info = info;
            CurrentScope = RootTable = root;
        }

        public VariableTable RootTable { get; }
        public VariableTable CurrentScope { get; set; }

        public override void VisitFunction(Function val) {
            // actually we move this to previous visitor but ... it's not bad here
            _info.Variable[val]?.MarkAssigned();

            var oldScope = CurrentScope;
            CurrentScope = _info.VariableTable[val];

            base.VisitFunction(val);
            CurrentScope = oldScope;
        }

        public override void VisitBiOp(BiOp val) {
            var oldBeingAssigned = _beingAssigned;
            _beingAssigned = false;

            if (val.Op == GlugBiOpType.Assign) {
                if (!_info.IsAssignable[val.ExprL]) {
                    throw new ArgumentOutOfRangeException();
                }

                _beingAssigned = true;
                Visit(val.ExprL);
                _beingAssigned = false;
                Visit(val.ExprR);

                if (val.ExprL is VarRef vr) {
                    _info.Variable[vr].MarkAssigned();
                } else if (val.ExprL is OnStackList osl) {
                    foreach (var expr in osl.List) {
                        _info.Variable[(VarRef)expr].MarkAssigned();
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

            if (!_info.Variable.TryGetValue(val, out _)) {
                if (!CurrentScope.TryLookupVariable(val.Id, out var v)) {
                    v = (_beingAssigned ? CurrentScope : RootTable).CreateVariable(val.Id);
                }

                _info.Variable[val] = v;
            }

            _info.Variable[val].HintUsedIn(CurrentScope);
        }
    }
}
