using static TikzGraphGen.ToolSettingDictionary;

namespace TikzGraphGen
{
    public class Edge
    {
        public EdgeToolInfo Style { get; private set; }
        public FreeLabel Label { get; private set; }
        public double Value { get; private set; }
        protected Vertex _source, _destination;

        public Edge(EdgeToolInfo settings)
        {
            Style = settings;
            Label = null;
            Value = 0;
            _source = null;
            _destination = null;
        }
        public Edge(EdgeToolInfo settings, Vertex s, Vertex d)
        {
            Style = settings;
            Label = null;
            Value = 0;
            _source = s;
            _destination = d;
        }
        public Edge(EdgeToolInfo e, Vertex s, Vertex d, FreeLabel label)
        {
            Style = e;
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

        public override string ToString()
        {
            return $"{_source} -> {_destination}";
        }
    }
}
