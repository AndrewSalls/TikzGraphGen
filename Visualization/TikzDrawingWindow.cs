using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TikzGraphGen.GraphData;

namespace TikzGraphGen.Visualization
{
    //TODO: Make vertex/edge thickness, radius, etc. and distance from the corner scale to zoom level
    public class TikzDrawingWindow : Form
    {
        public enum SelectedTool
        {
            Vertex, Edge, EdgeCap, Label, Transform, Select, AreaSelect, Lasso, Weight, Tracker, Merge, Split
        }
        public static readonly float[] FIXED_ZOOM_LEVEL_PERCENT = new float[] { 1/16, 1/8, 1/4, 1/3, 1/2, 1, 2, 3, 4, 8, 16 };
        public static readonly float ZOOM_OOB_MULTIPLIER = 0.8f;
        public static readonly int UNIQUE_ZOOM_LEVEL = -1;

        public static readonly Color DRAWING_BACKGROUND_COLOR = Color.White;

        public static readonly GraphInfo DEFAULT_GRAPH_SETTINGS = new()
        {
            defaultBGColor = Color.White,
            defaultLines = new EdgeLineStyle(Color.Black, 1),
            defaultBorders = new VertexBorderStyle(VertexBorderStyle.BorderStyle.Circle, Color.Black, 1, 10)
        };

        private readonly RoutedShortcutCommand _rsc;
        private Graph _graph;
        private Graph _selectedSubgraph;
        private Graph _subgraphCopy;

        private int _fixedZoomLevel;
        private float _variableZoom;
        private Coord _visibleCorner;

        private Vertex _firstVertex;

        private bool _drawBorder;
        private bool _angleSnap;
        private bool _unitSnap;
        private bool _drawUnitGrid; 

        public TikzDrawingWindow(Form parent, RoutedShortcutCommand rsc) : this(parent, DEFAULT_GRAPH_SETTINGS, rsc) { }
        public TikzDrawingWindow(Form parent, GraphInfo settings, RoutedShortcutCommand rsc) : base()
        {
            _graph = new Graph(settings);
            _rsc = rsc;
            _selectedSubgraph = null;
            _subgraphCopy = null;
            _fixedZoomLevel = 5;
            _variableZoom = 1;
            _visibleCorner = new Coord(0, 0);

            _drawBorder = true;
            _angleSnap = true;
            _unitSnap = false;

            _firstVertex = null;

            Owner = parent;
            TopLevel = false;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;

            BackColor = DRAWING_BACKGROUND_COLOR;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            MouseClick += TikzDrawingWindow_Click;

            _rsc.Undo += () => _graph = Graph.Undo(_graph);
            _rsc.Redo += () => _graph = Graph.Redo(_graph);
            _rsc.Cut = () => { if (_selectedSubgraph != null) { _subgraphCopy = _selectedSubgraph; _graph.RemoveSubgraph(_subgraphCopy); _selectedSubgraph = null; } };
            _rsc.Copy = () => { if(_selectedSubgraph != null) _subgraphCopy = _selectedSubgraph; };
            _rsc.Paste = () => { if (_subgraphCopy != null) { _graph.AddSubgraph(_subgraphCopy, MouseToCoord(new Coord(MousePosition.X, MousePosition.Y))); } };
            _rsc.ZoomInc = ZoomIn;
            _rsc.ZoomDec = ZoomOut;
            _rsc.ZoomFit = ZoomFit;
            _rsc.ToggleBorder = () => { _drawBorder = !_drawBorder; Refresh(); };
            _rsc.ToggleAngleSnap = () => _angleSnap = !_angleSnap;
            _rsc.ToggleUnitSnap = () => _unitSnap = !_unitSnap;
            _rsc.ToggleUnitGrid = () => { _drawUnitGrid = !_drawUnitGrid; Refresh(); };
            _rsc.SelectAll = () => _selectedSubgraph = _graph;
        }

        public bool HasUnsavedChanges()
        {
            return _graph.UpdateFlag;
        }

        public Graph GetData()
        {
            return _graph;
        }

        public void NewGraph(Graph graph)
        {
            throw new NotImplementedException();
        }

