namespace TikzGraphGen
{
    public class FreeLabel
    {
        public string Label { get; set; }
        public float XOffset { get; set; }
        public float YOffset { get; set; }
        public float Angle { get; set; }
        public float Radius { get; set; }
        protected Vertex origin;

        public FreeLabel(string name, float xOff, float yOff, float angle, float r, Vertex o)
        {
            Label = name;
            XOffset = xOff;
            YOffset = yOff;
            Angle = angle;
            Radius = r;
            origin = o;
        }
    }
}
