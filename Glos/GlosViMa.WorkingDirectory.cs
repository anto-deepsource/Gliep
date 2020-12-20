namespace GeminiLab.Glos {
    public partial class GlosViMa {
        public string WorkingDirectory { get; set; }

        public IGlosUnit? CurrentExecutingUnit {
            get {
                var cor = CurrentCoroutine;

                if (cor == null) {
                    return null;
                }
                
                var cs = cor.CallStackFrames;

                // never supposed to happen
                if (cs.Length <= 0) {
                    return null;
                }

                return cs[^1].Function.Unit;
            }
        }
    }
}
