using System.Collections.Generic;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class VariableAllocationGlobalInfo {
        public VariableTable   RootTable = null!;
        public IList<Function> Functions = null!;
    }

    public class VariableAllocationInfo {
        public Variable?                    Variable         = null!;
        public VariableTable                VariableTable    = null!;
        public Dictionary<string, Variable> PrivateVariables = null!;
    }

    public class VarDefVisitor : RecursiveInVisitor<VariableTable> {
        protected override void VisitRoot(Node root) {
            var gInfo = Pass.GlobalInformation<VariableAllocationGlobalInfo>();
            _rootTable = gInfo.RootTable = new VariableTable(null!, null!);
            gInfo.Functions = new List<Function>();

            VisitNode(root, _rootTable);
        }

        private VariableTable _rootTable = null!;

        public override void VisitFunction(Function val, VariableTable currentScope) {
            Pass.GlobalInformation<VariableAllocationGlobalInfo>().Functions.Add(val);

            var info = Pass.NodeInformation<VariableAllocationInfo>(val);

            info.Variable = val.ExplicitlyNamed ? currentScope.CreateVariable(val.Name) : null!;
            info.VariableTable = new VariableTable(val, currentScope);

            for (int i = 0; i < val.Parameters.Count; ++i) {
                info.VariableTable.CreateVariable(val.Parameters[i], i);
            }

            base.VisitFunction(val, info.VariableTable);
        }

        public override void VisitVarRef(VarRef val, VariableTable currentScope) {
            base.VisitVarRef(val, currentScope);

            if (val.IsDef) {
                Pass.NodeInformation<VariableAllocationInfo>(val).Variable = currentScope.CreateVariable(val.Id);
            } else if (val.IsGlobal) {
                Pass.NodeInformation<VariableAllocationInfo>(val).Variable = _rootTable.CreateVariable(val.Id);
            }
        }

        public override void VisitFor(For val, VariableTable currentScope) {
            var info = Pass.NodeInformation<VariableAllocationInfo>(val);

            info.PrivateVariables = new Dictionary<string, Variable>();

            info.PrivateVariables[For.PrivateVariableNameIterateFunction] = currentScope.CreatePrivateVariable(For.PrivateVariableNameIterateFunction);
            info.PrivateVariables[For.PrivateVariableNameStatus] = currentScope.CreatePrivateVariable(For.PrivateVariableNameStatus);
            info.PrivateVariables[For.PrivateVariableNameIterator] = currentScope.CreatePrivateVariable(For.PrivateVariableNameIterator);

            base.VisitFor(val, currentScope);
        }
    }
}
