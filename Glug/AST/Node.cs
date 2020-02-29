using System;
using System.Collections.Generic;
using System.Linq;

using GeminiLab.Core2;
using GeminiLab.Core2.IO;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.AST {
    public abstract class Node { }
    
    public abstract class Expr : Node {
        public virtual bool IsVarRef { get; } = false;
        public virtual bool IsOnStackList { get; } = false;
    }

    public abstract class Literal : Expr { }

    public class LiteralInteger : Literal {
        public LiteralInteger(long value) {
            Value = value;
        }

        public long Value { get; }
    }

    public class LiteralString : Literal {
        public LiteralString(string value) {
            Value = value;
        }

        public string Value { get; }
    }

    public class LiteralBool : Literal {
        public LiteralBool(bool value) {
            Value = value;
        }

        public bool Value { get; }
    }

    public class LiteralNil : Literal { }

    public class VarRef : Expr {
        public Variable Variable { get; set; }
        public bool IsDef { get; set; }
        public bool IsGlobal { get; set; }

        public override bool IsVarRef { get; } = true;

        public VarRef(string id) {
            Id = id;
        }

        public string Id { get; }
    }

    public struct IfBranch {
        public IfBranch(Expr condition, Expr body) {
            Condition = condition;
            Body = body;
        }

        public Expr Condition;
        public Expr Body;

        public void Deconstruct(out Expr condition, out Expr body) => (condition, body) = (Condition, Body);
    }

    public class If : Expr {
        public override bool IsOnStackList { get; }

        public If(IList<IfBranch> branches, Expr? elseBranch) {
            Branches = branches;
            ElseBranch = elseBranch;

            IsOnStackList = Branches.Any(x => x.Body.IsOnStackList) || (ElseBranch?.IsOnStackList ?? false);
        }

        public IList<IfBranch> Branches { get; }
        public Expr? ElseBranch { get; }
    }

    public class While : Expr {
        public While(Expr condition, Expr body) {
            Condition = condition;
            Body = body;
        }

        public Expr Condition { get; }
        public Expr Body { get; }
    }

    public class Return : Expr {
        public Return(Expr expr) {
            Expr = expr;
        }

        public Expr Expr { get; }
    }
    
    public class Function : Expr {
        public VariableTable VariableTable { get; set; }

        public Variable Self { get; set; }

        public Function(string? name, List<string> parameters, Expr body) {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public string? Name { get; }
        public List<string> Parameters { get; }
        public Expr Body { get; }
    }

    public class OnStackList : Expr {
        public override bool IsVarRef { get; }
        public override bool IsOnStackList { get; } = true;

        public OnStackList(List<Expr> list) {
            List = list;

            IsVarRef = list.All(x => x is VarRef);
        }

        public List<Expr> List { get; }
    }

    public class Block : Expr {
        public override bool IsOnStackList { get; }

        public Block(IList<Expr> list) {
            List = list;

            IsOnStackList = List.Count > 0 && List[^1].IsOnStackList;
        }

        public IList<Expr> List { get; }
    }

    public enum GlugUnOpType {
        Not,
        Neg,
    }

    public class UnOp : Expr {
        public UnOp(GlugUnOpType op, Expr expr) {
            Op = op;
            Expr = expr;
        }

        public GlugUnOpType Op { get; }
        public Expr Expr { get; }
    }

    public enum GlugBiOpType {
        Add,
        Sub,
        Mul,
        Div,
        Mod,
        Lsh,
        Rsh,
        And,
        Orr,
        Xor,
        Gtr,
        Lss,
        Geq,
        Leq,
        Equ,
        Neq,
        Call,
        Assign,
        Index,
    }

    public class BiOp : Expr {
        public override bool IsOnStackList { get; }
        public override bool IsVarRef { get; }

        public BiOp(GlugBiOpType op, Expr exprL, Expr exprR) {
            Op = op;
            ExprL = exprL;
            ExprR = exprR;

            IsOnStackList = op == GlugBiOpType.Call;
            IsVarRef = op == GlugBiOpType.Index;
        }

        public GlugBiOpType Op { get; }
        public Expr ExprL { get; }
        public Expr ExprR { get; }
    }

    public struct TableDefPair {
        public TableDefPair(Expr key, Expr value) {
            Key = key;
            Value = value;
        }

        public Expr Key;
        public Expr Value;

        public void Deconstruct(out Expr key, out Expr value) => (key, value) = (Key, Value);
    } 

    public class TableDef : Expr {
        public TableDef(IList<TableDefPair> pairs) {
            Pairs = pairs;
        }

        public IList<TableDefPair> Pairs { get; }
    }
}
