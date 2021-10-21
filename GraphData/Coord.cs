using System;

namespace TikzGraphGen.GraphData
{
    public class Coord
    {
        public static readonly float MAX_ERROR = 0.000001f;

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

        public static float DistanceFrom(Coord a, Coord b) => MathF.Sqrt(MathF.Pow(a.Y - b.Y, 2) + MathF.Pow(a.X - b.X, 2));
        public static float AngleBetween(Coord a, Coord b)
        {
            float output = MathF.Atan2(a.Y - b.Y, a.X - b.X);
            if (output < 0) //Changes bounds from (-PI, PI) to (0, 2 PI)
                output = 2 * MathF.PI + output;
            
            return output;
        }

        public override string ToString() => $"({X}, {Y})";
        public override bool Equals(object obj) => (obj is Coord c) && c.X == X && c.Y == Y;
        public bool Equalish(object obj) => (obj is Coord c) && MathF.Abs(c.X - X) < MAX_ERROR && MathF.Abs(c.Y - Y) < MAX_ERROR;

        public override int GetHashCode() => base.GetHashCode();
    }
}
