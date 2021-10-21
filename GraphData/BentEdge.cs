using static TikzGraphGen.ToolSettingDictionary;

namespace TikzGraphGen
{
    public class BentEdge : Edge
    {
        public float BendDegree { get; private set; }
        public BentEdge(EdgeToolInfo settings) : base(settings)
        {
            BendDegree = 0;
        }
        public BentEdge(Vertex s, Vertex d, EdgeToolInfo settings) : base(settings, s, d)
        {
            BendDegree = 0;
        }
        public BentEdge(Vertex s, Vertex d, FreeLabel label, float bendDegree, EdgeToolInfo settings) : base(settings, s, d, label)
        {
            BendDegree = bendDegree;
        }

        public override string ToString()
        {
            return $"{_source} ^> {_destination}";
        }
    }
}
