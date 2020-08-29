using System;
using System.Collections.Generic;

using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class WhileBreakPairingVisitor : RecursiveInVisitor<Breakable?> {
        private readonly NodeInformation _info;

        public WhileBreakPairingVisitor(NodeInformation info) {
            _info = info;
        }

        public override void VisitWhile(While val, Breakable? arg) {
            _info.Breaks[val] = new List<Break>();
            base.VisitWhile(val, val);
        }

        public override void VisitFor(For val, Breakable? arg) {
            _info.Breaks[val] = new List<Break>();
            base.VisitFor(val, val);
        }

        public override void VisitBreak(Break val, Breakable? w) {
            _info.BreakParent[val] = w ?? throw new ArgumentOutOfRangeException();
            _info.Breaks[w].Add(val);
            base.VisitBreak(val, w);
        }
    }
}
