using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TikzGraphGen.GraphData;

namespace TikzGraphGen.Visualization
{
    //TODO: Implement zooming in/out
    //TODO: Add flag for _rsc that prevents changing tools while performing an action with the current tool
    //TODO: Add fill tool for drawing planar colorizations (and maybe vertex colorizations): Add planar colorizations to analysis
    public class TikzDrawingWindow : Form
    {
        public enum SelectedTool
        {
            Vertex, Edge, EdgeCap, Label, Eraser, Transform, Select, AreaSelect, Lasso, Weight, Tracker, Merge, Split
        }
        public static readonly float[] FIXED_ZOOM_LEVEL_PERCENT = new float[] { 1/16, 1/8, 1/4, 1/3, 1/2, 1, 2, 3, 4, 8, 16 };
        public static readonly float ZOOM_OOB_MULTIPLIER = 0.8f;
        public static readonly int UNIQUE_ZOOM_LEVEL = -1;

        public static readonly Color DRAWING_BACKGROUND_COLOR = Color.White;

        public static readonly double DRAG_SENSITIVITY = 8.0; //distance before dragging is recognized, in pixels
        public static readonly Color ERASER_HIGHLIGHT_COLOR = Color.FromArgb(155, 200, 200, 200);
        public static readonly float MAX_ANGLE_LINK = 4.0f;

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

        //private int _fixedZoomLevel;
        //private float _variableZoom;
        private Coord _visibleCorner;
        private Point? _mouseDownPos;
        private Point _mouseDragPos;
        private bool _isDragging;

        private Vertex _firstVertex;

        private bool _drawBorder;
        private bool _angleSnap;
        private float _angleSnapAmt; //By default snaps by 15 degrees (represented as pi / 12). Must be between 0 and 2pi
        private bool _unitSnap;
        private bool _gridUnitSnap;
        private float _unitSize; //By default 1 unit = 1 mm
        private bool _drawUnitGrid; 

        public TikzDrawingWindow(Form parent, RoutedShortcutCommand rsc) : this(parent, DEFAULT_GRAPH_SETTINGS, rsc) { }
        public TikzDrawingWindow(Form parent, GraphInfo settings, RoutedShortcutCommand rsc) : base()
        {
            _graph = new Graph(settings);
            _rsc = rsc;
            _selectedSubgraph = null;
            _subgraphCopy = null;
            //_fixedZoomLevel = 5;
            //_variableZoom = 1;
            _visibleCorner = new Coord(0, 0);

            _drawBorder = true;
            _angleSnap = true;
            _angleSnapAmt = ((float)Math.PI) / 12f;
            _unitSnap = false;
            _gridUnitSnap = false;
            _unitSize = UnitConverter.ST_MM * 10; //1 cm, a common Tikz unit
            _mouseDownPos = null;
            _mouseDragPos = new Point(0, 0);
            _isDragging = false;

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
            MouseDown += TikzDrawingWindow_MouseDown;
            MouseMove += TikzDrawingWindow_MouseMove;
            MouseUp += TikzDrawingWindow_MouseUp;

            _rsc.Undo += () => _graph = Graph.Undo(_graph);
            _rsc.Redo += () => _graph = Graph.Redo(_graph);
            _rsc.DeleteSelected += () => { _graph.RemoveSubgraph(_selectedSubgraph); _selectedSubgraph = null; };
            _rsc.Cut = () => { if (_selectedSubgraph != null) { _subgraphCopy = _selectedSubgraph; _graph.RemoveSubgraph(_subgraphCopy); _selectedSubgraph = null; } };
            _rsc.Copy = () => { if(_selectedSubgraph != null) _subgraphCopy = _selectedSubgraph; };
            _rsc.Paste = () => { if (_subgraphCopy != null) { _graph.AddSubgraph(_subgraphCopy, MouseToCoord(new Coord(MousePosition.X, MousePosition.Y))); } };
            _rsc.ZoomInc = ZoomIn;
            _rsc.ZoomDec = ZoomOut;
            _rsc.ZoomFit = ZoomFit;
            _rsc.ToggleBorder = () => { _drawBorder = !_drawBorder; Refresh(); };
            _rsc.ToggleAngleSnap = () => { _angleSnap = !_angleSnap; Refresh(); };
            _rsc.ToggleUnitSnap = () => { _unitSnap = !_unitSnap; if (_gridUnitSnap && _unitSnap) _rsc.ToggleGridUnitSnap(); else Refresh(); };
            _rsc.ToggleGridUnitSnap = () => { _gridUnitSnap = !_gridUnitSnap; if (_gridUnitSnap && _unitSnap) _rsc.ToggleUnitSnap(); else Refresh(); };
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
            /*
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
            */
        }
        public void ZoomOut()
        {
            /*
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
            }*/
        }

