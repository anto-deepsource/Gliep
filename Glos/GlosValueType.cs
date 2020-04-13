using System;

namespace GeminiLab.Glos {
    public enum GlosValueType : byte {
        Nil = 0x00,
        Integer = 0x01,
        Float = 0x02,
        Boolean = 0x03,
        // Delimiter = 0x05,

        Table = 0x80,
        String = 0x81,
        Function = 0x82,
        // Userdata = 0x83,
        ExternalFunction = 0x84,
    }

    public static class GlosValueTypeExtensions {
        public static string GetName(this GlosValueType type) => type switch {
            GlosValueType.Nil => "nil",
            GlosValueType.Integer => "integer",
            GlosValueType.Float => "float",
            GlosValueType.Boolean => "boolean",
            GlosValueType.Table => "table",
            GlosValueType.String => "string",
            GlosValueType.Function => "function",
            GlosValueType.ExternalFunction => "function",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
