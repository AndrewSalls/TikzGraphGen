using System.Drawing;
using System.Windows.Forms;
using static TikzGraphGen.EdgeLineStyle;

namespace TikzGraphGen
{
    public class ToolSettingDictionary
    {
        public interface IGenericToolInfo{ };

        public struct VertexToolInfo //TODO: Add tool data to these structs
        {
            public VertexBorderStyle.BorderStyle Style { get; set; }
            public Color BorderColor { get; set; }
            public Color FillColor { get; set; }
            public float Thickness { get; set; }
            public int PolyCount { get; set; }
            public float Radius { get; set; }
            public float XRadius { get; set; }
            public float YRadius { get; set; }
        }
        public struct EdgeToolInfo
        {
            public Color Color { get; set; }
            public LineStyle Style { get; set; }
            public Concentration Density { get; set; }
            public float DashWidth { get; set; }
            public float DashSpacing { get; set; }
            public float PatternOffset { get; set; }
            public float Thickness { get; set; }
        }
        public struct EdgeCapToolInfo
        {
            public EdgeCapShape Style { get; set; }
            public bool IsThick { get; set; }
            public bool IsReversed { get; set; }
            public int TriangleDegree { get; set; }
        }
        public struct LabelToolInfo
        {

        }
        public struct EraserToolInfo
        {
            public float Radius { get; set; }
        }
        public struct TransformToolInfo
        {
        }
        public struct SelectToolInfo
        {
        }
        public struct AreaSelectToolInfo
        {
        }
        public struct LassoToolInfo
        {
        }
        public struct WeightToolInfo
        {
        }
        public struct TrackerToolInfo
        {
        }
        public struct MergeToolInfo
        {
        }
        public struct SplitToolInfo
        {

        }

        public VertexToolInfo VertexInfo;
        public EdgeToolInfo EdgeInfo;
        public EdgeCapToolInfo EdgeCapInfo;
        public LabelToolInfo LabelInfo;
        public EraserToolInfo EraserInfo;
        public TransformToolInfo TransformInfo;
        public SelectToolInfo SelectInfo;
        public AreaSelectToolInfo AreaSelectInfo;
        public LassoToolInfo LassoInfo;
        public WeightToolInfo WeightInfo;
        public TrackerToolInfo TrackerInfo;
        public MergeToolInfo MergeInfo;
        public SplitToolInfo SplitInfo;

        public ToolSettingDictionary()
        {
            //TODO: Define default info values here
            InitializeDefaultVertex();
            InitializeDefaultEdge();
            InitializeDefaultEdgeCap();
            InitializeDefaultLabel();
            InitializeDefaultEraser();
            InitializeDefaultTransform();
            InitializeDefaultSelect();
            InitializeDefaultAreaSelect();
            InitializeDefaultLasso();
            InitializeDefaultWeight();
            InitializeDefaultTracker();
            InitializeDefaultMerge();
            InitializeDefaultSplit();
        }

        private void InitializeDefaultVertex()
        {
            VertexInfo.Style = VertexBorderStyle.BorderStyle.Circle;
            VertexInfo.BorderColor = Color.Black;
            VertexInfo.FillColor = Color.White;
            VertexInfo.Thickness = 1;
            VertexInfo.PolyCount = 0;
            VertexInfo.Radius = 10f;
            VertexInfo.XRadius = 20f;
            VertexInfo.YRadius = 20f;
        }
        private void InitializeDefaultEdge()
        {
            EdgeInfo.Color = Color.Black;
            EdgeInfo.DashSpacing = 0;
            EdgeInfo.DashWidth = 0;
            EdgeInfo.Density = Concentration.Regular;
            EdgeInfo.PatternOffset = 0;
            EdgeInfo.Style = LineStyle.Solid;
            EdgeInfo.Thickness = 1;
        }
        private void InitializeDefaultEdgeCap()
        {
            EdgeCapInfo.Style = EdgeCapShape.Latex;
            EdgeCapInfo.IsReversed = false;
            EdgeCapInfo.IsThick = false;
            EdgeCapInfo.TriangleDegree = 90;
        }
        private void InitializeDefaultLabel()
        {

        }
        private void InitializeDefaultEraser()
        {
            EraserInfo.Radius = 20;
        }
        private void InitializeDefaultTransform()
        {

        }
        private void InitializeDefaultSelect()
        {

        }
        private void InitializeDefaultAreaSelect()
        {

        }
        private void InitializeDefaultLasso()
        {

        }
        private void InitializeDefaultWeight()
        {

        }
        private void InitializeDefaultTracker()
        {

        }
        private void InitializeDefaultMerge()
        {

        }
        private void InitializeDefaultSplit()
        {

        }

    }
}
