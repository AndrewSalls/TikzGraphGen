namespace TikzGraphGen
{
    public class Edge
    {
        public EdgeLineStyle Style { get; set; }
        public FreeLabel Label { get; set; }
        public double Value { get; set; }
        protected Vertex _source, _destination;

        public Edge(GraphInfo settings)
        {
            Style = settings.defaultLines;
            Label = null;
            Value = 0;
            _source = null;
            _destination = null;
        }
        public Edge(GraphInfo settings, Vertex s, Vertex d)
        {
            Style = settings.defaultLines;
            Label = null;
            Value = 0;
            _source = s;
            _destination = d;
        }
        public Edge(EdgeLineStyle e, Vertex s, Vertex d)
        {
            Style = e;
            Label = null;
            Value = 0;
            _source = s;
            _destination = d;
        }
        public Edge(EdgeLineStyle e, Vertex s, Vertex d, FreeLabel label)
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
    }
}
