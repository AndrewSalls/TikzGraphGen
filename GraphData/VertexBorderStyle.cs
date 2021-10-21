using System;
using System.Drawing;
using static TikzGraphGen.ToolSettingDictionary;

namespace TikzGraphGen
{
    public class VertexBorderStyle
    {
        public enum BorderStyle
        {
            Circle, CircleSplit, NoSign, Diamond, Cross, Strike, Rectangle, Ellipse, Polygon, Star, None
        }

        private VertexToolInfo _vertexInfo;
        public VertexToolInfo VertexInfo { get; private set; }
        
        public VertexBorderStyle(BorderStyle style, Color color, float thick, float rad)
        {
            if (style.Equals(BorderStyle.Polygon) || style.Equals(BorderStyle.Star) || style.Equals(BorderStyle.Rectangle) || style.Equals(BorderStyle.Ellipse))
                throw new ArgumentException("Polygons, stars, rectangles, and ellipses must use their constructor.");
            _vertexInfo.Style = style;
            _vertexInfo.BorderColor = color;
            _vertexInfo.Thickness = thick;
            _vertexInfo.Radius = rad;
            _vertexInfo.XRadius = 0;
            _vertexInfo.YRadius = 0;
            _vertexInfo.PolyCount = 0;
        }
        public VertexBorderStyle(BorderStyle style, Color color, float thick, float rad, int polys)
        {
            if (!style.Equals(BorderStyle.Polygon) || !style.Equals(BorderStyle.Star))
                throw new ArgumentException("Only polygon and star border styles can use the polygonal constructor.");
            _vertexInfo.Style = style;
            _vertexInfo.BorderColor = color;
            _vertexInfo.Thickness = thick;
            _vertexInfo.Radius = rad;
            _vertexInfo.PolyCount = polys;
            _vertexInfo.XRadius = 0;
            _vertexInfo.YRadius = 0;
        }
        public VertexBorderStyle(BorderStyle style, Color color, float thick, float width, float height)
        {
            if (!style.Equals(BorderStyle.Rectangle) || !style.Equals(BorderStyle.Ellipse))
                throw new ArgumentException("Only rectangle and ellipse border styles can use the non-regular constructor.");
            _vertexInfo.Style = style;
            _vertexInfo.BorderColor = color;
            _vertexInfo.Thickness = thick;
            _vertexInfo.Radius = 0;
            _vertexInfo.PolyCount = 0;
            _vertexInfo.XRadius = width;
            _vertexInfo.YRadius = height;
        }
    }
}