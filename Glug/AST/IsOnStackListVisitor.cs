using System.Linq;

namespace GeminiLab.Glug.AST {
    public class IsOnStackListVisitor : RecursiveVisitor {
        private readonly NodeInformation _info;

        public IsOnStackListVisitor(NodeInformation info) {
            _info = info;
        }

        protected override void PreVisit(Node node) {
            _info.IsOnStackList[node] = false;
        }

        public override void VisitIf(If val) {
            base.VisitIf(val);

            _info.IsOnStackList[val] = val.Branches.Any(b => _info.IsOnStackList[b.Body]) || (val.ElseBranch != null && _info.IsOnStackList[val.ElseBranch]);
        }

        public override void VisitWhile(While val) {
            base.VisitWhile(val);

            _info.IsOnStackList[val] = _info.IsOnStackList[val.Body] || _info.Breaks[val].Any(b => _info.IsOnStackList[b]);
        }

        public override void VisitBreak(Break val) {
            base.VisitBreak(val);

            _info.IsOnStackList[val] = _info.IsOnStackList[val.Expr];
        }

        public override void VisitOnStackList(OnStackList val) {
            base.VisitOnStackList(val);

            _info.IsOnStackList[val] = true;
        }

        public override void VisitBlock(Block val) {
            base.VisitBlock(val);

            _info.IsOnStackList[val] = val.List.Count > 0 && _info.IsOnStackList[val.List[^1]];
        }

        public override void VisitBiOp(BiOp val) {
            base.VisitBiOp(val);

            _info.IsOnStackList[val] = val.Op == GlugBiOpType.Call || val.Op == GlugBiOpType.Concat;
        }
    }
}