        public void ZoomFit()
        {
            /*
            Coord bounds = _graph.GetBounds();
            float xRatio = Width / bounds.X;
            float yRatio = Height / bounds.Y;
            _variableZoom = Math.Min(xRatio, yRatio);
            _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;]
            */
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

            Graph sub = _graph.GetSubgraphWithin(_visibleCorner, (ClientRectangle.Width + _visibleCorner.X), (ClientRectangle.Height + _visibleCorner.Y));
            Coord mouseGraphPos = _visibleCorner + _mouseDragPos;

            //TODO: Draw border and/or unit grid here

            if (!sub.IsEmpty())
            {
                foreach (Vertex v in sub.ViewVertices())
                    DrawVertex(e.Graphics, v, v.Offset - _visibleCorner);

                foreach(Edge eg in sub.ViewEdges())
                    DrawEdge(e.Graphics, eg, eg.ViewSource().Offset - _visibleCorner, eg.ViewDestination().Offset - _visibleCorner);
            }

            if (_rsc.CurrentTool == SelectedTool.Edge && _isDragging && _firstVertex != null)
                DrawEdge(e.Graphics, new Edge(_graph.Info, null, null), _firstVertex.Offset, _mouseDragPos);
            if (_rsc.CurrentTool == SelectedTool.Eraser && _isDragging)
                e.Graphics.FillEllipse(new SolidBrush(ERASER_HIGHLIGHT_COLOR), ((Coord)_mouseDragPos).X - _rsc.ToolInfo.EraserInfo.Radius, ((Coord)_mouseDragPos).Y - _rsc.ToolInfo.EraserInfo.Radius, 2 * _rsc.ToolInfo.EraserInfo.Radius, 2 * _rsc.ToolInfo.EraserInfo.Radius);
            if (_rsc.CurrentTool == SelectedTool.Vertex && (_unitSnap || _angleSnap || _gridUnitSnap))
            {
                Coord center = FindVertexRepositioning(new Coord(mouseGraphPos.X, mouseGraphPos.Y));

                if(!center.Equals(mouseGraphPos) || (_unitSnap || _graph.GetPointClosestTo(mouseGraphPos) != null && Coord.DistanceFrom(mouseGraphPos, _graph.GetPointClosestTo(mouseGraphPos).Offset) <= MAX_ANGLE_LINK * _unitSize))
                    e.Graphics.FillEllipse(new SolidBrush(ERASER_HIGHLIGHT_COLOR), center.X - _rsc.ToolInfo.VertexInfo.XRadius, center.Y - _rsc.ToolInfo.VertexInfo.YRadius, 2 * _rsc.ToolInfo.VertexInfo.XRadius, 2 * _rsc.ToolInfo.VertexInfo.YRadius);
            }
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

            switch (_rsc.CurrentTool)
            {
                case SelectedTool.Vertex:
                    if (!_isDragging)
                    {
                        Coord drawPos = FindVertexRepositioning(_visibleCorner + mousePos);

                        _graph.CreateVertex(drawPos);
                    }
                    break;
                case SelectedTool.Edge:
                    if(_firstVertex == null)
                        _firstVertex = GetVerticesIn(_visibleCorner + mousePos).FirstOrDefault();
                    else
                    {
                        Vertex _secondVertex = GetVerticesIn(_visibleCorner + mousePos).FirstOrDefault();
                        if (_firstVertex != null && _secondVertex != null && _firstVertex != _secondVertex && !_firstVertex.IsAdjacentTo(_secondVertex)) //TODO: Make support loops & multiedges later (give warning or automatically curve lines) (A -> A)
                            _graph.CreateEdge(_firstVertex, _secondVertex);

                        _firstVertex = null;
                    }
                    break;
                case SelectedTool.Eraser:
                    Graph sub = _graph.GetSubgraphTouchingCircle(_visibleCorner + mousePos, _rsc.ToolInfo.EraserInfo.Radius);
                    if(sub.ViewEdges().Count > 0 || sub.ViewVertices().Count > 0)
                        _graph.RemoveSubgraph(sub);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private Coord FindVertexRepositioning(Coord drawPos)
        {
            Coord origin = _graph.GetPointClosestTo(drawPos)?.Offset ?? drawPos;

            float angle = Coord.AngleBetween(drawPos, origin);
            float offset = Coord.DistanceFrom(drawPos, origin);

            if (!_gridUnitSnap && (origin.Equals(drawPos) || (!_unitSnap && offset > MAX_ANGLE_LINK * _unitSize)))
                return drawPos;

            if (_unitSnap)
            {
                offset = Round(offset, _unitSize);
                drawPos = new(origin.X + MathF.Cos(angle) * offset, origin.Y + MathF.Sin(angle) * offset);
            }
            if (_angleSnap)
            {
                angle = Round(angle, _angleSnapAmt);
                drawPos = new(origin.X + MathF.Cos(angle) * offset, origin.Y + MathF.Sin(angle) * offset);
            }
            if(_gridUnitSnap)
                drawPos = new(Round(drawPos.X, _unitSize), Round(drawPos.Y, _unitSize));

            return drawPos;
        }

        private static float Round(float startVal, float range)
        {
            if ((startVal % range) > (range / 2f))
                startVal += range;
            startVal -= startVal % range;

            return startVal;
        }

        public void TikzDrawingWindow_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDownPos = e.Location;
        }
        public void TikzDrawingWindow_MouseMove(object sender, MouseEventArgs e)
        {
            _mouseDragPos = e.Location;

            if (!_isDragging && _rsc.CurrentTool.Equals(SelectedTool.Vertex) && (_unitSnap || _angleSnap || _gridUnitSnap))
                Refresh();

            if (_isDragging || (_mouseDownPos != null && Math.Sqrt(Math.Pow(e.Location.X - ((Point)_mouseDownPos).X, 2) + Math.Pow(e.Location.Y - ((Point)_mouseDownPos).Y, 2)) > DRAG_SENSITIVITY))
                DragEvent(sender, e);
        }

        private void DragEvent(object sender, MouseEventArgs e)
        {
            _isDragging = true;

            Coord mousePos = e.Location;

            switch (_rsc.CurrentTool)
            {
                case SelectedTool.Vertex:
                    break;
                case SelectedTool.Edge:
                    if (_mouseDownPos != null)
                        _firstVertex = GetVerticesIn(_visibleCorner + _mouseDownPos).FirstOrDefault();
                    break;
                case SelectedTool.Eraser: //TODO: Check for subgraph that touches circle, not that is inside of circle
                    Graph sub = _graph.GetSubgraphTouchingCircle(_visibleCorner + mousePos, _rsc.ToolInfo.EraserInfo.Radius);
                    if (sub.ViewEdges().Count > 0 || sub.ViewVertices().Count > 0)
                        _graph.RemoveSubgraph(sub);
                    break;
                default:
                    throw new NotImplementedException();
            }
            Refresh();
        }

        public void TikzDrawingWindow_MouseUp(object sender, MouseEventArgs e)
        {
            Vertex _secondVertex = GetVerticesIn(_visibleCorner + e.Location).FirstOrDefault();

            if (_firstVertex != null && _secondVertex != null && _isDragging && _firstVertex != _secondVertex && !_firstVertex.IsAdjacentTo(_secondVertex)) //TODO: Make support loops & multiedges later (give warning or automatically curve lines) (A -> A)
                _graph.CreateEdge(_firstVertex, _secondVertex);

            _mouseDownPos = null;
            if (_isDragging)
                _firstVertex = null;
            _isDragging = false;

            Refresh();
        }

        private List<Vertex> GetVerticesIn(Coord c, float radius = 0)
        {
            return _graph.ViewVertices().FindAll(v => c.X >= v.Offset.X - v.Style.Radius - (v.Style.OblongWidth / 2) - radius &&
                   c.Y >= v.Offset.Y - v.Style.Radius - (v.Style.OblongHeight / 2) - radius &&
                   c.X <= v.Offset.X + v.Style.Radius + (v.Style.OblongWidth / 2) + radius &&
                   c.Y <= v.Offset.Y + v.Style.Radius + (v.Style.OblongWidth / 2) + radius);
        }
        private List<Edge> GetEdgesIn(Coord coord, float radius = 0)
        {
            return _graph.ViewEdges().FindAll(e =>
            {
                float a = e.ViewSource().Offset.Y - e.ViewDestination().Offset.Y;
                float b = e.ViewDestination().Offset.X - e.ViewSource().Offset.X;
                float c = (e.ViewSource().Offset.X * e.ViewDestination().Offset.Y) - (e.ViewDestination().Offset.X * e.ViewSource().Offset.Y);
                return radius >= Math.Abs(a * coord.X + b * coord.Y + c) / Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
            });
        }
    }
}