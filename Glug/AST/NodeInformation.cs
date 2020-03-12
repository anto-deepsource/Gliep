using System.Collections.Generic;

namespace GeminiLab.Glug.AST {
    public class NodeInformation {
        public Dictionary<Node, bool> IsAssignable { get; set; } = new Dictionary<Node, bool>();
        public Dictionary<Node, bool> IsOnStackList { get; set; } = new Dictionary<Node, bool>();

        public Dictionary<Break, While> BreakParent { get; set; } = new Dictionary<Break, While>();
        public Dictionary<While, IList<Break>> Breaks { get; set; } = new Dictionary<While, IList<Break>>();

        public Dictionary<Function, VariableTable> VariableTable { get; set; } = new Dictionary<Function, VariableTable>();
        public Dictionary<Node, Variable> Variable { get; set; } = new Dictionary<Node, Variable>();
    }
}
