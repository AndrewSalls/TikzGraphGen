using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TikzGraphGen.GraphData;
using static TikzGraphGen.ToolSettingDictionary;

//Next goals:
//Fix Set Ratio (link to drawing window) and Zoom Fit (account for angular radius offset + make actually work)
//Shape tool to evenly space vertices (and menu Shape options)
//Implement labels for editing vertices or creating labels in space (specially modified vertices with tag to not be picked up in algorithms
//Implement label edge snap (find mid point of edge (create function that is overriden in bent/curved edge) and find instantaneous angle at that point (also implement a function)
//                                then, check if mouse is above or below the point. Add text at point above or below correspondingly, spaced away along perpendicular line by pixel amount determined by LabelToolInfo's edgeSpacing property
//                                finally, add text rotation style property to LabelToolInfo and text rotation angle property (styles: angle, vertical, horizontal, parallel, perpendicular), which determine angle to write text at
//                                also add CenterType property (Start of text, end of text, midpoint, auto) - Auto will choose whichever centering will make the text avoid crossing over the text
//Implement Merge
//Implement Split
//Implement Weight
//Implement Tracker
//Implement redo/undo
//Implement all of the Transform tools
//Create Hide Tool (hides vertex to allow editing vertices behind it; also include command/button to show all vertices)
//Make zooming in/out with shortcuts keep position under cursor at same coordinate

namespace TikzGraphGen.Visualization
{
    //TODO: Add fill tool for drawing planar colorizations (and maybe vertex colorizations): Add planar colorizations to analysis
    public class TikzDrawingWindow : Form
    {
        public enum SelectedTool
        {
            Vertex, Edge, EdgeCap, Label, Eraser, Transform, Select, AreaSelect, Lasso, Weight, Tracker, Merge, Split
        }
        public static readonly float[] FIXED_ZOOM_LEVEL_PERCENT = new float[] { 16f, 8f, 4f, 3f, 2f, 1f, 1/2f, 1/3f, 1/4f, 1/8f, 1/16f };
        public static readonly float ZOOM_OOB_MULTIPLIER = 0.8f;
        public static readonly int UNIQUE_ZOOM_LEVEL = -1;
        public static readonly float MAX_ZOOM = 100000f;
        public static readonly float MIN_ZOOM = 0.000001f;

        public static readonly Color DRAWING_BACKGROUND_COLOR = Color.White;

        public static readonly float DRAG_SENSITIVITY = 8.0f; //distance before dragging is recognized, in pixels
        public static readonly Color ACTION_HIGHLIGHT_COLOR = Color.FromArgb(155, 200, 200, 200);
        public static readonly Color SELECTED_HIGHLIGHT_COLOR = Color.FromArgb(155, 172, 206, 247);
        public static readonly Color BORDER_GUIDE_COLOR = Color.FromArgb(255, 150, 150, 150);
        public static readonly float BORDER_GUIDE_WIDTH = 2f;
        public static readonly Color UNIT_GRID_GUIDE_COLOR = Color.FromArgb(255, 200, 200, 200);
        public static readonly float UNIT_GRID_GUIDE_WIDTH = 1f;
        public static readonly float HIGHLIGHT_WIDTH = 3f;
        public static readonly float MAX_ANGLE_LINK = 4.0f;
        public static readonly SizeF PAGE_SIZE = new(UnitConverter.InToPx(8.5f), UnitConverter.InToPx(11f));

        public static readonly ToolSettingDictionary DEFAULT_GRAPH_SETTINGS = new();

        private readonly RoutedShortcutCommand _rsc;
        private Graph _graph;
        private Graph _selectedSubgraph;
        private Graph _subgraphCopy;

        private Coord _visibleCorner;
        private Point? _mouseDownPos;
        private Point _mouseDragPos;
        private bool _isDragging;

        private Vertex _firstVertex;
        private readonly List<PointF> _lassoPts;

        private bool _drawBorder;
        private bool _angleSnap;
        private float _angleSnapAmt; //By default snaps by 15 degrees (represented as pi / 12). Must be between 0 and 2pi
        private bool _unitSnap;
        private bool _gridUnitSnap;
        private float _unitSize; //By default 1 unit = 1 mm
        private bool _labelEdgeSnap;
        private bool _drawUnitGrid;

        private int _fixedZoomLevel;
        private float _variableZoom;

