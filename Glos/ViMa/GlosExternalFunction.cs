namespace GeminiLab.Glos.ViMa {
    // TODO: answer following questions:
    // Question I: should external functions receive a vm as an argument?
    // Question II: how to avoid copy at calling
    //  - for arguments, ReadonlySpan is an option
    //  - for return values?
    public delegate GlosValue[] GlosExternalFunction(GlosValue[] arg);
}
