using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using GeminiLab.Glos.CodeGenerator;

namespace GeminiLab.Glug.AST {
    public enum VariablePlace {
        Argument,
        LocalVariable,
        Context,
        Global,
    }

    public class Variable {
        internal Variable(VariableTable table, string name) {
            Table = table;
            Name = name;
            ArgumentId = -1;
        }

        internal Variable(VariableTable table, string name, int argId) {
            Table = table;
            Name = name;
            ArgumentId = argId;
        }

        public VariableTable Table { get; }

        public string Name { get; }

        public bool RefOutsideScope { get; private set; } = false;

        public bool Assigned { get; private set; } = false;

        public bool IsArgument => ArgumentId >= 0;

        public int ArgumentId { get; }

        public LocalVariable? LocalVariable { get; set; }

        public VariablePlace Place { get; private set; }

        public void DeterminePlace() {
            if (Table.IsRoot) {
                Place = VariablePlace.Global;
            } else if (RefOutsideScope) {
                Place = VariablePlace.Context;
            } else if (Assigned) {
                Place = VariablePlace.LocalVariable;
            } else if (IsArgument) {
                Place = VariablePlace.Argument;
            } else {
                Place = VariablePlace.LocalVariable;
            }
        }

        public void CreateLoadInstr(GlosFunctionBuilder fgen) {
            switch (Place) {
            case VariablePlace.Argument:
                fgen.AppendLdArg(ArgumentId);
                break;
            case VariablePlace.LocalVariable:
                fgen.AppendLdLoc(LocalVariable!);
                break;
            case VariablePlace.Context:
                fgen.AppendLdStr(Name);
                fgen.AppendRvc();
                break;
            case VariablePlace.Global:
                fgen.AppendLdStr(Name);
                fgen.AppendRvg();
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        public void CreateStoreInstr(GlosFunctionBuilder fgen) {
            switch (Place) {
            case VariablePlace.LocalVariable:
                fgen.AppendStLoc(LocalVariable!);
                break;
            case VariablePlace.Context:
                fgen.AppendLdStr(Name);
                fgen.AppendUvc();
                break;
            case VariablePlace.Global:
                fgen.AppendLdStr(Name);
                fgen.AppendUvg();
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        public void HintUsedIn(VariableTable scope) {
            if (scope != Table) RefOutsideScope = true;
        }

        public void MarkAssigned() {
            Assigned = true;
        }
    }

    public class VariableTable {
        public VariableTable(Expr scope, VariableTable? parent) {
            Scope = scope;
            Parent = parent;
        }

        public Expr Scope { get; }

        public VariableTable? Parent { get; }

        public bool IsRoot => Parent == null;

        public IReadOnlyDictionary<string, Variable> Variables => _variables;

        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();

        public bool TryLookupVariableLocally(string name, [NotNullWhen(true)] out Variable? variable) {
            return _variables.TryGetValue(name, out variable);
        }

        public bool TryLookupVariable(string name, [NotNullWhen(true)] out Variable? variable) {
            if (TryLookupVariableLocally(name, out variable)) return true;

            return Parent?.TryLookupVariableLocally(name, out variable) ?? false;
        }

        public Variable CreateVariable(string name) {
            if (_variables.TryGetValue(name, out var rv)) return rv;
            return _variables[name] = new Variable(this, name);
        }

        public Variable CreateVariable(string name, int argId) {
            if (_variables.TryGetValue(name, out var rv)) return rv;
            return _variables[name] = new Variable(this, name, argId);
        }

        public void DetermineVariablePlace() {
            foreach (var variable in _variables.Values) variable.DeterminePlace();
        }
    }
}
