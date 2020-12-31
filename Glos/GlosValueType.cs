using System;

namespace GeminiLab.Glos {
    public enum GlosValueType : byte {
        Nil     = 0x00,
        Integer = 0x01,
        Float   = 0x02,
        Boolean = 0x03,

        Table          = 0x80,
        String         = 0x81,
        Function       = 0x82,
        Userdata       = 0x83, // NOT used yet
        EFunction      = 0x84,
        Vector         = 0x85,
        PureEFunction  = 0x86,
        AsyncEFunction = 0x87,
        Coroutine      = 0x88,
    }

    public static class GlosValueTypeExtensions {
        public static string GetName(this GlosValueType type) =>
            type switch {
                GlosValueType.Nil            => "nil",
                GlosValueType.Integer        => "integer",
                GlosValueType.Float          => "float",
                GlosValueType.Boolean        => "boolean",
                GlosValueType.Table          => "table",
                GlosValueType.String         => "string",
                GlosValueType.Function       => "function",
                GlosValueType.EFunction      => "function",
                GlosValueType.Vector         => "vector",
                GlosValueType.PureEFunction  => "function",
                GlosValueType.AsyncEFunction => "function",
                GlosValueType.Coroutine      => "coroutine",
                _                            => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
    }
}
