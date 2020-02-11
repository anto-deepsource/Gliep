namespace GeminiLab.Glos.ViMa {
    public enum GlosValueType : byte {
        Nil = 0x00,
        Integer = 0x01,
        Float = 0x02,
        Boolean = 0x03,
        Delimiter = 0x05,

        Table = 0x80,
        String = 0x81,
        Function = 0x82,
        // Userdata = 0x83,
        ExternalFunction = 0x84,
    }
}
