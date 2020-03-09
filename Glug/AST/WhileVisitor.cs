using System;
using System.Collections.Generic;

namespace GeminiLab.Glug.AST {
    public class WhileVisitor : RecursiveInVisitor<While?> {
        public override void VisitFunction(Function val, While? arg) {
            base.VisitFunction(val, null);
        }

        public override void VisitWhile(While val, While? arg) {
            base.VisitWhile(val, val);
        }

        public override void VisitBreak(Break val, While? arg) {
            base.VisitBreak(val, null);
            val.Parent = arg ?? throw new ArgumentOutOfRangeException();
            val.Parent.Breaks.Add(val);
        }
    }
}