        public void NewGraph() //Create graph from scratch
        {
            throw new NotImplementedException();
        }

        public void SelectSubArea(Rectangle area)
        {
            throw new NotImplementedException();
        }

        public Coord MouseToCoord(Coord mousePos)
        {
            throw new NotImplementedException();
        }

        public void ZoomIn()
        {
            if (_fixedZoomLevel > 0)
            {
                _fixedZoomLevel--;
                _variableZoom = FIXED_ZOOM_LEVEL_PERCENT[_fixedZoomLevel];
            }
            else if(_variableZoom <= FIXED_ZOOM_LEVEL_PERCENT[0])
            {
                _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
                _variableZoom *= ZOOM_OOB_MULTIPLIER;
            }
            else if(_variableZoom >= FIXED_ZOOM_LEVEL_PERCENT[^1] / ZOOM_OOB_MULTIPLIER) //Zoom is significantly higher than largest fixed level
            {
                _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
                _variableZoom *= ZOOM_OOB_MULTIPLIER;
            }
            else
            {
                int pos;
                for(pos = FIXED_ZOOM_LEVEL_PERCENT.Length - 1; pos >= 0; pos--)
                {
                    if(FIXED_ZOOM_LEVEL_PERCENT[pos] < _variableZoom)
                        break;
                }
                _fixedZoomLevel = pos;
                _variableZoom = FIXED_ZOOM_LEVEL_PERCENT[pos];
            }
        }
        public void ZoomOut()
        {
            if (_fixedZoomLevel > 0 && _fixedZoomLevel < FIXED_ZOOM_LEVEL_PERCENT.Length - 1)
            {
                _fixedZoomLevel++;
                _variableZoom = FIXED_ZOOM_LEVEL_PERCENT[_fixedZoomLevel];
            }
            else if (_variableZoom >= FIXED_ZOOM_LEVEL_PERCENT[^1])
            {
                _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
                _variableZoom *= 1 + ZOOM_OOB_MULTIPLIER;
            }
            else if (_variableZoom <= FIXED_ZOOM_LEVEL_PERCENT[0] / (1 + ZOOM_OOB_MULTIPLIER)) //Zoom is significantly smaller than smallest fixed level
            {
                _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
                _variableZoom *= 1 + ZOOM_OOB_MULTIPLIER;
            }
            else
            {
                int pos;
                for (pos = 0; pos < FIXED_ZOOM_LEVEL_PERCENT.Length; pos++)
                {
                    if (FIXED_ZOOM_LEVEL_PERCENT[pos] > _variableZoom)
                        break;
                }
                _fixedZoomLevel = pos;
                _variableZoom = FIXED_ZOOM_LEVEL_PERCENT[pos];
            }
        }

        public void ZoomFit()
        {
            Coord bounds = _graph.GetBounds();
            float xRatio = Width / bounds.X;
            float yRatio = Height / bounds.Y;
            _variableZoom = Math.Min(xRatio, yRatio);
            _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
        }

        public void ScrollVisibleArea(float dx, float dy)
        {
            _visibleCorner += (dx, dy);
        }

        public bool CanUndo()
        {
            return _graph.CanUndo();
        }
        public bool CanRedo()
        {
            return _graph.CanRedo();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            float zoomAmt = _fixedZoomLevel != UNIQUE_ZOOM_LEVEL ? FIXED_ZOOM_LEVEL_PERCENT[_fixedZoomLevel] : _variableZoom;
            Graph sub = _graph.GetSubgraphWithin(_visibleCorner, zoomAmt * (ClientRectangle.Width + _visibleCorner.X), zoomAmt * (ClientRectangle.Height + _visibleCorner.Y));

            if (!sub.IsEmpty())
            {
                foreach (Vertex v in sub.ViewVertices())
                    DrawVertex(e.Graphics, v, FindDistanceFrom(_visibleCorner, v));

                foreach(Edge eg in sub.ViewEdges())
                    DrawEdge(e.Graphics, eg, FindDistanceFrom(_visibleCorner, eg.ViewSource()), FindDistanceFrom(_visibleCorner, eg.ViewDestination()));
            }

            //TODO: Draw border and/or unit grid here
        }

