namespace TikzGraphGen
{
    public class BentEdge : Edge
    {
        public float BendDegree { get; set; }
        public BentEdge(GraphInfo settings) : base(settings)
        {
            BendDegree = 0;
        }
        public BentEdge(GraphInfo settings, Vertex s, Vertex d) : base(settings, s, d)
        {
            BendDegree = 0;
        }
        public BentEdge(EdgeLineStyle e, Vertex s, Vertex d, FreeLabel label, float bendDegree) : base(e, s, d, label)
        {
            BendDegree = bendDegree;
        }
    }
}
