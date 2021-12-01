using System;
using System.Drawing;
using System.Windows.Forms;
using TikzGraphGen.GraphData;

//Next goals:
//Make it so that dragging when clicking the second vertex for the merge tool causes the merged vertex to be created at the current mouse position rather than at the midpoint
//Split this file's functions into three files. One stays the same, and handles setting up the window, one is DrawingWindowRenderer, and one is GraphInteractionInterface
//Implement Split
//Implement labels for editing vertices or creating labels in space (specially modified vertices with tag to not be picked up in algorithms
//Implement label edge snap (find mid point of edge (create function that is overriden in bent/curved edge) and find instantaneous angle at that point (also implement a function)
//                                then, check if mouse is above or below the point. Add text at point above or below correspondingly, spaced away along perpendicular line by pixel amount determined by LabelToolInfo's edgeSpacing property
//                                finally, add text rotation style property to LabelToolInfo and text rotation angle property (styles: angle, vertical, horizontal, parallel, perpendicular), which determine angle to write text at
//                                also add CenterType property (Start of text, end of text, midpoint, auto) - Auto will choose whichever centering will make the text avoid crossing over the text
//Create Hide Tool (hides vertex to allow editing vertices behind it; also include command/button to show all vertices)
//Implement Weight
//Implement Tracker
//Implement redo/undo
//Implement all of the Transform tools
//Make zooming in/out with shortcuts keep position under cursor at same coordinate
//Add setting to determine space to leave around ZoomFit on screen (e.g. setting of 1 inch means there is a 1 inch border of unfilled screenspace around the boundary of the graph)
//Begin implementing some algorithms

namespace TikzGraphGen.Visualization
{
    //TODO: Add fill tool for drawing planar colorizations (and maybe vertex colorizations): Add planar colorizations to analysis
    public class TikzDrawingWindow : Form
    {
        public enum SelectedTool
        {
            Vertex, Edge, EdgeCap, Label, Eraser, Transform, Shape, Select, AreaSelect, Lasso, Weight, Tracker, Merge, Split
        }

        public static readonly float[] FIXED_ZOOM_LEVEL_PERCENT = new float[] { 16f, 8f, 4f, 3f, 2f, 1f, 1/2f, 1/3f, 1/4f, 1/8f, 1/16f };
        public static readonly float ZOOM_OOB_MULTIPLIER = 0.8f;
        public static readonly int UNIQUE_ZOOM_LEVEL = -1;
        public static readonly float MAX_ZOOM = 100000f;
        public static readonly float MIN_ZOOM = 0.000001f;

        public static readonly Color DRAWING_BACKGROUND_COLOR = Color.White;

        public static readonly float DRAG_SENSITIVITY = 8.0f; //distance before dragging is recognized, in pixels
        public static readonly float MAX_ANGLE_LINK = 4.0f;

        public static readonly Point OFF_SCREEN = new(-1, -1);

        public static readonly ToolSettingDictionary DEFAULT_GRAPH_SETTINGS = new();

        private readonly RoutedShortcutCommand _rsc;
        private Graph _graph;
        private Graph _subgraphCopy;

        private readonly ToolActionData _info;
        private readonly DrawingWindowRenderer _renderer;
        private readonly GraphicalInteractionManager _gui;

        private int _fixedZoomLevel;
        private float _variableZoom;

