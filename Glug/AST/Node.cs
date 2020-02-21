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
        public virtual int VarRefOnStackListLength { get; } = 0;
    }

    public abstract class Literal : Expr { }

    public class LiteralInteger : Literal {
        public LiteralInteger(long value) {
            Value = value;
        }

        public long Value { get; }
    }     

    public class LiteralBool : Literal {
        public LiteralBool(bool value) {
            Value = value;
        }

        public bool Value { get; }
    }

    public class LiteralNil : Literal { }

    public class VarRef : Expr {
        public Variable Var { get; set; }
        public bool IsDef { get; set; }

        public override bool IsVarRef { get; } = true;
        public override int VarRefOnStackListLength { get; } = 1;

        public VarRef(string id) {
            Id = id;
        }

        public string Id { get; }
    }

    public struct IfBranch {
        public IfBranch(Expr condition, Expr expression) {
            Condition = condition;
            Expression = expression;
        }

        public Expr Condition;
        public Expr Expression;

        public void Deconstruct(out Expr condition, out Expr expression) => (condition, expression) = (Condition, Expression);
    }

    public class If : Expr {
        public override bool IsOnStackList { get; }

        public If(IList<IfBranch> branches, Expr? elseBranch) {
            Branches = branches;
            ElseBranch = elseBranch;

            IsOnStackList = Branches.All(x => x.Expression.IsOnStackList) && (ElseBranch?.IsOnStackList ?? true);
        }

        public IList<IfBranch> Branches { get; }
        public Expr? ElseBranch { get; }
    }

    public class While : Expr {
        public While(Expr condition, Expr expression) {
            Condition = condition;
            Expression = expression;
        }

        public Expr Condition { get; }
        public Expr Expression { get; }
    }

    public class Return : Expr {
        public Return(Expr expression) {
            Expression = expression;
        }

        public Expr Expression { get; }
    }
    
    public class Function : Expr {
        public VariableTable VariableTable { get; set; }

        public Variable Self { get; set; }

        public Function(string name, List<string> @params, Expr expression) {
            Name = name;
            Params = @params;
            Expression = expression;
        }

        public string Name { get; }
        public List<string> Params { get; }
        public Expr Expression { get; }
    }

    public class OnStackList : Expr {
        public override bool IsVarRef { get; }
        public override bool IsOnStackList { get; } = true;
        public override int VarRefOnStackListLength { get; }

        public OnStackList(List<Expr> list) {
            List = list;

            if (list.All(x => x is VarRef)) {
                IsVarRef = true;
                VarRefOnStackListLength = List.Count;
            } else {
                IsVarRef = false;
                VarRefOnStackListLength = 0;
            }
        }

        public List<Expr> List { get; }
    }

    public class Block : Expr {
        public override bool IsOnStackList { get; }

        public Block(IList<Expr> statements) {
            Statements = statements;

            IsOnStackList = Statements.Count > 0 && Statements[^1].IsOnStackList;
        }

        public IList<Expr> Statements { get; }
    }

    public class UnOp : Expr {
        public UnOp(GlugTokenType op, Expr expr) {
            Op = op;
            Expr = expr;
        }

        public GlugTokenType Op { get; }
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
        Assign
    }

    public class BiOp : Expr {
        public override bool IsOnStackList { get; }

        public BiOp(GlugBiOpType op, Expr exprL, Expr exprR) {
            Op = op;
            ExprL = exprL;
            ExprR = exprR;

            IsOnStackList = op == GlugBiOpType.Call;
        }

        public GlugBiOpType Op { get; }
        public Expr ExprL { get; }
        public Expr ExprR { get; }
    }
}
