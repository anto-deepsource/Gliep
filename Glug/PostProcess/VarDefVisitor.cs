using System.Collections.Generic;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
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

            if (val.IsDef) {
                _info.Variable[val] = currentScope.CreateVariable(val.Id);
            } else if (val.IsGlobal) {
                _info.Variable[val] = RootTable.CreateVariable(val.Id);
            }
        }

        public override void VisitFor(For val, VariableTable currentScope) {
            if (!_info.PrivateVariables.ContainsKey(val)) {
                _info.PrivateVariables[val] = new Dictionary<string, Variable>();
            }

            _info.PrivateVariables[val][For.PrivateVariableNameIterateFunction] = currentScope.CreatePrivateVariable(For.PrivateVariableNameIterateFunction);
            _info.PrivateVariables[val][For.PrivateVariableNameStatus] = currentScope.CreatePrivateVariable(For.PrivateVariableNameStatus);
            _info.PrivateVariables[val][For.PrivateVariableNameIterator] = currentScope.CreatePrivateVariable(For.PrivateVariableNameIterator);

            base.VisitFor(val, currentScope);
        }

        // place this function here temporarily, TODO: find a better place for it
        public void DetermineVariablePlace() {
            foreach (var function in Functions) {
                _info.VariableTable[function].DetermineVariablePlace();
            }

            RootTable.DetermineVariablePlace();
        }
    }
}