        public TikzDrawingWindow(Form parent, RoutedShortcutCommand rsc) : base()
        {
            _graph = new Graph();
            _rsc = rsc;
            _subgraphCopy = null;

            _info = new ToolActionData(Width, _rsc);
            _renderer = new DrawingWindowRenderer(_info);
            _gui = new GraphicalInteractionManager(_info, () => _rsc.RefreshWindow(), (b) => _rsc.ModifyToolPermission(b));

            _fixedZoomLevel = 5; //5 = 1.0 scale (default)
            _variableZoom = FIXED_ZOOM_LEVEL_PERCENT[_fixedZoomLevel];

            Owner = parent;
            TopLevel = false;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;

            BackColor = DRAWING_BACKGROUND_COLOR;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            MouseClick += (o, e) => _gui.Click(e, _variableZoom, _graph, ClientRectangle, _rsc.CurrentTool, _rsc.ToolInfo); ;
            MouseDown += _gui.MouseDown;
            MouseMove += (o, e) => _gui.MouseMove(e, _variableZoom, _graph, ClientRectangle, _rsc.CurrentTool, _rsc.ToolInfo); ;
            MouseUp += (o, e) => _gui.MouseUp(e, _variableZoom, _graph, ClientRectangle, _rsc.CurrentTool, _rsc.ToolInfo);
            MouseLeave += (o, e) =>
            {
                _info.MouseDrag = OFF_SCREEN;
                ToolDataReset();
                Refresh();
            };

            _rsc.CurrentToolChanged += (t) => ToolDataReset();
            _rsc.Undo += () => _graph = Graph.Undo(_graph);
            _rsc.Redo += () => _graph = Graph.Redo(_graph);
            _rsc.DeleteSelected += () => { if(_info.Selected != null) _graph.DeleteSubgraph(_info.Selected); _info.Selected = null; Refresh(); };
            _rsc.Cut = () => { if (_info.Selected != null) { _subgraphCopy = _info.Selected; _graph.RemoveSubgraph(_subgraphCopy); _info.Selected = null; } };
            _rsc.Copy = () => { if(_info.Selected != null) _subgraphCopy = _info.Selected; };
            _rsc.Paste = () => { if (_subgraphCopy != null) { _graph.AddSubgraph(_subgraphCopy, _info.Corner + ((Coord)_info.MouseDrag) / _variableZoom); } };
            _rsc.ZoomInc = ZoomIn;
            _rsc.ZoomDec = ZoomOut;
            _rsc.ZoomFit = ZoomFit;
            _rsc.RefreshWindow = Refresh;
            _rsc.SelectAll = () => { _info.Selected = _graph; Refresh(); };

            _rsc.ZoomPercentageChanged += (f) => { 
                _variableZoom = f;
                for (int pos = FIXED_ZOOM_LEVEL_PERCENT.Length - 1; pos >= 0; pos--)
                {
                    if (FIXED_ZOOM_LEVEL_PERCENT[pos] != f)
                        continue;

                    _fixedZoomLevel = pos;
                }
            };
        }

        public bool HasUnsavedChanges() => _graph.UpdateFlag;

        public Graph GetData() => _graph;

        public void NewGraph(Graph graph)
        {
            _graph = graph;
            ToolDataReset();
            Refresh();
        }

        public void NewGraph() //Create graph from scratch
        {
            _graph = new Graph();
            ToolDataReset();
            Refresh();
        }

        public void ToolDataReset()
        {
            _info.Reset();
            Refresh();
        }

        public void ZoomIn()
        {
            if (_fixedZoomLevel > 0)
            {
                _fixedZoomLevel--;
                _variableZoom = FIXED_ZOOM_LEVEL_PERCENT[_fixedZoomLevel];
            }
            else if(_fixedZoomLevel == 0 || _variableZoom >= FIXED_ZOOM_LEVEL_PERCENT[0])
            {
                _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
                _variableZoom = MathF.Min(MAX_ZOOM, _variableZoom / ZOOM_OOB_MULTIPLIER);
            }
            else if(_variableZoom / ZOOM_OOB_MULTIPLIER < FIXED_ZOOM_LEVEL_PERCENT[^1]) //Zoom is significantly higher than largest fixed level
            {
                _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
                _variableZoom = MathF.Min(MAX_ZOOM, _variableZoom / ZOOM_OOB_MULTIPLIER);
            }
            else
            {
                int pos;
                for(pos = FIXED_ZOOM_LEVEL_PERCENT.Length - 1; pos >= 0; pos--)
                {
                    if(FIXED_ZOOM_LEVEL_PERCENT[pos] >= _variableZoom)
                        break;
                }
                _fixedZoomLevel = pos;
                _variableZoom = FIXED_ZOOM_LEVEL_PERCENT[pos];
            }
            Refresh();
        }
        public void ZoomOut()
        {
            if (_fixedZoomLevel != -1 && _fixedZoomLevel < FIXED_ZOOM_LEVEL_PERCENT.Length - 1)
            {
                _fixedZoomLevel++;
                _variableZoom = FIXED_ZOOM_LEVEL_PERCENT[_fixedZoomLevel];
            }
            else if (_fixedZoomLevel == FIXED_ZOOM_LEVEL_PERCENT.Length - 1 || _variableZoom <= FIXED_ZOOM_LEVEL_PERCENT[^1])
            {
                _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
                _variableZoom = MathF.Max(MIN_ZOOM, _variableZoom * ZOOM_OOB_MULTIPLIER);
            }
            else if (_variableZoom * ZOOM_OOB_MULTIPLIER > FIXED_ZOOM_LEVEL_PERCENT[0]) //Zoom is significantly smaller than smallest fixed level
            {
                _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
                _variableZoom = MathF.Max(MIN_ZOOM, _variableZoom * ZOOM_OOB_MULTIPLIER);
            }
            else
            {
                int pos;
                for (pos = 0; pos <= FIXED_ZOOM_LEVEL_PERCENT.Length - 1; pos++)
                {
                    if (FIXED_ZOOM_LEVEL_PERCENT[pos] <= _variableZoom)
                        break;
                }
                _fixedZoomLevel = pos;
                _variableZoom = FIXED_ZOOM_LEVEL_PERCENT[pos];
            }
            Refresh();
        }

