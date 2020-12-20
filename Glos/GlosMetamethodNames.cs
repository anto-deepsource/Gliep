using System;

namespace GeminiLab.Glos {
    public static class GlosMetamethodNames {
        public static readonly string Add = "__add";
        public static readonly string Sub = "__sub";
        public static readonly string Mul = "__mul";
        public static readonly string Div = "__div";
        public static readonly string Mod = "__mod";

        public static readonly string Lsh = "__lsh";
        public static readonly string Rsh = "__rsh";
        public static readonly string And = "__and";
        public static readonly string Orr = "__orr";
        public static readonly string Xor = "__xor";

        public static readonly string Lss = "__lss";
        public static readonly string Equ = "__equ";

        public static readonly string Ren = "__ren";
        public static readonly string Uen = "__uen";

        public static readonly string Not = "__not";
        public static readonly string Neg = "__neg";
        public static readonly string Str = "__str";

        public static readonly string Hash = "__hash";

        public static string FromOp(GlosOp op) {
            return op switch {
                GlosOp.Add => Add,
                GlosOp.Sub => Sub,
                GlosOp.Mul => Mul,
                GlosOp.Div => Div,
                GlosOp.Mod => Mod,
                GlosOp.Lsh => Lsh,
                GlosOp.Rsh => Rsh,
                GlosOp.And => And,
                GlosOp.Orr => Orr,
                GlosOp.Xor => Xor,
                GlosOp.Lss => Lss,
                GlosOp.Equ => Equ,
                GlosOp.Ren => Ren,
                GlosOp.Uen => Uen,
                GlosOp.Not => Not,
                GlosOp.Neg => Neg,
                _          => "",
            };
        }
    }
}
