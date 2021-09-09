namespace TikzGraphGen
{
    public class CurvedEdge : Edge
    {
        public float ExitAngle { get; private set; }
        public float EnterAngle { get; private set; }
        public float CurveLooseness { get; private set; }

        public CurvedEdge(GraphInfo settings) : base(settings)
        {
            ExitAngle = 0;
            EnterAngle = 0;
            CurveLooseness = 0;
        }
        public CurvedEdge(GraphInfo settings, Vertex s, Vertex d) : base(settings, s, d)
        {
            ExitAngle = 0;
            EnterAngle = 0;
            CurveLooseness = 0;
        }
        public CurvedEdge(EdgeLineStyle e, Vertex s, Vertex d, FreeLabel label, float exitAngle, float enterAngle, float looseness) : base(e, s, d, label)
        {
            ExitAngle = exitAngle;
            EnterAngle = enterAngle;
            CurveLooseness = looseness;
        }

        public override string ToString()
        {
            return $"{_source} u> {_destination}";
        }
    }
}