        public TikzDrawingWindow(Form parent, RoutedShortcutCommand rsc) : base()
        {
            _graph = new Graph();
            _rsc = rsc;
            _selectedSubgraph = null;
            _subgraphCopy = null;
            _visibleCorner = new Coord((Width - PAGE_SIZE.Width) / 2, 0);

            _drawBorder = false;
            _drawUnitGrid = false;
            _angleSnap = true;
            _angleSnapAmt = MathF.PI / 12f;
            _unitSnap = false;
            _gridUnitSnap = false;
            _unitSize = UnitConverter.ST_MM * 10; //1 cm, a common Tikz unit
            _mouseDownPos = null;
            _mouseDragPos = new Point(0, 0);
            _isDragging = false;

            _firstVertex = null;
            _lassoPts = new List<PointF>();

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

            MouseClick += TikzDrawingWindow_Click;
            MouseDown += TikzDrawingWindow_MouseDown;
            MouseMove += TikzDrawingWindow_MouseMove;
            MouseUp += TikzDrawingWindow_MouseUp;

            _rsc.CurrentToolChanged += (t) =>
            {
                _firstVertex = null;
                _isDragging = false;
                _mouseDownPos = null;
                _lassoPts.Clear();
                Refresh();
            };
            _rsc.Undo += () => _graph = Graph.Undo(_graph);
            _rsc.Redo += () => _graph = Graph.Redo(_graph);
            _rsc.DeleteSelected += () => { if(_selectedSubgraph != null) _graph.DeleteSubgraph(_selectedSubgraph); _selectedSubgraph = null; Refresh(); };
            _rsc.Cut = () => { if (_selectedSubgraph != null) { _subgraphCopy = _selectedSubgraph; _graph.RemoveSubgraph(_subgraphCopy); _selectedSubgraph = null; } };
            _rsc.Copy = () => { if(_selectedSubgraph != null) _subgraphCopy = _selectedSubgraph; };
            _rsc.Paste = () => { if (_subgraphCopy != null) { _graph.AddSubgraph(_subgraphCopy, MouseToCoord(new Coord(MousePosition.X, MousePosition.Y))); } };
            _rsc.ZoomInc = ZoomIn;
            _rsc.ZoomDec = ZoomOut;
            _rsc.ZoomFit = ZoomFit;
            _rsc.ToggleBorder = () => { _drawBorder = !_drawBorder; Refresh(); };
            _rsc.ToggleAngleSnap = () => { _angleSnap = !_angleSnap; Refresh(); };
            _rsc.ToggleUnitSnap = () => { _unitSnap = !_unitSnap; Refresh(); };
            _rsc.ToggleGridUnitSnap = () => { _gridUnitSnap = !_gridUnitSnap; Refresh(); };
            _rsc.ToggleUnitGrid = () => { _drawUnitGrid = !_drawUnitGrid; Refresh(); };
            _rsc.ToggleLabelEdgeSnap = () => { _labelEdgeSnap = !_labelEdgeSnap; Refresh(); };
            _rsc.SelectAll = () => { _selectedSubgraph = _graph; Refresh(); };

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
            (Coord min, Coord max) bounds = _graph.GetBounds();
            float xRatio = Width / (bounds.max.X - bounds.min.X);
            float yRatio = Height / (bounds.max.Y - bounds.min.Y);
            _visibleCorner = bounds.min;
            _variableZoom = Math.Min(xRatio, yRatio);
            _fixedZoomLevel = UNIQUE_ZOOM_LEVEL;
            Refresh();
        }

        public void ScrollVisibleArea(float dx, float dy)
        {
            _visibleCorner += (dx, dy);
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.ResetTransform();
            base.OnPaint(e);

            e.Graphics.ScaleTransform(_variableZoom, _variableZoom);
            Graph sub = _graph.GetSubgraphTouchingPolygon(new() { _visibleCorner, new(_visibleCorner.X + ClientRectangle.Width / _variableZoom, _visibleCorner.Y),
                                                                  _visibleCorner + new Coord(ClientRectangle.Width, ClientRectangle.Height) / _variableZoom, new(_visibleCorner.X, _visibleCorner.Y + ClientRectangle.Height / _variableZoom) });

            if (_drawUnitGrid)
            {
                for (float x = _visibleCorner.X - _visibleCorner.X % _unitSize; x <= _visibleCorner.X + Width; x += _unitSize)
                    e.Graphics.DrawLine(new Pen(new SolidBrush(UNIT_GRID_GUIDE_COLOR), UNIT_GRID_GUIDE_WIDTH), x - _visibleCorner.X, 0, x - _visibleCorner.X, Height);
                for (float y = _visibleCorner.Y - _visibleCorner.Y % _unitSize; y <= _visibleCorner.Y + Height; y += _unitSize)
                    e.Graphics.DrawLine(new Pen(new SolidBrush(UNIT_GRID_GUIDE_COLOR), UNIT_GRID_GUIDE_WIDTH), 0, y - _visibleCorner.Y, Width, y - _visibleCorner.Y);
            }
            if(_drawBorder) //I'm too lazy to just draw the line segments that are actually visible
                e.Graphics.DrawRectangle(new Pen(new SolidBrush(BORDER_GUIDE_COLOR), BORDER_GUIDE_WIDTH), -_visibleCorner.X, -_visibleCorner.Y, PAGE_SIZE.Width, PAGE_SIZE.Height);

            Graph selectedSub = _selectedSubgraph?.GetSubgraphWithin(_visibleCorner, Width, Height);
            if (selectedSub != null && !selectedSub.IsEmpty())
            {
                foreach (Edge eg in selectedSub.ViewEdges())
                    DrawHighlightedEdge(e.Graphics, eg);

                foreach (Vertex v in selectedSub.ViewVertices())
                    DrawHighlightedVertex(e.Graphics, v, v.Offset - _visibleCorner);
            }

            if (!sub.IsEmpty())
                foreach (Edge eg in sub.ViewEdges())
                    DrawEdge(e.Graphics, eg);

            PreVertexPaint(e);

            if (!sub.IsEmpty())
                foreach (Vertex v in sub.ViewVertices())
                    DrawVertex(e.Graphics, v, v.Offset - _visibleCorner);

            PostVertexPaint(e);
            e.Graphics.ResetTransform();
            e.Graphics.DrawString($"{_variableZoom}", new Font(FontFamily.GenericMonospace, 20), new SolidBrush(Color.Black), new PointF());
        }

