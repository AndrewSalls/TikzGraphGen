using System.Collections.Generic;
using System.Linq;
using TikzGraphGen.GraphData;

namespace TikzGraphGen
{
    public class Vertex
    {
        public enum RelativePositioning
        {
            Above, Below, Left, Right
        }

        public VertexBorderStyle Style { get; private set; }
        public string Label { get; private set; }
        public double Value { get; private set; }
        public Coord Offset { get; set; }
        protected HashSet<Edge> _incident;

        public Vertex(GraphInfo settings, Coord offset)
        {
            Style = settings.defaultBorders;
            Label = offset.ToString();
            Value = 0;
            _incident = new HashSet<Edge>();
            Offset = offset;
        }
        public Vertex(VertexBorderStyle s, string name, Coord offset)
        {
            Style = s;
            Label = name;
            Value = 0;
            Offset = offset;
        }
        public Vertex(VertexBorderStyle s, string name, Coord offset, IEnumerable<Edge> i) : this(s, name, offset)
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

        public override string ToString()
        {
            return $"({Label} | {Offset.X}, {Offset.Y})";
        }
    }
}
