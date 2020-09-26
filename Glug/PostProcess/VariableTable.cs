using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GeminiLab.Glos.CodeGenerator;
using GeminiLab.Glug.AST;

namespace GeminiLab.Glug.PostProcess {
    public enum VariablePlace {
        Argument,
        LocalVariable,
        Context,
        DynamicScope,
        Global,
    }

    public class Variable {
        private Variable(VariableTable table, string name, int argumentId = -1, bool isDynamic = false) {
            Table = table;
            Name = name;
            ArgumentId = argumentId;
            Dynamic = isDynamic;
        }

        public static Variable Create(VariableTable table, string name) => new Variable(table, name);
        public static Variable CreateArgument(VariableTable table, string name, int argumentId) => new Variable(table, name, argumentId: argumentId);
        public static Variable CreateDynamic(VariableTable table, string name) => new Variable(table, name, isDynamic: true);

        public VariableTable Table { get; }

        public string Name { get; }

        public bool IsArgument => ArgumentId >= 0;
        public int ArgumentId { get; }

        public bool Dynamic { get; }

        public bool RefOutsideScope { get; private set; } = false;

        public bool Assigned { get; private set; } = false;

        public LocalVariable? LocalVariable { get; set; }

        public VariablePlace Place { get; set; }

        public void DeterminePlace() {
            if (Dynamic) {
                Place = VariablePlace.DynamicScope;
            } else if (Table.IsRoot) {
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

        public void CreateLoadInstr(FunctionBuilder fgen) {
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
            case VariablePlace.DynamicScope:
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

        public void CreateStoreInstr(FunctionBuilder fgen) {
            switch (Place) {
            case VariablePlace.LocalVariable:
                fgen.AppendStLoc(LocalVariable!);
                break;
            case VariablePlace.Context:
                fgen.AppendLdStr(Name);
                fgen.AppendUvc();
                break;
            case VariablePlace.DynamicScope:
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

        public void MarkUsedIn(VariableTable scope) {
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

            return Parent != null && Parent.TryLookupVariable(name, out variable);
        }

        private const string PrivateVariablePrefix = "<pv>";

        private int _privateVariableCount = -1;

        public Variable CreatePrivateVariable(string? hint = null) {
            return CreateVariable(PrivateVariablePrefix + "_" + (hint ?? " ") + "_" + (_privateVariableCount += 1));
        }

        public Variable CreateVariable(string name) {
            if (_variables.TryGetValue(name, out var rv)) return rv;

            return _variables[name] = Variable.Create(this, name);
        }

        public Variable CreateVariable(string name, int argId) {
            if (_variables.TryGetValue(name, out var rv)) return rv;

            return _variables[name] = Variable.CreateArgument(this, name, argId);
        }

        public Variable CreateDynamicVariable(string name) {
            if (_variables.TryGetValue(name, out var rv)) return rv;

            return _variables[name] = Variable.CreateDynamic(this, name);
        }

        public void DetermineVariablePlace() {
            foreach (var variable in _variables.Values) variable.DeterminePlace();
        }
    }
}
