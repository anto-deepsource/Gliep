using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using GeminiLab.Core2.Text;

namespace GeminiLab.Glos {
    public partial struct GlosValue {
        public partial class Calculator {
            [ExcludeFromCodeCoverage]
            public static string DebugStringify(in GlosValue v) {
                return v.Type switch {
                    GlosValueType.Nil            => "nil",
                    GlosValueType.Integer        => v.AssumeInteger().ToString(),
                    GlosValueType.Float          => v.AssumeFloat().ToString(CultureInfo.InvariantCulture),
                    GlosValueType.Boolean        => v.AssumeBoolean() ? "true" : "false",
                    GlosValueType.Table          => $"<table: {RuntimeHelpers.GetHashCode(v.AssumeTable()):x8}>",
                    GlosValueType.String         => $"\"{EscapeSequenceConverter.Encode(v.AssumeString())}\"",
                    GlosValueType.Function       => $"<function \"{v.AssumeFunction().Prototype.Name} {(v.AssumeFunction().ParentContext is { } ctx ? $"at {RuntimeHelpers.GetHashCode(ctx):x8}" : "unbound")}\">",
                    GlosValueType.EFunction      => $"<efunction: {RuntimeHelpers.GetHashCode(v.AssumeEFunction()):x8}>",
                    GlosValueType.Vector         => $"<vector: {RuntimeHelpers.GetHashCode(v.AssumeVector()):x8}>",
                    GlosValueType.PureEFunction  => $"<pure efunction: {RuntimeHelpers.GetHashCode(v.AssumePureEFunction()):x8}>",
                    GlosValueType.AsyncEFunction => $"<async efunction: {RuntimeHelpers.GetHashCode(v.AssumeAsyncEFunction()):x8}>",
                    GlosValueType.Coroutine      => $"<coroutine: {RuntimeHelpers.GetHashCode(v.AssumeCoroutine()):x8}>", // TODO: better stringify
                    GlosValueType.Exception      => $"<exception: {v.AssertException().Message}>",
                    _                            => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}
