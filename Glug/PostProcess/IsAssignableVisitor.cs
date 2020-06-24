using System;
using System.Linq;

using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class IsAssignableVisitor : RecursiveVisitor {
        private readonly NodeInformation _info;
        
        public IsAssignableVisitor(NodeInformation info) {
            _info = info;
        }

        protected override void PreVisit(Node node) {
            _info.IsAssignable[node] = false;
        }

        public override void VisitVarRef(VarRef val) {
            base.VisitVarRef(val);

            _info.IsAssignable[val] = true;
        }

        public override void VisitOnStackList(OnStackList val) {
            base.VisitOnStackList(val);

            _info.IsAssignable[val] = val.List.All(v => _info.IsAssignable[v] && !(v is OnStackList));
        }

        public override void VisitBiOp(BiOp val) {
            base.VisitBiOp(val);

            if (val.Op == GlugBiOpType.Assign && !_info.IsAssignable[val.ExprL]) {
                throw new ArgumentOutOfRangeException();
            }

            _info.IsAssignable[val] = val.Op == GlugBiOpType.Index || val.Op == GlugBiOpType.IndexLocal;
        }

        public override void VisitMetatable(Metatable val) {
            base.VisitMetatable(val);

            _info.IsAssignable[val] = true;
        }
    }
}
