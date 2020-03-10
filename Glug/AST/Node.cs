using System.Collections.Generic;
using System.Linq;
using GeminiLab.Glos.CodeGenerator;

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
        public Variable Variable { get; set; } = null!;
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
        public override bool IsOnStackList => Body.IsOnStackList || Breaks.Any(b => b.Expr.IsOnStackList);

        public While(Expr condition, Expr body) {
            Condition = condition;
            Body = body;
        }

        public Expr Condition { get; }
        public Expr Body { get; }
        // CAUTION:
        // All properties of all node classes are readonly, except While.Breaks and Break.Parent
        // because these properties cannot be calculated by current parser (stateless, static, lock-free)
        // these properties should be assigned by only WhileVisitor
        public IList<Break> Breaks { get; set; } = new List<Break>();

        public Label EndLabel { get; set; } = null!;
    }

    public class Return : Expr {
        public Return(Expr expr) {
            Expr = expr;
        }

        public Expr Expr { get; }
    }

    public class Break : Expr {
        public Break(Expr expr) {
            Expr = expr;
        }

        public Expr Expr { get; }
        public While Parent { get; set; } = null!;
    }

    public class Function : Expr {
        public VariableTable VariableTable { get; set; } = null!;

        public Variable Self { get; set; } = null!;

        public Function(string name, bool explicitlyNamed, List<string> parameters, Expr body) {
            Name = name;
            ExplicitlyNamed = explicitlyNamed;
            Parameters = parameters;
            Body = body;
        }

        public string Name { get; }
        public bool ExplicitlyNamed { get; }
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
        Typeof,
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
        Concat,
    }

    public class BiOp : Expr {
        public override bool IsOnStackList { get; }
        public override bool IsVarRef { get; }

        public BiOp(GlugBiOpType op, Expr exprL, Expr exprR) {
            Op = op;
            ExprL = exprL;
            ExprR = exprR;

            IsOnStackList = op == GlugBiOpType.Call || op == GlugBiOpType.Concat;
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

    public class Metatable : Expr {
        public override bool IsVarRef { get; } = true;

        public Metatable(Expr table) {
            Table = table;
        }

        public Expr Table;
    }
}
