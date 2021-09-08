using System;
using System.Drawing;

namespace TikzGraphGen
{
    public class VertexBorderStyle
    {
        public enum BorderStyle
        {
            Circle, CircleSplit, NoSign, Diamond, Cross, Strike, Rectangle, Ellipse, Polygon, Star, None
        }

        public BorderStyle Style { get; private set; }
        public Color BorderColor { get; private set; }
        public float Thickness { get; private set; }
        public float OblongWidth { get; private set; }
        public float OblongHeight { get; private set; }
        public int PolyCount { get; private set; }
        public float Radius { get; private set; }
        
        public VertexBorderStyle(BorderStyle style, Color color, float thick, float rad)
        {
            if (style.Equals(BorderStyle.Polygon) || style.Equals(BorderStyle.Star) || style.Equals(BorderStyle.Rectangle) || style.Equals(BorderStyle.Ellipse))
                throw new ArgumentException("Polygons, stars, rectangles, and ellipses must use their constructor.");
            Style = style;
            BorderColor = color;
            Thickness = thick;
            Radius = rad;
            OblongWidth = 0;
            OblongHeight = 0;
            PolyCount = 0;
        }
        public VertexBorderStyle(BorderStyle style, Color color, float thick, float rad, int polys)
        {
            if (!style.Equals(BorderStyle.Polygon) || !style.Equals(BorderStyle.Star))
                throw new ArgumentException("Only polygon and star border styles can use the polygonal constructor.");
            Style = style;
            BorderColor = color;
            Thickness = thick;
            Radius = rad;
            PolyCount = polys;
            OblongWidth = 0;
            OblongHeight = 0;
        }
        public VertexBorderStyle(BorderStyle style, Color color, float thick, float width, float height)
        {
            if (!style.Equals(BorderStyle.Rectangle) || !style.Equals(BorderStyle.Ellipse))
                throw new ArgumentException("Only rectangle and ellipse border styles can use the non-regular constructor.");
            Style = style;
            BorderColor = color;
            Thickness = thick;
            Radius = 0;
            PolyCount = 0;
            OblongWidth = width;
            OblongHeight = height;
        }
    }
}