namespace GeminiLab.Glug {
    public struct PositionInSource {
        public string Source;
        public int    Row;
        public int    Column;

        public override string ToString() {
            return $"{Source}:{Row}:{Column}";
        }

        public static PositionInSource NotAPosition() => new PositionInSource { Source = null!, Row = -1, Column = -1 };

        public bool IsNotAPosition() => Row < 0 && Column < 0;
    }
}
