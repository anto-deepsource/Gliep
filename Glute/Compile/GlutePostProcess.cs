using System.Collections.Generic;
using GeminiLab.Core2.Collections;
using GeminiLab.Glos;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.PostProcess;

namespace GeminiLab.Glute.Compile {
    // todo: merge this class with GluteVarRefVisitor
    internal class VariableInContextMarker : RecursiveVisitor {
        public override void VisitFunction(Function val) {
            var table = Pass.NodeInformation<VariableAllocationInfo>(val).VariableTable;
            table.Variables.Values.ForEach(v => {
                if (v.Place != VariablePlace.DynamicScope) v.Place = VariablePlace.Context;
            });

            base.VisitFunction(val);
        }
    }

    public class GlutePostProcess {
        public static IGlosUnit PostProcessAndCodeGen(Expr root) {
            root = new Function("<root>", false, new List<string>(), root);

            var pass = new Pass();
            pass.AppendVisitor(new BreakTargetVisitor());
            pass.AppendVisitor(new NodeGenericInfoVisitor());
            pass.AppendVisitor(new VarDefVisitor());
            pass.AppendVisitor(new GluteVarRefVisitor());
            pass.AppendVisitor(new VariableInContextMarker());
            pass.AppendVisitor(new CodeGenVisitor());

            pass.Visit(root);

            return pass.GetVisitor<CodeGenVisitor>().Builder.GetResult();
        }
    }
}