        private void PreVertexPaint(PaintEventArgs e)
        {
            switch(_rsc.CurrentTool)
            {
                case SelectedTool.Edge:
                    if (_isDragging && _firstVertex != null)
                    {
                        EdgeToolInfo edge = _rsc.ToolInfo.EdgeInfo;
                        EdgeCapToolInfo edgeCap = _rsc.ToolInfo.EdgeCapInfo;
                        edge.Color = ACTION_HIGHLIGHT_COLOR;
                        edgeCap.Style = EdgeLineStyle.EdgeCapShape.None;
                        DrawEdge(e.Graphics, new Edge(edge, edgeCap, _firstVertex, new Vertex(new() { Style = VertexBorderStyle.BorderStyle.None, FillColor = Color.Transparent }, _visibleCorner + (Coord)_mouseDragPos / _variableZoom)));
                    }
                    break;
                case SelectedTool.Select:
                    if (_selectedSubgraph != null && _isDragging)
                        foreach (Edge eg in _selectedSubgraph.ViewEdges())
                            DrawEdge(e.Graphics, eg);
                    break;
                case SelectedTool.Vertex:
                case SelectedTool.EdgeCap:
                case SelectedTool.Eraser:
                case SelectedTool.AreaSelect:
                case SelectedTool.Lasso:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        private void PostVertexPaint(PaintEventArgs e)
        {
            switch (_rsc.CurrentTool)
            {
                case SelectedTool.Vertex:
                    Coord mouseGraphPos = _visibleCorner + (Coord)_mouseDragPos / _variableZoom;
                    if (_unitSnap || _angleSnap || _gridUnitSnap)
                    {
                        Coord center = FindVertexRepositioning(mouseGraphPos);

                        if (!FindVertexRepositioning(mouseGraphPos).Equals(mouseGraphPos) || _graph.GetPointClosestTo(mouseGraphPos) != null && Coord.DistanceFrom(mouseGraphPos, _graph.GetPointClosestTo(mouseGraphPos).Offset) <= MAX_ANGLE_LINK * _unitSize)
                        {
                            Coord adjustedMousePos = center - _visibleCorner;
                            e.Graphics.FillEllipse(new SolidBrush(ACTION_HIGHLIGHT_COLOR), adjustedMousePos.X - _rsc.ToolInfo.VertexInfo.Radius - _rsc.ToolInfo.VertexInfo.XRadius, adjustedMousePos.Y - _rsc.ToolInfo.VertexInfo.Radius - _rsc.ToolInfo.VertexInfo.YRadius, 2 * _rsc.ToolInfo.VertexInfo.Radius + 2 * _rsc.ToolInfo.VertexInfo.XRadius, 2 * _rsc.ToolInfo.VertexInfo.Radius + 2 * _rsc.ToolInfo.VertexInfo.YRadius);
                            Coord origin = _graph.GetSubgraphWithin(_visibleCorner, Width, Height).GetPointClosestTo(mouseGraphPos)?.Offset;
                            if(origin != null)
                                adjustedMousePos = origin - _visibleCorner;
                            if (!_gridUnitSnap && _unitSnap && origin != null) //Unit snap rendering
                            {
                                for (float dist = 0; dist <= MAX_ANGLE_LINK * _unitSize; dist += _unitSize)
                                    e.Graphics.DrawEllipse(new Pen(new SolidBrush(ACTION_HIGHLIGHT_COLOR)), adjustedMousePos.X - dist, adjustedMousePos.Y - dist, 2 * dist, 2 * dist);
                            }
                            if (!_gridUnitSnap && _angleSnap && origin != null) //Angle snap rendering
                            {
                                for (float sum = 0; sum <= 2 * MathF.PI; sum += _angleSnapAmt)
                                    e.Graphics.DrawLine(new Pen(new SolidBrush(ACTION_HIGHLIGHT_COLOR)), adjustedMousePos, adjustedMousePos + (MAX_ANGLE_LINK * _unitSize) * new Coord(MathF.Cos(sum), MathF.Sin(sum)));
                            }
                        }
                    }
                    break;
                case SelectedTool.Edge:
                case SelectedTool.EdgeCap:
                    break;
                case SelectedTool.Eraser:
                    if (_isDragging)
                        e.Graphics.FillEllipse(new SolidBrush(ACTION_HIGHLIGHT_COLOR), ((Coord)_mouseDragPos / _variableZoom).X - _rsc.ToolInfo.EraserInfo.Radius, ((Coord)_mouseDragPos / _variableZoom).Y - _rsc.ToolInfo.EraserInfo.Radius, 2 * _rsc.ToolInfo.EraserInfo.Radius, 2 * _rsc.ToolInfo.EraserInfo.Radius);
                    break;
                case SelectedTool.Select:
                    if (_selectedSubgraph != null && _isDragging)
                        foreach (Vertex v in _selectedSubgraph.ViewVertices())
                            DrawVertex(e.Graphics, v, v.Offset - _visibleCorner);
                    break;
                case SelectedTool.AreaSelect:
                    if (_isDragging)
                        e.Graphics.FillRectangle(new SolidBrush(SELECTED_HIGHLIGHT_COLOR), ((Point)_mouseDownPos).X / _variableZoom, ((Point)_mouseDownPos).Y / _variableZoom, (_mouseDragPos.X - ((Point)_mouseDownPos).X) / _variableZoom, (_mouseDragPos.Y - ((Point)_mouseDownPos).Y) / _variableZoom);
                    break;
                case SelectedTool.Lasso:
                    if (_lassoPts.Count == 2)
                        e.Graphics.DrawLine(new Pen(new SolidBrush(SELECTED_HIGHLIGHT_COLOR)), _lassoPts[0] - _visibleCorner, _lassoPts[1] - _visibleCorner);
                    else if (_lassoPts.Count >= 3)
                        e.Graphics.FillPolygon(new SolidBrush(SELECTED_HIGHLIGHT_COLOR), _lassoPts.Append(_lassoPts[0]).Select(p => (PointF)(p - _visibleCorner)).ToArray());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void DrawVertex(Graphics g, Vertex v, Coord pos) //TODO: Add support for other edge/vertex types, text in vertices, etc.
        {
            switch (v.Style.Style)
            {
                case VertexBorderStyle.BorderStyle.Circle:
                    g.FillEllipse(new SolidBrush(v.Style.FillColor), pos.X - v.Style.Radius, pos.Y - v.Style.Radius, 2 * v.Style.Radius, 2 * v.Style.Radius);
                    g.DrawEllipse(new Pen(new SolidBrush(v.Style.BorderColor), v.Style.Thickness), pos.X - v.Style.Radius, pos.Y - v.Style.Radius, 2 * v.Style.Radius, 2 * v.Style.Radius);
                    break;
                case VertexBorderStyle.BorderStyle.None:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        private static void DrawHighlightedVertex(Graphics g, Vertex v, Coord pos)
        {
            g.FillEllipse(new SolidBrush(SELECTED_HIGHLIGHT_COLOR), pos.X - v.Style.Radius - HIGHLIGHT_WIDTH, pos.Y - v.Style.Radius - HIGHLIGHT_WIDTH, 2 * v.Style.Radius + 2 * HIGHLIGHT_WIDTH, 2 * v.Style.Radius + 2 * HIGHLIGHT_WIDTH);
        }
        private void DrawEdge(Graphics g, Edge e)
        {
            Pen p = e.Style.LineInfo.Style switch
            {
                EdgeLineStyle.LineStyle.Solid => new Pen(e.Style.LineInfo.Color, e.Style.LineInfo.Thickness),
                EdgeLineStyle.LineStyle.Dash => new Pen(e.Style.LineInfo.Color, e.Style.LineInfo.Thickness)
                {
                    DashStyle = DashStyle.Dash,
                    Width = e.Style.LineInfo.Thickness,
                    DashOffset = e.Style.LineInfo.PatternOffset,
                    DashPattern = new float[] { e.Style.LineInfo.DashWidth, e.Style.LineInfo.DashSpacing }
                },
                EdgeLineStyle.LineStyle.Dot => new Pen(e.Style.LineInfo.Color, e.Style.LineInfo.Thickness)
                {
                    DashStyle = DashStyle.Dot,
                    Width = e.Style.LineInfo.Thickness,
                    DashOffset = e.Style.LineInfo.PatternOffset,
                    DashPattern = new float[] { e.Style.LineInfo.DashSpacing }
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
                    Coord start = e.GetSourceOffset() - _visibleCorner;
                    Coord end = e.GetDestinationOffset() - _visibleCorner;
                    g.DrawLine(p, start.X, start.Y, end.X, end.Y);
                    break;
                default:
                    throw new NotImplementedException();
            }

            DrawEdgeCaps(g, e);
        }
        private void DrawHighlightedEdge(Graphics g, Edge e)
        {
            Pen p = new(SELECTED_HIGHLIGHT_COLOR, e.Style.LineInfo.Thickness + 2 * HIGHLIGHT_WIDTH);

            switch (e) //TODO: Draw bent/curved edge
            {
                case BentEdge _:
                    throw new NotImplementedException();
                case CurvedEdge _:
                    throw new NotImplementedException();
                case Edge _:
                    Coord start = e.GetSourceOffset() - _visibleCorner;
                    Coord end = e.GetDestinationOffset() - _visibleCorner;
                    g.DrawLine(p, start.X, start.Y, end.X, end.Y);
                    break;
                default:
                    throw new NotImplementedException();
            }

            DrawHighlightedEdgeCaps(g, e);
        }

        private void DrawEdgeCaps(Graphics g, Edge e)
        {
           DrawSpecificEdgeCap(g, e.GetSourceOffset() - _visibleCorner, Coord.AngleBetween(e.ViewDestination().Offset, e.ViewSource().Offset), e.Style.SDirectionCap, e.Style.LineInfo.Color, e.Style.LineInfo.Thickness, false);
           DrawSpecificEdgeCap(g, e.GetDestinationOffset() - _visibleCorner, Coord.AngleBetween(e.ViewSource().Offset, e.ViewDestination().Offset), e.Style.DDirectionCap, e.Style.LineInfo.Color, e.Style.LineInfo.Thickness, false);
        }
        private void DrawHighlightedEdgeCaps(Graphics g, Edge e)
        {
            DrawSpecificEdgeCap(g, e.GetSourceOffset() - _visibleCorner, Coord.AngleBetween(e.ViewDestination().Offset, e.ViewSource().Offset), e.Style.SDirectionCap, SELECTED_HIGHLIGHT_COLOR, e.Style.LineInfo.Thickness, true);
            DrawSpecificEdgeCap(g, e.GetDestinationOffset() - _visibleCorner, Coord.AngleBetween(e.ViewSource().Offset, e.ViewDestination().Offset), e.Style.DDirectionCap, SELECTED_HIGHLIGHT_COLOR, e.Style.LineInfo.Thickness, true);
        }
        //TODO: Modify drawing edge caps to properly follow dimensions of PGF arrow tips
        private static void DrawSpecificEdgeCap(Graphics g, Coord capEdge, float pointDirection, EdgeCapToolInfo style, Color color, float lineWidth, bool isHighlight)
        {
            Coord unit = Coord.AngleUnit(pointDirection);
            Coord perpUnit = Coord.AngleUnit(pointDirection + MathF.PI / 2);
            switch (style.Style)
            {
                case EdgeLineStyle.EdgeCapShape.Latex:
                case EdgeLineStyle.EdgeCapShape.Stealth:
                    PointF[] points = new PointF[4] {
                        capEdge,
                        capEdge + (style.ParallelScale * 15 * lineWidth) * unit + (style.PerpendicularScale * 5 * lineWidth) * perpUnit,
                        capEdge + (style.ParallelScale * 10 * lineWidth) * unit,
                        capEdge + (style.ParallelScale * 15 * lineWidth) * unit - (style.PerpendicularScale * 5 * lineWidth) * perpUnit
                    };
                    if (isHighlight)
                    {
                        points[0] -= HIGHLIGHT_WIDTH * unit;
                        points[1] += HIGHLIGHT_WIDTH * Coord.AngleUnit((Coord.AngleBetween(points[1], capEdge) + Coord.AngleBetween(points[1], points[2])) / 2);
                        points[3] += HIGHLIGHT_WIDTH * Coord.AngleUnit((Coord.AngleBetween(points[3], capEdge) + Coord.AngleBetween(points[3], points[2])) / 2);
                        points[2] += HIGHLIGHT_WIDTH * unit;
                    }
                    g.FillPolygon(new SolidBrush(color), points);
                    break;
                case EdgeLineStyle.EdgeCapShape.Triangle:
                    float baseAngle = (180 - style.TriangleDegree) * MathF.PI / 360; //Triangle is always an isoceles, so obtains one of the base side angles
                    float baseScale = (15 * lineWidth) / MathF.Tan(baseAngle);
                    points = new PointF[3] {
                        capEdge,
                        capEdge + (15 * lineWidth) * unit + baseScale * perpUnit,
                        capEdge + (15 * lineWidth) * unit - baseScale * perpUnit
                    };
                    if(isHighlight)
                    {
                        points[0] -= HIGHLIGHT_WIDTH * unit;
                        points[1] += HIGHLIGHT_WIDTH * Coord.AngleUnit(pointDirection + baseAngle);
                        points[2] += HIGHLIGHT_WIDTH * Coord.AngleUnit(pointDirection - baseAngle);
                    }
                    g.FillPolygon(new SolidBrush(color), points);
                    break;
                case EdgeLineStyle.EdgeCapShape.OpenTriangle:
                    baseAngle = (180 - style.TriangleDegree) * MathF.PI / 360; //Triangle is always an isoceles, so obtains one of the base side angles
                    baseScale = (15 * lineWidth) / MathF.Tan(baseAngle);
                    points = new PointF[3] {
                        capEdge,
                        capEdge + (15 * lineWidth) * unit + baseScale * perpUnit,
                        capEdge + (15 * lineWidth) * unit - baseScale * perpUnit
                    };
                    if (isHighlight)
                    {
                        points[0] -= HIGHLIGHT_WIDTH * unit;
                        points[1] += HIGHLIGHT_WIDTH * Coord.AngleUnit(pointDirection + baseAngle);
                        points[2] += HIGHLIGHT_WIDTH * Coord.AngleUnit(pointDirection - baseAngle);
                    }
                    g.DrawPolygon(new Pen(new SolidBrush(color), lineWidth), points);
                    break;
                case EdgeLineStyle.EdgeCapShape.Circle:
                    g.FillEllipse(new SolidBrush(color), new RectangleF(capEdge + (7.5f * lineWidth) * unit - new Coord(7.5f * lineWidth + (isHighlight ? HIGHLIGHT_WIDTH : 0), 7.5f * lineWidth + (isHighlight ? HIGHLIGHT_WIDTH : 0)), new SizeF(15 * lineWidth + (isHighlight ? 2 * HIGHLIGHT_WIDTH : 0), 15 * lineWidth + (isHighlight ? 2 * HIGHLIGHT_WIDTH : 0))));
                    break;
                case EdgeLineStyle.EdgeCapShape.ButtCap:
                case EdgeLineStyle.EdgeCapShape.None:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void TikzDrawingWindow_Click(object sender, MouseEventArgs e)
        {
            Coord mousePos = _visibleCorner + (Coord)e.Location / _variableZoom;

            switch (_rsc.CurrentTool)
            {
                case SelectedTool.Vertex:
                    if (!_isDragging)
                        _graph.CreateVertex(FindVertexRepositioning(mousePos), _rsc.ToolInfo.VertexInfo);
                    break;
                case SelectedTool.Edge:
                    if (_firstVertex == null)
                    {
                        _firstVertex = GetVerticesInCircle(_graph, mousePos).FirstOrDefault();
                    }
                    else
                    {
                        Vertex _secondVertex = GetVerticesInCircle(_graph, mousePos).FirstOrDefault();
                        if (_firstVertex != null && _secondVertex != null && _firstVertex != _secondVertex && !_firstVertex.IsAdjacentTo(_secondVertex)) //TODO: Make support loops & multiedges later (give warning or automatically curve lines) (A -> A)
                            _graph.CreateEdge(_firstVertex, _secondVertex, _rsc.ToolInfo.EdgeInfo, _rsc.ToolInfo.EdgeCapInfo);

                        _firstVertex = null;
                    }
                    break;
                case SelectedTool.EdgeCap:
                    List<Edge> edges = _graph.GetSubgraphTouchingCircle(mousePos, _rsc.ToolInfo.EraserInfo.Radius)?.ViewEdges();
                    if(edges.Count > 0)
                    {
                        Edge ed = edges.Aggregate((close, e) => {
                            float sourceDist = Coord.DistanceFrom(e.GetSourceOffset(), mousePos);
                            float destinationDist = Coord.DistanceFrom(e.GetDestinationOffset(), mousePos);
                            float sourceDistR = Coord.DistanceFrom(close.GetSourceOffset(), mousePos);
                            float destinationDistR = Coord.DistanceFrom(close.GetDestinationOffset(), mousePos);
                            return (sourceDist <= sourceDistR && sourceDist <= destinationDistR) || (destinationDist <= sourceDistR && destinationDist <= destinationDistR) ? e : close;
                        });
                        if (Coord.DistanceFrom(ed.GetSourceOffset(), mousePos) <= Coord.DistanceFrom(ed.GetDestinationOffset(), mousePos))
                            ed.Style.SDirectionCap = _rsc.ToolInfo.EdgeCapInfo;
                        else
                            ed.Style.DDirectionCap = _rsc.ToolInfo.EdgeCapInfo;
                    }
                    break;
                case SelectedTool.Eraser:
                    Graph sub = _graph.GetSubgraphTouchingCircle(mousePos, 1); //Very small eraser to try and only erase what is directly clicked on
                    if(sub.ViewEdges().Count > 0 || sub.ViewVertices().Count > 0)
                        _graph.DeleteSubgraph(sub);
                    break;
                case SelectedTool.Select:
                    if(!_isDragging)
                        _selectedSubgraph = _graph.GetSubgraphTouchingCircle(mousePos, 1); //Very small selector to try and only select what is directly clicked on
                    break;
                case SelectedTool.AreaSelect:
                    break;
                case SelectedTool.Lasso:
                    if(!_isDragging)
                    {
                        if (_lassoPts.Count > 1 && (Coord.DistanceFrom(mousePos, _lassoPts[0]) < _rsc.ToolInfo.EraserInfo.Radius || Coord.DistanceFrom(mousePos, _lassoPts[^1]) < DRAG_SENSITIVITY))
                        {
                            _selectedSubgraph = _graph.GetSubgraphTouchingPolygon(_lassoPts.Append(_lassoPts[0]).Select(p => (Coord)p).ToList());
                            _lassoPts.Clear();
                        }
                        else
                            _lassoPts.Add(mousePos);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private Coord FindVertexRepositioning(Coord drawPos) //TODO: Add shortcut to lock snapping to the current closest vertex, so that intersecting snapping ranges are less annoying
        {
            Coord origin = _graph.GetPointClosestTo(drawPos)?.Offset ?? drawPos;

            float angle = Coord.AngleBetween(drawPos, origin);
            float offset = Coord.DistanceFrom(drawPos, origin);

            if (!_gridUnitSnap && (origin.Equals(drawPos) || offset > MAX_ANGLE_LINK * _unitSize))
                return drawPos;

            if (!_gridUnitSnap && _unitSnap)
            {
                offset = Round(offset, _unitSize);
                drawPos = new(origin.X + MathF.Cos(angle) * offset, origin.Y + MathF.Sin(angle) * offset);
            }
            if (!_gridUnitSnap && _angleSnap)
            {
                angle = MathF.Max(0, MathF.Min(Round(angle, _angleSnapAmt), 2 * MathF.PI));
                drawPos = new(origin.X + MathF.Cos(angle) * offset, origin.Y + MathF.Sin(angle) * offset);
            }
            if(_gridUnitSnap)
                drawPos = new(Round(drawPos.X, _unitSize), Round(drawPos.Y, _unitSize));

            return drawPos;
        }

        private static float Round(float startVal, float range)
        {
            if (MathF.Abs(startVal % range) > (range / 2f))
                startVal += MathF.Sign(startVal) * range;
            startVal -= startVal % range;

            return startVal;
        }

        public void TikzDrawingWindow_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDownPos = e.Location;
            _mouseDragPos = e.Location;
        }
        public void TikzDrawingWindow_MouseMove(object sender, MouseEventArgs e)
        {

            if (!_isDragging && _rsc.CurrentTool.Equals(SelectedTool.Vertex) && (_unitSnap || _angleSnap || _gridUnitSnap))
                Refresh();

            if (_isDragging || (_mouseDownPos != null && Math.Sqrt(Math.Pow(e.Location.X - ((Point)_mouseDownPos).X, 2) + Math.Pow(e.Location.Y - ((Point)_mouseDownPos).Y, 2)) > DRAG_SENSITIVITY))
                DragEvent(sender, e);

            _mouseDragPos = e.Location;
        }

        private void DragEvent(object sender, MouseEventArgs e)
        {
            Coord mousePos = _visibleCorner + (Coord)e.Location / _variableZoom;

            switch (_rsc.CurrentTool)
            {
                case SelectedTool.Vertex:
                case SelectedTool.EdgeCap:
                case SelectedTool.AreaSelect:
                    break;
                case SelectedTool.Edge:
                    _firstVertex = GetVerticesInCircle(_graph, _visibleCorner + (Coord)_mouseDownPos / _variableZoom).FirstOrDefault();
                    if(_firstVertex != null)
                        _rsc.ModifyToolPermission(false);
                    break;
                case SelectedTool.Eraser:
                    Graph sub = _graph.GetSubgraphTouchingCircle(mousePos, _rsc.ToolInfo.EraserInfo.Radius);
                    if (sub.ViewEdges().Count > 0 || sub.ViewVertices().Count > 0)
                        _graph.DeleteSubgraph(sub);
                    break;
                case SelectedTool.Select:
                    if (!_isDragging) //Only sets subgraph when first moving mouse enough to start dragging (instead of every frame)
                    {
                        //Only sets subgraph if the selected subgraph is not being clicked
                        if (_selectedSubgraph == null || _selectedSubgraph.GetSubgraphTouchingCircle(_visibleCorner + (Coord)_mouseDownPos / _variableZoom, 1).IsEmpty())
                            _selectedSubgraph = _graph.GetSubgraphTouchingCircle(_visibleCorner + (Coord)_mouseDownPos / _variableZoom, 1); //Very small selector to try and only click what is directly clicked on
                        
                        _graph.RemoveSubgraph(_selectedSubgraph, true);
                        _rsc.ModifyToolPermission(false);
                    }
                    _selectedSubgraph.Translate(((Coord)e.Location - _mouseDragPos) / _variableZoom);
                    break;
                case SelectedTool.Lasso:
                    if (_isDragging)
                        _lassoPts.RemoveAt(_lassoPts.Count - 1);

                    if (_lassoPts.Count > 0 && Coord.DistanceFrom(mousePos, _lassoPts[0]) < _rsc.ToolInfo.EraserInfo.Radius)
                        _lassoPts.Add(_lassoPts[0]);
                    else if(_lassoPts.Count == 0) //If first lasso movement is a mouse drag, instantly creates line rather than ignoring dragging part
                    {
                        _lassoPts.Add(_visibleCorner + (Coord)_mouseDownPos / _variableZoom);
                        _lassoPts.Add(mousePos);
                    }
                    else
                        _lassoPts.Add(mousePos);

                    _rsc.ModifyToolPermission(false);
                    break;
                default:
                    throw new NotImplementedException();
            }

            _isDragging = true;
            Refresh();
        }

        public void TikzDrawingWindow_MouseUp(object sender, MouseEventArgs e)
        {
            switch (_rsc.CurrentTool)
            {
                case SelectedTool.Vertex:
                case SelectedTool.EdgeCap:
                case SelectedTool.Eraser:
                    break;
                case SelectedTool.Edge:
                    Vertex _secondVertex = GetVerticesInCircle(_graph, _visibleCorner + e.Location).FirstOrDefault();

                    if (_firstVertex != null && _secondVertex != null && _isDragging && _firstVertex != _secondVertex && !_firstVertex.IsAdjacentTo(_secondVertex)) //TODO: Make support loops & multiedges later (give warning or automatically curve lines) (A -> A)
                        _graph.CreateEdge(_firstVertex, _secondVertex, _rsc.ToolInfo.EdgeInfo, _rsc.ToolInfo.EdgeCapInfo);

                    _rsc.ModifyToolPermission(true);
                    break;
                case SelectedTool.Select:
                    _graph.AddSubgraph(_selectedSubgraph, true);
                    _rsc.ModifyToolPermission(true);
                    break;
                case SelectedTool.AreaSelect:
                    Coord downPos = _visibleCorner + (Coord)_mouseDownPos / _variableZoom, dragPos = _visibleCorner + (Coord)_mouseDragPos / _variableZoom;
                    Coord tlCorner = new(MathF.Min(downPos.X, dragPos.X), MathF.Min(downPos.Y, dragPos.Y));
                    Coord brCorner = new(MathF.Max(downPos.X, dragPos.X), MathF.Max(downPos.Y, dragPos.Y));
                    _selectedSubgraph = _graph.GetSubgraphWithin(tlCorner, brCorner.X - tlCorner.X, brCorner.Y - tlCorner.Y);
                    break;
                case SelectedTool.Lasso:
                    if (_isDragging && _lassoPts.Count > 2 && _lassoPts[^1].Equals(_lassoPts[0]))
                    {
                        _selectedSubgraph = _graph.GetSubgraphTouchingPolygon(_lassoPts.Select(p => (Coord)p).ToList());
                        _lassoPts.Clear();
                    }
                    _rsc.ModifyToolPermission(true);
                    break;
                default:
                    throw new NotImplementedException();
            }
            _mouseDownPos = null;
            if (_isDragging)
                _firstVertex = null;
            _isDragging = false;

            Refresh();
        }

        private static List<Vertex> GetVerticesInCircle(Graph g, Coord c)
        {
            return g.ViewVertices().FindAll(v =>
                   c.X >= v.Offset.X - v.Style.Radius - (v.Style.XRadius / 2) &&
                   c.Y >= v.Offset.Y - v.Style.Radius - (v.Style.YRadius / 2) &&
                   c.X <= v.Offset.X + v.Style.Radius + (v.Style.XRadius / 2) &&
                   c.Y <= v.Offset.Y + v.Style.Radius + (v.Style.XRadius / 2));
        }
    }
}