using System;
using System.Collections.Generic;
using System.IO;
using GeminiLab.Core2.IO;

using GeminiLab.Glos.ViMa;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Parser;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug {
    public static class TypicalCompiler {
        public static IGlugTokenStream Tokenize(string value) => new GlugTokenizer(new StringReader(value));

        public static Expr Parse(IGlugTokenStream stream) => GlugParser.Parse(stream);

        public static void ProcessTree(ref Expr root) {
            root = new Function("<root>", new List<string>(), root);

            var vdv = new FunctionAndVarDefVisitor();
            vdv.Visit(root);

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
            var ast = Parse(Tokenize(code));
            ProcessTree(ref ast);
            return CodeGen(ast);
        }
    }
}
