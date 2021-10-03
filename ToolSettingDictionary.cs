namespace TikzGraphGen
{
    public class ToolSettingDictionary
    {
        public interface IGenericToolInfo{ };

        public struct VertexToolInfo //TODO: Add tool data to these structs
        {

        }
        public struct EdgeToolInfo
        {

        }
        public struct EdgeCapToolInfo
        {

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

        }
        private void InitializeDefaultEdge()
        {

        }
        private void InitializeDefaultEdgeCap()
        {

        }
        private void InitializeDefaultLabel()
        {

        }
        private void InitializeDefaultEraser()
        {
            EraserInfo.Radius = 50;
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
