using System.Collections.Generic;
using System.IO;

using GeminiLab.Glos.ViMa;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Parser;
using GeminiLab.Glug.Tokenizer;

namespace XUnitTester.Glug {
    public class GlugExecutionTestBase {
        internal GlosViMa ViMa { get; } = new GlosViMa();

        public GlosValue[] Execute(string source, GlosContext? context = null) {
            using var tok = new GlugTokenizer(new StringReader(source));
            var rootFun = new Function("<root>", new List<string>(), GlugParser.Parse(tok));

            var vdv = new FunctionAndVarDefVisitor();
            vdv.Visit(rootFun);

            var vcv = new VarRefVisitor(vdv.RootTable);
            vcv.Visit(rootFun);

            vdv.DetermineVariablePlace();

            var gen = new CodeGenVisitor();
            gen.Visit(rootFun);

            var unit = gen.Builder.GetResult();

            return ViMa.ExecuteUnit(unit, null, context ?? new GlosContext(null!));
        }
    }
}
