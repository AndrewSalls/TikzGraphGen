namespace TikzGraphGen
{
    public class FreeLabel
    {
        public string Label { get; private set; }
        public float XOffset { get; private set; }
        public float YOffset { get; private set; }
        public float Angle { get; private set; }
        public float Radius { get; private set; }
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
