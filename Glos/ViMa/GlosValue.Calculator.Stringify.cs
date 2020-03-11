using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

using GeminiLab.Core2.Text;

namespace GeminiLab.Glos.ViMa {
    public partial struct GlosValue {
        public partial class Calculator {
            [ExcludeFromCodeCoverage]
            public static string DebugStringify(in GlosValue v) {
                return v.Type switch {
                    GlosValueType.Nil => "nil",
                    GlosValueType.Integer => v.AssumeInteger().ToString(),
                    GlosValueType.Float => v.AssumeFloat().ToString(CultureInfo.InvariantCulture),
                    GlosValueType.Boolean => v.AssumeBoolean() ? "true" : "false",
                    GlosValueType.Table => $"<table: {RuntimeHelpers.GetHashCode(v.AssumeTable()):x8}>",
                    GlosValueType.String => $"\"{EscapeSequenceConverter.Encode(v.AssumeString())}\"",
                    GlosValueType.Function => $"<function \"{v.AssumeFunction().Prototype.Name} {(v.AssumeFunction().ParentContext is {} ctx ? $"at {RuntimeHelpers.GetHashCode(ctx):x8}" : "unbound")}\">",
                    GlosValueType.ExternalFunction => $"<external function {RuntimeHelpers.GetHashCode(v.AssumeExternalFunction()):x8}>",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            public string Stringify(in GlosValue v) {
                GlosValue temp = default;
                temp.SetNil();

                if (v.Type == GlosValueType.String) return v.AssumeString();

                if (v.Type == GlosValueType.Table && TryInvokeMetamethod(ref temp, v, _viMa, GlosMetamethodNames.Str)) {
                    return temp.Type == GlosValueType.String ? temp.AssumeString() : Stringify(temp);
                }

                return DebugStringify(v);
            }
        }
    }
}
