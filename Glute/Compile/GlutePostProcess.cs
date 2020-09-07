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

            new BreakTargetVisitor(it).VisitNode(root, null);
            new NodeGenericInfoVisitor(it).VisitNode(root);
            new IsAssignableVisitor(it).VisitNode(root);

            var vdv = new VarDefVisitor(it);
            vdv.VisitNode(root, vdv.RootTable);

            var vcv = new GluteVarRefVisitor(vdv.RootTable, it);
            vcv.VisitNode(root, vdv.RootTable, false);

            vdv.DetermineVariablePlace();

            new VariableInContextMarker(it).VisitNode(root);

            var gen = new CodeGenVisitor(it);
            gen.VisitNode(root, null!, false);

            return gen.Builder.GetResult();
        }
    }
}
