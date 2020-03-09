using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using GeminiLab.Glos.ViMa;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Parser;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug {
    [ExcludeFromCodeCoverage]
    public static class TypicalCompiler {
        public static IGlugTokenStream Tokenize(string value) => new GlugTokenizer(new StringReader(value));

        public static Expr Parse(IGlugTokenStream stream) => GlugParser.Parse(stream);

        public static void ProcessTree(ref Expr root) {
            root = new Function("<root>", false, new List<string>(), root);

            var wbv = new WhileVisitor();
            wbv.Visit(root, null);

            var vdv = new VarDefVisitor();
            vdv.Visit(root, vdv.RootTable);

            var vcv = new VarRefVisitor(vdv.RootTable);
            vcv.Visit(root);

            vdv.DetermineVariablePlace();

        }

        public static GlosUnit CodeGen(Expr root) {
            var gen = new CodeGenVisitor();
            gen.Visit(root);

            return gen.Builder.GetResult();
        }

        public static GlosUnit Compile(string code) {
            using var tok = Tokenize(code);
            var ast = Parse(tok);
            ProcessTree(ref ast);
            return CodeGen(ast);
        }
    }
}
