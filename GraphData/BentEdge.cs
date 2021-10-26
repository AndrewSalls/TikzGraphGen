using static TikzGraphGen.ToolSettingDictionary;

namespace TikzGraphGen
{
    public class BentEdge : Edge
    {
        public float BendDegree { get; private set; }
        public BentEdge(EdgeToolInfo settings, EdgeCapToolInfo capSettings) : base(settings, capSettings)
        {
            BendDegree = 0;
        }
        public BentEdge(Vertex s, Vertex d, EdgeToolInfo settings, EdgeCapToolInfo capSettings) : base(settings, capSettings, s, d)
        {
            BendDegree = 0;
        }
        public BentEdge(Vertex s, Vertex d, FreeLabel label, float bendDegree, EdgeToolInfo settings, EdgeCapToolInfo capSettings) : base(settings, capSettings, s, d, label)
        {
            BendDegree = bendDegree;
        }

        public override string ToString()
        {
            return $"{_source} ^> {_destination}";
        }
    }
}
