using System;
using TikzGraphGen.GraphData;
using static TikzGraphGen.ToolSettingDictionary;

namespace TikzGraphGen
{
    public class Edge
    {
        public EdgeLineStyle Style { get; private set; }
        public FreeLabel Label { get; private set; }
        public double Value { get; private set; }
        protected Vertex _source, _destination;

        public Edge(EdgeToolInfo edgeSettings, EdgeCapToolInfo capSettings)
        {
            Style = new EdgeLineStyle(edgeSettings, capSettings, capSettings);
            Label = null;
            Value = 0;
            _source = null;
            _destination = null;
        }
        public Edge(EdgeToolInfo edgeSettings, EdgeCapToolInfo capSettings, Vertex s, Vertex d)
        {
            Style = new EdgeLineStyle(edgeSettings, capSettings, capSettings);
            Label = null;
            Value = 0;
            _source = s;
            _destination = d;
        }
        public Edge(EdgeToolInfo edgeSettings, EdgeCapToolInfo capSettings, Vertex s, Vertex d, FreeLabel label)
        {
            Style = new EdgeLineStyle(edgeSettings, capSettings, capSettings);
            Label = label;
            Value = 0;
            _source = s;
            _destination = d;
        }

        public void Connect(Vertex s, Vertex d)
        {
            _source = s;
            _destination = d;
        }
        public void Disconnect()
        {
            _source.Disconnect(this);
            _destination.Disconnect(this);
        }

        public bool IsIncidentTo(Vertex v)
        {
            return _source == v || _destination == v;
        }
        public bool IsAdjacentTo(Edge e)
        {
            return e != this && _source.IsIncidentTo(e) || _destination.IsIncidentTo(e);
        }

        public Vertex ViewSource()
        {
            return _source;
        }
        public Vertex ViewDestination()
        {
            return _destination;
        }

        public Coord GetSourceOffset()
        {
            float angle = Coord.AngleBetween(_source.Offset, _destination.Offset);
            float startAngularRadius = _source.GetAngularRadius(angle);
            return new(_source.Offset.X - MathF.Cos(angle) * startAngularRadius, _source.Offset.Y - MathF.Sin(angle) * startAngularRadius);
        }
        public Coord GetDestinationOffset()
        {
            float angle = Coord.AngleBetween(_source.Offset, _destination.Offset);
            float endAngularRadius = _destination.GetAngularRadius(angle);
            return new(_destination.Offset.X + MathF.Cos(angle) * endAngularRadius, _destination.Offset.Y + MathF.Sin(angle) * endAngularRadius);
        }

        public override string ToString()
        {
            return $"{_source} -> {_destination}";
        }
    }
}