        public void ZoomFit()
        {
            (Coord min, Coord max) = _graph.GetBounds();
            float xRatio = Width / (max.X - min.X);
            float yRatio = Height / (max.Y - min.Y);

            _variableZoom = Math.Min(xRatio, yRatio);
            _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;

            Coord shift;
            if (yRatio < xRatio)
                shift = new((Width - yRatio * (max.X - min.X)) / 2f, 0);
            else
                shift = new(0, (Height - xRatio * (max.Y - min.Y)) / 2f);

            _info.Corner = min - shift / _variableZoom;
            Refresh();
        }

        public void ScrollVisibleArea(float dx, float dy)
        {
            _info.Corner += (dx, dy);
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.ResetTransform();
            base.OnPaint(e);
            e.Graphics.ScaleTransform(_variableZoom, _variableZoom);
            _renderer.PaintScreen(e, _variableZoom, _graph, ClientRectangle, _rsc.CurrentTool, _rsc.ToolInfo);
        }

        public static Coord FindVertexRepositioning(Graph g, ToolActionData i, Coord drawPos, Coord relativeTo = null) //TODO: Add shortcut to lock snapping to the current closest vertex, so that intersecting snapping ranges are less annoying. To do this, save closest vertex (or allow selecting one) then use that value for the optional parameter
        {
            Coord origin = relativeTo ?? g.GetPointClosestTo(drawPos)?.Offset ?? drawPos;

            float angle = Coord.AngleBetween(drawPos, origin);
            float offset = Coord.DistanceFrom(drawPos, origin);

            if (!i.SnapToUnitGrid && (origin.Equals(drawPos) || offset > MAX_ANGLE_LINK * DrawingWindowRenderer.UNIT_SIZE))
                return drawPos;

            if (!i.SnapToUnitGrid && i.SnapToUnit)
            {
                offset = Round(offset, DrawingWindowRenderer.UNIT_SIZE);
                drawPos = new(origin.X + MathF.Cos(angle) * offset, origin.Y + MathF.Sin(angle) * offset);
            }
            if (!i.SnapToUnitGrid && i.SnapToAngle)
            {
                angle = MathF.Max(0, MathF.Min(Round(angle, i.AngleSnapAmt), 2 * MathF.PI));
                drawPos = new(origin.X + MathF.Cos(angle) * offset, origin.Y + MathF.Sin(angle) * offset);
            }
            if (i.SnapToUnitGrid)
                drawPos = new(Round(drawPos.X + i.Corner.X, DrawingWindowRenderer.UNIT_SIZE) - i.Corner.X, Round(drawPos.Y + i.Corner.Y, DrawingWindowRenderer.UNIT_SIZE) - i.Corner.Y);

                return drawPos;
        }

        /**
         * Rounds a number to the nearest multiple of a provided number
         * 
         * startVal is value being rounded
         * range is interval such that k*range <= startVal <= (k + 1)*range
         **/
        private static float Round(float startVal, float range)
        {
            if (MathF.Abs(startVal % range) > (range / 2f))
                startVal += MathF.Sign(startVal) * range;
            startVal -= startVal % range;

            return startVal;
        }
    }
}