using System;
using System.Linq;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class NodeGenericInfo {
        public bool IsPseudo      = false;
        public bool IsOnStackList = false;
        public bool IsAssignable  = false;
    }

    public class NodeGenericInfoVisitor : RecursiveVisitor {
        protected override void PreVisit(Node node) {
            var info = Pass.NodeInformation<NodeGenericInfo>(node);
            info.IsOnStackList = false;
            info.IsAssignable = false;
        }

        public override void VisitVarRef(VarRef val) {
            base.VisitVarRef(val);

            Pass.NodeInformation<NodeGenericInfo>(val).IsAssignable = true;
        }

        public override void VisitIf(If val) {
            base.VisitIf(val);

            Pass.NodeInformation<NodeGenericInfo>(val).IsOnStackList =
                val.Branches.Any(b => Pass.NodeInformation<NodeGenericInfo>(b.Body).IsOnStackList) || (val.ElseBranch != null && Pass.NodeInformation<NodeGenericInfo>(val.ElseBranch).IsOnStackList);
        }

        public override void VisitWhile(While val) {
            base.VisitWhile(val);

            Pass.NodeInformation<NodeGenericInfo>(val).IsOnStackList =
                Pass.NodeInformation<NodeGenericInfo>(val.Body).IsOnStackList || Pass.NodeInformation<BreakableInfo>(val).Breaks.Any(b => Pass.NodeInformation<NodeGenericInfo>(b).IsOnStackList);
        }

        public override void VisitFor(For val) {
            base.VisitFor(val);

            Pass.NodeInformation<NodeGenericInfo>(val).IsOnStackList =
                Pass.NodeInformation<NodeGenericInfo>(val.Body).IsOnStackList || Pass.NodeInformation<BreakableInfo>(val).Breaks.Any(b => Pass.NodeInformation<NodeGenericInfo>(b).IsOnStackList);
        }

        public override void VisitBreak(Break val) {
            base.VisitBreak(val);

            Pass.NodeInformation<NodeGenericInfo>(val).IsOnStackList = Pass.NodeInformation<NodeGenericInfo>(val.Expr).IsOnStackList;
        }

        public override void VisitOnStackList(OnStackList val) {
            base.VisitOnStackList(val);

            var info = Pass.NodeInformation<NodeGenericInfo>(val);

            info.IsOnStackList = true;
            info.IsAssignable = val.List.All(v => Pass.NodeInformation<NodeGenericInfo>(v).IsAssignable && !(v is OnStackList));
        }

        public override void VisitBlock(Block val) {
            base.VisitBlock(val);

            Pass.NodeInformation<NodeGenericInfo>(val).IsOnStackList = val.List.Count > 0 && Pass.NodeInformation<NodeGenericInfo>(val.List[^1]).IsOnStackList;
        }

        public override void VisitBiOp(BiOp val) {
            base.VisitBiOp(val);

            var info = Pass.NodeInformation<NodeGenericInfo>(val);

            info.IsOnStackList =
                val.Op == GlugBiOpType.Call
             || val.Op == GlugBiOpType.Concat
             || (val.Op == GlugBiOpType.Assign && val.ExprL is OnStackList);

            if (val.Op == GlugBiOpType.Assign && !Pass.NodeInformation<NodeGenericInfo>(val.ExprL).IsAssignable) {
                throw new ArgumentOutOfRangeException();
            }

            info.IsAssignable =
                val.Op == GlugBiOpType.Index
             || val.Op == GlugBiOpType.IndexLocal;
        }

        public override void VisitSysCall(SysCall val) {
            base.VisitSysCall(val);

            Pass.NodeInformation<NodeGenericInfo>(val).IsOnStackList = val.Result == SysCall.ResultType.Osl || val.Result == SysCall.ResultType.None;
        }

        public override void VisitMetatable(Metatable val) {
            base.VisitMetatable(val);

            Pass.NodeInformation<NodeGenericInfo>(val).IsAssignable = true;
        }
    }
}
