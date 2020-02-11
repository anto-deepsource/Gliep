using System;
using System.Collections.Generic;
using System.Linq;

using GeminiLab.Core2;
using GeminiLab.Core2.IO;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug.AST {
    public abstract class Node { }
    
    public abstract class Expr : Node {
        public bool IsVarRef { get; set; }
        public bool IsOnStackList { get; set; }
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
        
        public VarRef(string id) {
            Id = id;
        }

        public string Id { get; }
    }

    public struct IfBranch {
        public Expr Condition;
        public Expr Expression;

        public IfBranch(Expr condition, Expr expression) {
            Condition = condition;
            Expression = expression;
        }

        public void Deconstruct(out Expr condition, out Expr expression) => (condition, expression) = (Condition, Expression);
    }

    public class If : Expr {
        public IList<IfBranch> Branches { get; } = new List<IfBranch>();
        public Expr? ElseBranch { get; set; }
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
        public VariableTable Variables { get; set; }

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
        public OnStackList(List<Expr> list) {
            List = list;
        }

        public List<Expr> List { get; }
    }

    public class Block : Expr {
        public List<Expr> Statements { get; } = new List<Expr>();
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
        public BiOp(GlugBiOpType op, Expr exprL, Expr exprR) {
            Op = op;
            ExprL = exprL;
            ExprR = exprR;
        }

        public GlugBiOpType Op { get; }
        public Expr ExprL { get; }
        public Expr ExprR { get; }
    }
}
