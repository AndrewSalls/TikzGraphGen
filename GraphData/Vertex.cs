using System;
using System.Collections.Generic;
using System.Linq;
using TikzGraphGen.GraphData;
using static TikzGraphGen.ToolSettingDictionary;

namespace TikzGraphGen
{
    public class Vertex
    {
        public enum RelativePositioning
        {
            Above, Below, Left, Right
        }

        public VertexToolInfo Style { get; private set; }
        public string Label { get; private set; }
        public double Value { get; private set; }
        public Coord Offset { get; set; }
        protected HashSet<Edge> _incident;

        public Vertex(VertexToolInfo settings, Coord offset)
        {
            Style = settings;
            Label = offset.ToString();
            Value = 0;
            _incident = new HashSet<Edge>();
            Offset = offset;
        }
        public Vertex(VertexToolInfo s, string name, Coord offset)
        {
            Style = s;
            Label = name;
            Value = 0;
            Offset = offset;
        }
        public Vertex(VertexToolInfo s, string name, Coord offset, IEnumerable<Edge> i) : this(s, name, offset)
        {
            _incident = new HashSet<Edge>(i);
        }

        public void Connect(Edge inc)
        {
            _incident.Add(inc);
        }
        public void Disconnect(Edge inc)
        {
            _incident.Remove(inc);
        }

        public bool IsIncidentTo(Edge e)
        {
            return _incident.Contains(e);
        }
        public bool IsAdjacentTo(Vertex v)
        {
            return v != this && _incident.Any(e => e.IsIncidentTo(v));
        }

        public List<Edge> ViewEdges()
        {
            return _incident.ToList();
        }

        public float GetAngularRadius(float angle)
        {
            switch(Style.Style)
            {
                case VertexBorderStyle.BorderStyle.Circle:
                case VertexBorderStyle.BorderStyle.CircleSplit:
                case VertexBorderStyle.BorderStyle.NoSign:
                    return Style.Radius;
                case VertexBorderStyle.BorderStyle.Ellipse:
                    return Style.XRadius * Style.YRadius / MathF.Sqrt(Style.XRadius*Style.XRadius * MathF.Sin(angle) * MathF.Sin(angle) + Style.YRadius*Style.YRadius * MathF.Cos(angle)*MathF.Cos(angle));
                case VertexBorderStyle.BorderStyle.None:
                    return 0;
                default:
                    throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return $"({Label} | {Offset.X}, {Offset.Y})";
        }
    }
}
