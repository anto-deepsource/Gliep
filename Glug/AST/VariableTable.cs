using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GeminiLab.Glug.AST {
    public class Variable {
        public Variable(VariableTable table) {
            Table = table;
        }

        public VariableTable Table { get; }

        public bool RefOutsideScope { get; private set; } = false;

        public void HintUsedIn(VariableTable scope) {
            if (scope != Table) RefOutsideScope = true;
        }
    }

    public class VariableTable {
        public VariableTable(Expr scope, VariableTable? parent) {
            Scope = scope;
            Parent = parent;
        }

        public Expr Scope { get; }

        public VariableTable? Parent { get; }

        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();

        public bool TryLookupVariableLocally(string name, [NotNullWhen(true)] out Variable? variable) {
            return _variables.TryGetValue(name, out variable);
        }

        public bool TryLookupVariable(string name, [NotNullWhen(true)] out Variable? variable) {
            if (TryLookupVariableLocally(name, out variable)) return true;

            return Parent?.TryLookupVariableLocally(name, out variable) ?? false;
        }

        public Variable CreateVariable(string name) {
            return _variables[name] = new Variable(this);
        }
    }
}