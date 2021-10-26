using static TikzGraphGen.ToolSettingDictionary;

namespace TikzGraphGen
{
    public class CurvedEdge : Edge
    {
        public float ExitAngle { get; private set; }
        public float EnterAngle { get; private set; }
        public float CurveLooseness { get; private set; }

        public CurvedEdge(EdgeToolInfo settings, EdgeCapToolInfo capSettings) : base(settings, capSettings)
        {
            ExitAngle = 0;
            EnterAngle = 0;
            CurveLooseness = 0;
        }
        public CurvedEdge(EdgeToolInfo settings, EdgeCapToolInfo capSettings, Vertex s, Vertex d) : base(settings, capSettings, s, d)
        {
            ExitAngle = 0;
            EnterAngle = 0;
            CurveLooseness = 0;
        }
        public CurvedEdge(EdgeToolInfo e, EdgeCapToolInfo capSettings, Vertex s, Vertex d, FreeLabel label, float exitAngle, float enterAngle, float looseness) : base(e, capSettings, s, d, label)
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