        private static Coord FindDistanceFrom(Coord point, Vertex v)
        {
            return v.Offset - point;
        }

        private static void DrawVertex(Graphics g, Vertex v, Coord pos) //TODO: Add support for other edge/vertex types, text in vertices, etc.
        {
            switch (v.Style.Style)
            {
                case VertexBorderStyle.BorderStyle.Circle:
                    g.DrawEllipse(new Pen(new SolidBrush(v.Style.BorderColor), v.Style.Thickness), pos.X - v.Style.Radius, pos.Y - v.Style.Radius, 2 * v.Style.Radius, 2 * v.Style.Radius);
                    break;
                case VertexBorderStyle.BorderStyle.None:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        private static void DrawEdge(Graphics g, Edge e, Coord start, Coord end)
        {
            Pen p = e.Style.Style switch
            {
                EdgeLineStyle.LineStyle.Solid => new Pen(e.Style.Color, e.Style.Thickness),
                EdgeLineStyle.LineStyle.Dash => new Pen(e.Style.Color, e.Style.Thickness)
                {
                    DashStyle = DashStyle.Dash,
                    Width = e.Style.Thickness,
                    DashOffset = e.Style.PatternOffset,
                    DashPattern = new float[] { e.Style.DashWidth, e.Style.DashSpacing }
                },
                EdgeLineStyle.LineStyle.Dot => new Pen(e.Style.Color, e.Style.Thickness)
                {
                    DashStyle = DashStyle.Dot,
                    Width = e.Style.Thickness,
                    DashOffset = e.Style.PatternOffset,
                    DashPattern = new float[] { e.Style.DashSpacing }
                },
                EdgeLineStyle.LineStyle.None => new Pen(Color.Transparent),
                _ => throw new NotImplementedException(),
            };
            switch (e) //TODO: Draw bent/curved edge
            {
                case BentEdge _:
                    throw new NotImplementedException();
                case CurvedEdge _:
                    throw new NotImplementedException();
                case Edge _:
                    g.DrawLine(p, start.X, start.Y, end.X, end.Y);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void TikzDrawingWindow_Click(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                for (int i = 0; i < e.Delta; i++)
                {
                    ZoomIn();
                }
                for (int i = e.Delta; i < 0; i++)
                {
                    ZoomOut();
                }
            }

            Coord mousePos = e.Location;
            float zoomAmt = _fixedZoomLevel != UNIQUE_ZOOM_LEVEL ? FIXED_ZOOM_LEVEL_PERCENT[_fixedZoomLevel] : _variableZoom;

            switch (_rsc.CurrentTool)
            {
                case SelectedTool.Vertex:
                    _graph.CreateVertex(_visibleCorner + mousePos * zoomAmt);
                    Refresh();
                    break;
                case SelectedTool.Edge:
                   if(_firstVertex == null)
                        _firstVertex = GetVertexAt(_visibleCorner + mousePos * zoomAmt);
                   else
                    {
                        Vertex _secondVertex = GetVertexAt(_visibleCorner + mousePos * zoomAmt);
                        if (_firstVertex != null && _secondVertex != null && _firstVertex != _secondVertex && !_firstVertex.IsAdjacentTo(_secondVertex)) //TODO: Make support loops & multiedges later (give warning or automatically curve lines) (A -> A)
                            _graph.CreateEdge(_firstVertex, _secondVertex);
                        _firstVertex = null;
                    }
                    Refresh();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private Vertex GetVertexAt(Coord c)
        {
            return _graph.ViewVertices().Find(v => (c.X >= v.Offset.X - v.Style.Radius - (v.Style.OblongWidth / 2) &&
                   c.Y >= v.Offset.Y - v.Style.Radius - (v.Style.OblongHeight / 2) &&
                   c.X <= v.Offset.X + v.Style.Radius + (v.Style.OblongWidth / 2) &&
                   c.Y <= v.Offset.Y + v.Style.Radius + (v.Style.OblongWidth / 2)));
        }
    }
}
