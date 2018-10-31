namespace Play
{
    public class MatchData
    {
        public enum MatchDirection
        {
            Left,
            Right,
            Top,
            Bottom
        }

        public readonly MatchDirection Direction;
        public readonly int Length;
        public readonly int X;
        public readonly int Y;

        public MatchData(MatchDirection dir, int length, int x, int y)
        {
            Direction = dir;
            Length = length;
            X = x;
            Y = y;
        }
    }
}