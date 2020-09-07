using System.Collections.Generic;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public class NodeInformation {
        public Dictionary<Node, bool> IsAssignable { get; set; } = new Dictionary<Node, bool>();
        public Dictionary<Node, bool> IsOnStackList { get; set; } = new Dictionary<Node, bool>();

        public Dictionary<Break, Breakable> BreakParent { get; set; } = new Dictionary<Break, Breakable>();
        public Dictionary<Breakable, IList<Break>> Breaks { get; set; } = new Dictionary<Breakable, IList<Break>>();

        public Dictionary<Function, VariableTable> VariableTable { get; set; } = new Dictionary<Function, VariableTable>();
        public Dictionary<Node, Variable> Variable { get; set; } = new Dictionary<Node, Variable>();

        public Dictionary<Node, Dictionary<string, Variable>> PrivateVariables { get; set; } = new Dictionary<Node, Dictionary<string, Variable>>();
    }
}
