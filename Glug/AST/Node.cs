using System.Collections.Generic;

namespace GeminiLab.Glug.AST {
    public abstract class Node { }
    
    public abstract class Expr : Node { }

    public abstract class Literal : Expr { }

    public class LiteralInteger : Literal {
        public LiteralInteger(long value) {
            Value = value;
        }

        public long Value { get; }
    }

    public class LiteralFloat : Literal {
        public unsafe LiteralFloat(long b) : this(*(double*)&b) { }

        public LiteralFloat(double value) {
            Value = value;
        }

        public double Value { get; }
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
        public VarRef(string id, bool isDef = false, bool isGlobal = false) {
            Id = id;
            IsDef = isDef;
            IsGlobal = isGlobal;
        }

        public string Id { get; }
        public bool IsDef { get; }
        public bool IsGlobal { get; }
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
        public If(IList<IfBranch> branches, Expr? elseBranch) {
            Branches = branches;
            ElseBranch = elseBranch;
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

    public class Break : Expr {
        public Break(Expr expr) {
            Expr = expr;
        }

        public Expr Expr { get; }
    }

    public class Function : Expr {
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
        public OnStackList(IList<Expr> list) {
            List = list;
        }

        public IList<Expr> List { get; }
    }

    public class Block : Expr {
        public Block(IList<Expr> list) {
            List = list;
        }

        public IList<Expr> List { get; }
    }

    public enum GlugUnOpType {
        Not,
        Neg,
        Typeof,
        IsNil,
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
        IndexLocal,
        Concat,
        ShortCircuitAnd,
        ShortCircuitOrr,
        NullCoalescing,
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

        public static bool IsShortCircuitOp(GlugBiOpType type) {
            return type == GlugBiOpType.ShortCircuitAnd || 
                type == GlugBiOpType.ShortCircuitOrr ||
                type == GlugBiOpType.NullCoalescing;
        }
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
        public Metatable(Expr table) {
            Table = table;
        }

        public Expr Table { get; }
    }

    // Following are node types which are not used directly by Glug itself but critical for tools based on Glug.

    // A basic principle of Glug is "NOTHING BUT EXPRESSIONS" but syscalls somehow break this rule.
    // Stack operations of all types of instructions (except for SysCall instructions) are DEFINITE, which means
    // they eats and then puts a DEFINITE number of values or OSLs on stack. However, stack operations of SysCall
    // instructions are INDEFINITE. So class SysCall requires extra information.
    public class SysCall : Expr {
        public SysCall(int id, IList<Expr> inputs, ResultType rt) {
            Id = id;
            Inputs = inputs;
            Result = rt;
        }

        public int Id { get; }

        public IList<Expr> Inputs { get; }

        public enum ResultType {
            Value,
            Osl,
            None,
        }

        public ResultType Result { get; }
    }

    // explicitly convert Child to value
    public class ToValue : Expr {
        public ToValue(Expr child) {
            Child = child;
        }
        
        public Expr Child { get; }
    }

    public class PseudoIndex : Expr {
        public PseudoIndex(bool isTail) {
            IsTail = isTail;
        }
        public bool IsTail { get; }
    }
}
