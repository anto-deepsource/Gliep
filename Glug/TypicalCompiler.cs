using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GeminiLab.Glos;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Parser;
using GeminiLab.Glug.PostProcess;
using GeminiLab.Glug.Tokenizer;

namespace GeminiLab.Glug {
    [ExcludeFromCodeCoverage]
    public static class TypicalCompiler {
        public static IGlugTokenStream Tokenize(TextReader input, string? sourceName = null) => new GlugTokenizer(input, sourceName);
        public static IGlugTokenStream Tokenize(string value, string? sourceName = null) => new GlugTokenizer(new StringReader(value), sourceName);

        public static Expr Parse(IGlugTokenStream stream) => new GlugParser(stream).Parse();

        public static IGlosUnit PostProcessAndCodeGen(Expr root) {
            root = new Function("<root>", false, new List<string>(), root);

            var pass = new Pass();
            pass.AppendVisitor(new BreakTargetVisitor());
            pass.AppendVisitor(new NodeGenericInfoVisitor());
            pass.AppendVisitor(new VarDefVisitor());
            pass.AppendVisitor(new VarRefVisitor());
            pass.AppendVisitor(new CodeGenVisitor());

            pass.Visit(root);
            
            return pass.GetVisitor<CodeGenVisitor>().Builder.GetResult();
        }

        public static IGlosUnit Compile(TextReader input, string? sourceName = null) {
            using var tok = Tokenize(input, sourceName);
            var ast = Parse(tok);
            return PostProcessAndCodeGen(ast);
        }

        public static IGlosUnit Compile(string code) {
            using var tok = Tokenize(code);
            var ast = Parse(tok);
            return PostProcessAndCodeGen(ast);
        }
    }
}
