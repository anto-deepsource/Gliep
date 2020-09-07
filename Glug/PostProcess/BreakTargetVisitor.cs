using System;
using System.Collections.Generic;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class BreakableInfo {
        public Breakable   Target = null!;
        public List<Break> Breaks = null!;
    }

    public class BreakTargetVisitor : RecursiveInVisitor<Stack<Breakable>> {
        public override void VisitFunction(Function val, Stack<Breakable> arg) {
            base.VisitFunction(val, new Stack<Breakable>());
        }

        public override void VisitWhile(While val, Stack<Breakable> arg) {
            Pass.NodeInformation<BreakableInfo>(val).Breaks = new List<Break>();
            arg.Push(val);
            base.VisitWhile(val, arg);
            arg.Pop();
        }

        public override void VisitFor(For val, Stack<Breakable> arg) {
            Pass.NodeInformation<BreakableInfo>(val).Breaks = new List<Break>();
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

            Pass.NodeInformation<BreakableInfo>(val).Target = w;
            Pass.NodeInformation<BreakableInfo>(w).Breaks.Add(val);
            base.VisitBreak(val, arg);
        }
    }
}
