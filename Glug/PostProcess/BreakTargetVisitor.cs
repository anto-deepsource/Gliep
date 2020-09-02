using System;
using System.Collections.Generic;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class BreakTargetVisitor : RecursiveInVisitor<Stack<Breakable>> {
        private readonly NodeInformation _info;

        public BreakTargetVisitor(NodeInformation info) {
            _info = info;
        }

        public override void VisitFunction(Function val, Stack<Breakable> arg) {
            base.VisitFunction(val, new Stack<Breakable>());
        }

        public override void VisitWhile(While val, Stack<Breakable> arg) {
            _info.Breaks[val] = new List<Break>();
            arg.Push(val);
            base.VisitWhile(val, arg);
            arg.Pop();
        }

        public override void VisitFor(For val, Stack<Breakable> arg) {
            _info.Breaks[val] = new List<Break>();
            arg.Push(val);
            base.VisitFor(val, arg);
            arg.Pop();
        }

        public override void VisitBreak(Break val, Stack<Breakable> arg) {
            Breakable? w = null;

            if (val.Label != null) {
                foreach (var breakable in arg) {
                    if (breakable.Label == val.Label) {
                        w = breakable;
                        break;
                    }
                }
            } else {
                w = arg.Count > 0 ? arg.Peek() : null;
            }

            if (w == null) throw new ArgumentOutOfRangeException();

            _info.BreakParent[val] = w;
            _info.Breaks[w].Add(val);
            base.VisitBreak(val, arg);
        }
    }
}
