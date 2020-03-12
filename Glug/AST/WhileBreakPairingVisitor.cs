using System;
using System.Collections.Generic;

namespace GeminiLab.Glug.AST {
    public class WhileBreakPairingVisitor : RecursiveInVisitor<While?> {
        private readonly NodeInformation _info;

        public WhileBreakPairingVisitor(NodeInformation info) {
            _info = info;
        }

        public override void VisitWhile(While val, While? arg) {
            _info.Breaks[val] = new List<Break>();
            base.VisitWhile(val, val);
        }

        public override void VisitBreak(Break val, While? w) {
            _info.BreakParent[val] = w ?? throw new ArgumentOutOfRangeException();
            _info.Breaks[w].Add(val);
            base.VisitBreak(val, w);
        }
    }
}