using System;
using System.Drawing;

namespace TikzGraphGen
{
    public class EdgeLineStyle
    {
        public enum Concentration
        {
            Loose, Regular, Dense
        }
        public enum LineStyle
        {
            Solid, Dash, Dot, DotDash, DashDot, DashDotDot, None
        }
        public Color Color { get; private set; }
        public LineStyle Style { get; private set; }
        public Concentration Density { get; private set; }
        public float DashWidth { get; private set; }
        public float DashSpacing { get; private set; }
        public float PatternOffset { get; private set; }
        public float Thickness { get; private set; }
        public bool SDirected { get; private set; }
        public bool TDirected { get; private set; }

        public EdgeLineStyle()
        {
            Style = LineStyle.None;
            Color = Color.Transparent;
            Density = Concentration.Regular;
            DashWidth = 0;
            DashSpacing = 0;
            PatternOffset = 0;
            Thickness = 0;
            SDirected = false;
            TDirected = false;
        }
        public EdgeLineStyle(Color color, float thick)
        {
            Style = LineStyle.Solid;
            Color = color;
            Density = Concentration.Regular;
            DashWidth = 0;
            DashSpacing = 0;
            PatternOffset = 0;
            Thickness = thick;
            SDirected = false;
            TDirected = false;
        }
        public EdgeLineStyle(LineStyle style, Color color, Concentration density, float width, float spacing, float offset, float thick)
        {
            if (style.Equals(LineStyle.Solid) || style.Equals(LineStyle.None))
                throw new ArgumentException("Solid and blank lines must use their constructor.");
            Style = style;
            Color = color;
            Density = density;
            DashWidth = width;
            DashSpacing = spacing;
            PatternOffset = offset;
            Thickness = thick;
            SDirected = false;
            TDirected = false;
        }
        public EdgeLineStyle(LineStyle style, Color color, Concentration density, float width, float spacing, float offset, float thick, bool StoT, bool TtoS) : this(style, color, density, width, spacing, offset, thick)
        {
            Thickness = thick;
            SDirected = StoT;
            TDirected = TtoS;
        }
    }
}
