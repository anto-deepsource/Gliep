namespace GeminiLab.Glos {
    public enum GlosOp : byte {
        Add     = 0x00,
        Sub     = 0x01,
        Mul     = 0x02,
        Div     = 0x03,
        Mod     = 0x04,
        Lsh     = 0x05,
        Rsh     = 0x06,
        And     = 0x07,
        Orr     = 0x08,
        Xor     = 0x09,
        Gtr     = 0x0a,
        Lss     = 0x0b,
        Geq     = 0x0c,
        Leq     = 0x0d,
        Equ     = 0x0e,
        Neq     = 0x0f,

        // Uen/UenL:
        // push value
        // push table
        // push key
        // uen/uen.L
        Smt     = 0x10,
        Gmt     = 0x11,
        Ren     = 0x12,
        Uen     = 0x13,
        RenL    = 0x14,
        UenL    = 0x15,
        Ien     = 0x16,

        Pshv    = 0x18,
        Popv    = 0x19,
        
        Not     = 0x1c,
        Neg     = 0x1d,
        Typeof  = 0x1e,
        IsNil   = 0x1f,

        // Uvc/Uvg:
        // push value
        // push key
        // uvc/uvg
        Rvc     = 0x20,
        Uvc     = 0x21,
        Rvg     = 0x22,
        Uvg     = 0x23,

        LdFun   = 0x28,
        LdStr   = 0x29,

        LdFunS  = 0x2c,
        LdStrS  = 0x2d,

        Ld0     = 0x30,
        Ld1     = 0x31,
        Ld2     = 0x32,
        Ld3     = 0x33,
        LdQ     = 0x34,
        Ld      = 0x35,
        LdS     = 0x36,
        LdNeg1  = 0x37,

        LdNTbl  = 0x38,
        LdNil   = 0x39,
        LdFlt   = 0x3a,
        LdNVec  = 0x3b,

        LdTrue  = 0x3c,
        LdFalse = 0x3d,

        LdLoc0  = 0x40,
        LdLoc1  = 0x41,
        LdLoc2  = 0x42,
        LdLoc3  = 0x43,
        LdLoc4  = 0x44,
        LdLoc5  = 0x45,
        LdLoc6  = 0x46,
        LdLoc7  = 0x47,
        StLoc0  = 0x48,
        StLoc1  = 0x49,
        StLoc2  = 0x4a,
        StLoc3  = 0x4b,
        StLoc4  = 0x4c,
        StLoc5  = 0x4d,
        StLoc6  = 0x4e,
        StLoc7  = 0x4f,

        LdArg0  = 0x50,
        LdArg1  = 0x51,
        LdArg2  = 0x52,
        LdArg3  = 0x53,

        LdArg   = 0x54,
        LdLoc   = 0x55,
        StLoc   = 0x56,
        LdArgc  = 0x57,
        LdArgS  = 0x58,
        LdLocS  = 0x59,
        StLocS  = 0x5a,

        LdArgA  = 0x5c,
        LdLocA  = 0x5d,
        StLocA  = 0x5e,

        B       = 0x80,
        Bf      = 0x81,
        Bt      = 0x82,
        Bn      = 0x83,
        Bnn     = 0x84,

        BS      = 0x88,
        BfS     = 0x89,
        BtS     = 0x8a,
        BnS     = 0x8b,
        BnnS    = 0x8c,

        Try     = 0x90,
        TryS    = 0x91,
        EndTry  = 0x92,
        Throw   = 0x93,

        Dup     = 0xa0,
        Pop     = 0xa1,

        Mkc     = 0xa5,
        Yield   = 0xa6,
        Resume  = 0xa7,

        LdDel   = 0xa8,
        Call    = 0xa9,
        Ret     = 0xaa,
        Bind    = 0xab,
        PopDel  = 0xac,
        DupList = 0xad,
        Pkv     = 0xae,
        Upv     = 0xaf,

        ShpRv0  = 0xb0,
        ShpRv1  = 0xb1,
        ShpRv2  = 0xb2,
        ShpRv3  = 0xb3,
        ShpRv   = 0xb4,
        ShpRvS  = 0xb5,

        Nop     = 0xc0,
        
        SysC0   = 0xe0,
        SysC1   = 0xe1,
        SysC2   = 0xe2,
        SysC3   = 0xe3,
        SysC4   = 0xe4,
        SysC5   = 0xe5,
        SysC6   = 0xe6,
        SysC7   = 0xe7,

        Invalid = 0xff,
    }
}
