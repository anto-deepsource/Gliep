using System.Collections.Generic;
using GeminiLab.Core2.Collections;
using GeminiLab.Glos;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.PostProcess;

namespace GeminiLab.Glute.Compile {
    internal class VariableInContextMarker : RecursiveVisitor {
        private readonly NodeInformation _info;

        public VariableInContextMarker(NodeInformation info) {
            _info = info;
        }

        public override void VisitFunction(Function val) {
            var table = _info.VariableTable[val];
            table.Variables.Values.ForEach(v => {
                if (v.Place != VariablePlace.DynamicScope) v.Place = VariablePlace.Context;
            });

            base.VisitFunction(val);
        }
    }

    public class GlutePostProcess {
        public static GlosUnit PostProcessAndCodeGen(Expr root) {
            root = new Function("<root>", false, new List<string>(), root);

            var it = new NodeInformation();

            new WhileBreakPairingVisitor(it).Visit(root, null);
            new IsOnStackListVisitor(it).Visit(root);
            new IsAssignableVisitor(it).Visit(root);

            var vdv = new VarDefVisitor(it);
            vdv.Visit(root, vdv.RootTable);

            var vcv = new GluteVarRefVisitor(vdv.RootTable, it);
            vcv.Visit(root, new VarRefVisitorContext(vdv.RootTable, false));

            vdv.DetermineVariablePlace();

            new VariableInContextMarker(it).Visit(root);

            var gen = new CodeGenVisitor(it);
            gen.Visit(root, new CodeGenContext(null!, false));

            return gen.Builder.GetResult();
        }
    }
}
