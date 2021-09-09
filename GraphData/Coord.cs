namespace TikzGraphGen.GraphData
{
    public class Coord
    {
        public Coord()
        {
            X = 0;
            Y = 0;
        }
        public Coord(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }

        public static Coord operator +(Coord c1, Coord c2) => new(c1.X + c2.X, c1.Y + c2.Y);
        public static Coord operator -(Coord c1, Coord c2) => new(c1.X - c2.X, c1.Y - c2.Y);
        public static Coord operator *(Coord c, float f) => new(c.X * f, c.Y * f);

        public static implicit operator Coord((float x, float y) c) => new(c.x, c.y);
        public static implicit operator Coord((double x, double y) c) => new((float)c.x, (float)c.y);
        public static implicit operator Coord(System.Drawing.Point p) => new(p.X, p.Y);

        public override string ToString() => $"({X}, {Y})";
    }
}
