using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TikzGraphGen.GraphData;
using static TikzGraphGen.ToolSettingDictionary;
using static TikzGraphGen.Visualization.TikzDrawingWindow;

namespace TikzGraphGen.Visualization
{
    public class DrawingWindowRenderer
    {
        public static readonly Color ACTION_HIGHLIGHT_COLOR = Color.FromArgb(155, 200, 200, 200);
        public static readonly Color SELECTED_HIGHLIGHT_COLOR = Color.FromArgb(155, 172, 206, 247);
        public static readonly Color BORDER_GUIDE_COLOR = Color.FromArgb(255, 150, 150, 150);
        public static readonly float BORDER_GUIDE_WIDTH = 2f;
        public static readonly Color UNIT_GRID_GUIDE_COLOR = Color.FromArgb(255, 200, 200, 200);
        public static readonly float UNIT_GRID_GUIDE_WIDTH = 1f;
        public static readonly float HIGHLIGHT_WIDTH = 3f;
        public static readonly SizeF PAGE_SIZE = new(UnitConverter.InToPx(8.5f), UnitConverter.InToPx(11f));
        public static readonly float UNIT_SIZE = UnitConverter.ST_MM * 10; //1 cm, a common Tikz unit

        private readonly ToolActionData _info;

        public DrawingWindowRenderer(ToolActionData info) => _info = info;

        public void PaintScreen(PaintEventArgs e, float zoomAmt, Graph graph, Rectangle screen, SelectedTool currentTool, ToolSettingDictionary toolInfo)
        {
            Graph sub = graph.GetSubgraphTouchingPolygon(new() { _info.Corner, new(_info.Corner.X + screen.Width / zoomAmt, _info.Corner.Y),
                                                                  _info.Corner + new Coord(screen.Width, screen.Height) / zoomAmt, new(_info.Corner.X, _info.Corner.Y + screen.Height / zoomAmt) });

            if (_info.DrawUnitGrid)
            {
                for (float x = _info.Corner.X - _info.Corner.X % UNIT_SIZE; x <= _info.Corner.X + screen.X / zoomAmt; x += UNIT_SIZE)
                    e.Graphics.DrawLine(new Pen(new SolidBrush(UNIT_GRID_GUIDE_COLOR), UNIT_GRID_GUIDE_WIDTH), x - _info.Corner.X, 0, x - _info.Corner.X, screen.Y / zoomAmt);
                for (float y = _info.Corner.Y - _info.Corner.Y % UNIT_SIZE; y <= _info.Corner.Y + screen.Y / zoomAmt; y += UNIT_SIZE)
                    e.Graphics.DrawLine(new Pen(new SolidBrush(UNIT_GRID_GUIDE_COLOR), UNIT_GRID_GUIDE_WIDTH), 0, y - _info.Corner.Y, screen.X / zoomAmt, y - _info.Corner.Y);
            }
            if(_info.DrawBorder) //I'm too lazy to just draw the line segments that are actually visible
                e.Graphics.DrawRectangle(new Pen(new SolidBrush(BORDER_GUIDE_COLOR), BORDER_GUIDE_WIDTH), -_info.Corner.X, -_info.Corner.Y, PAGE_SIZE.Width, PAGE_SIZE.Height);

            Graph selectedSub = _info.Selected?.GetSubgraphWithin(_info.Corner, screen.Width, screen.Height);
            if (selectedSub != null && !selectedSub.IsEmpty())
            {
                foreach (Edge eg in selectedSub.ViewEdges())
                    DrawHighlightedEdge(e.Graphics, eg);

                foreach (Vertex v in selectedSub.ViewVertices())
                    DrawHighlightedVertex(e.Graphics, v, v.Offset - _info.Corner);
            }
            if(currentTool.Equals(SelectedTool.Merge) && _info.FirstVertex != null)
                DrawHighlightedVertex(e.Graphics, _info.FirstVertex, _info.FirstVertex.Offset - _info.Corner);

            if (!sub.IsEmpty())
                foreach (Edge eg in sub.ViewEdges())
                    DrawEdge(e.Graphics, eg);

            PreVertexPaint(e, zoomAmt, graph, screen, currentTool, toolInfo);

            if (!sub.IsEmpty())
                foreach (Vertex v in sub.ViewVertices())
                    DrawVertex(e.Graphics, v, v.Offset - _info.Corner);

            PostVertexPaint(e, zoomAmt, graph, screen, currentTool, toolInfo);
            e.Graphics.ResetTransform();
            e.Graphics.DrawString($"{zoomAmt}", new Font(FontFamily.GenericMonospace, 20), new SolidBrush(Color.Black), new PointF(0, 0));
            e.Graphics.DrawString($"{_info.MouseDrag}", new Font(FontFamily.GenericMonospace, 20), new SolidBrush(Color.Black), new PointF(0, 30));
        }

        private void PreVertexPaint(PaintEventArgs e, float zoomAmt, Graph graph, Rectangle screen, SelectedTool currentTool, ToolSettingDictionary toolInfo)
        {
            switch (currentTool)
            {
                case SelectedTool.Edge:
                    if (_info.Dragging && _info.FirstVertex != null)
                    {
                        EdgeToolInfo edge = toolInfo.EdgeInfo;
                        EdgeCapToolInfo edgeCap = toolInfo.EdgeCapInfo;
                        edge.Color = ACTION_HIGHLIGHT_COLOR;
                        edgeCap.Style = EdgeLineStyle.EdgeCapShape.None;
                        DrawEdge(e.Graphics, new Edge(edge, edgeCap, _info.FirstVertex, new Vertex(new() { Style = VertexBorderStyle.BorderStyle.None, FillColor = Color.Transparent }, _info.Corner + (Coord)_info.MouseDrag / zoomAmt)));
                    }
                    break;
                case SelectedTool.Select:
                    if (_info.Selected != null && _info.Dragging)
                        foreach (Edge eg in _info.Selected.ViewEdges())
                            DrawEdge(e.Graphics, eg);
                    break;
                case SelectedTool.Vertex:
                case SelectedTool.EdgeCap:
                case SelectedTool.Eraser:
                case SelectedTool.AreaSelect:
                case SelectedTool.Lasso:
                case SelectedTool.Shape:
                case SelectedTool.Merge:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        private void PostVertexPaint(PaintEventArgs e, float zoomAmt, Graph graph, Rectangle screen, SelectedTool currentTool, ToolSettingDictionary toolInfo)
        {
            switch (currentTool)
            {
                case SelectedTool.Vertex:
                    if (_info.MouseDrag.Equals(OFF_SCREEN))
                        break;

                    Coord mouseGraphPos = (Coord)_info.MouseDrag / zoomAmt + _info.Corner;
                    Coord aligned = FindVertexRepositioning(graph, _info, mouseGraphPos) - _info.Corner;

                    if (!aligned.Equals(mouseGraphPos) || graph.GetPointClosestTo(mouseGraphPos) != null && Coord.DistanceFrom(mouseGraphPos, graph.GetPointClosestTo(mouseGraphPos).Offset) <= MAX_ANGLE_LINK * UNIT_SIZE)
                        e.Graphics.FillEllipse(new SolidBrush(ACTION_HIGHLIGHT_COLOR), aligned.X - toolInfo.VertexInfo.Radius - toolInfo.VertexInfo.XRadius, aligned.Y - toolInfo.VertexInfo.Radius - toolInfo.VertexInfo.YRadius, 2 * toolInfo.VertexInfo.Radius + 2 * toolInfo.VertexInfo.XRadius, 2 * toolInfo.VertexInfo.Radius + 2 * toolInfo.VertexInfo.YRadius);
                    break;
                case SelectedTool.Edge:
                case SelectedTool.EdgeCap:
                case SelectedTool.Merge:
                    break;
                case SelectedTool.Eraser:
                    if (_info.Dragging)
                        e.Graphics.FillEllipse(new SolidBrush(ACTION_HIGHLIGHT_COLOR), ((Coord)_info.MouseDrag / zoomAmt).X - toolInfo.EraserInfo.Radius, ((Coord)_info.MouseDrag / zoomAmt).Y - toolInfo.EraserInfo.Radius, 2 * toolInfo.EraserInfo.Radius, 2 * toolInfo.EraserInfo.Radius);
                    break;
                case SelectedTool.Shape:
                    if (_info.Dragging)
                    {
                        Coord mousePoint;

                        if (toolInfo.ShapeInfo.CenterPoint)
                            mousePoint = FindVertexRepositioning(graph, _info, (Coord)_info.MouseDrag / zoomAmt, (Coord)_info.MouseDown / zoomAmt) * zoomAmt;
                        else
                            mousePoint = (FindVertexRepositioning(graph, _info, (Coord)_info.MouseDrag / zoomAmt + _info.Corner) - _info.Corner) * zoomAmt;

                        float startAngle = Coord.AngleBetween(mousePoint, _info.MouseDown);
                        float rotation = 2 * MathF.PI / toolInfo.ShapeInfo.Points;
                        float pointDistance = Coord.DistanceFrom(mousePoint, _info.MouseDown) / zoomAmt;
                        PointF[] points = Enumerable.Range(0, toolInfo.ShapeInfo.Points)
                                                    .Select(i => (Coord)_info.MouseDown / zoomAmt + pointDistance * Coord.AngleUnit(startAngle + i * rotation))
                                                    .Select(c => new PointF(c.X, c.Y))
                                                    .ToArray();
                        e.Graphics.FillPolygon(new SolidBrush(SELECTED_HIGHLIGHT_COLOR), points);
                        VertexToolInfo defaultVertex = toolInfo.VertexInfo;
                        defaultVertex.BorderColor = ACTION_HIGHLIGHT_COLOR;
                        defaultVertex.FillColor = ACTION_HIGHLIGHT_COLOR;
                        foreach (PointF p in points)
                            DrawVertex(e.Graphics, new Vertex(defaultVertex, p), p);
                        if (toolInfo.ShapeInfo.CenterPoint)
                            DrawVertex(e.Graphics, new Vertex(defaultVertex, _info.MouseDown), (Coord)_info.MouseDown / zoomAmt);
                        if (toolInfo.ShapeInfo.OuterRing)
                        {
                            EdgeToolInfo defaultEdge = toolInfo.EdgeInfo;
                            EdgeCapToolInfo defaultEdgeCap = toolInfo.EdgeCapInfo;
                            defaultEdgeCap.Style = EdgeLineStyle.EdgeCapShape.None;
                            defaultEdge.Color = ACTION_HIGHLIGHT_COLOR;

                            for (int i = 0; i < points.Length; i++)
                                DrawEdge(e.Graphics, new Edge(defaultEdge, defaultEdgeCap, new Vertex(defaultVertex, points[i] + _info.Corner), new Vertex(defaultVertex, points[(i + 1) % points.Length] + _info.Corner)));
                        }
                    }
                    break;
                case SelectedTool.Select:
                    if (_info.Selected != null &&_info.Dragging)
                        foreach (Vertex v in _info.Selected.ViewVertices())
                            DrawVertex(e.Graphics, v, v.Offset - _info.Corner);
                    break;
                case SelectedTool.AreaSelect:
                    if (_info.Dragging)
                    {
                        float xMin = MathF.Min(((Point)_info.MouseDown).X, _info.MouseDrag.X) / zoomAmt;
                        float yMin = MathF.Min(((Point)_info.MouseDown).Y, _info.MouseDrag.Y) / zoomAmt;
                        float xMax = MathF.Max(((Point)_info.MouseDown).X, _info.MouseDrag.X) / zoomAmt;
                        float yMax = MathF.Max(((Point)_info.MouseDown).Y, _info.MouseDrag.Y) / zoomAmt;

                        e.Graphics.FillRectangle(new SolidBrush(SELECTED_HIGHLIGHT_COLOR), xMin, yMin, xMax - xMin, yMax - yMin);
                    }
                    break;
                case SelectedTool.Lasso:
                    if (_info.LassoPoints.Count == 2)
                        e.Graphics.DrawLine(new Pen(new SolidBrush(SELECTED_HIGHLIGHT_COLOR)), _info.LassoPoints[0] - _info.Corner, _info.LassoPoints[1] - _info.Corner);
                    else if (_info.LassoPoints.Count >= 3)
                        e.Graphics.FillPolygon(new SolidBrush(SELECTED_HIGHLIGHT_COLOR), _info.LassoPoints.Append(_info.LassoPoints[0]).Select(p => (PointF)(p - _info.Corner)).ToArray());
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (currentTool.Equals(SelectedTool.Vertex) || (currentTool.Equals(SelectedTool.Shape) && _info.MouseDown != null))
            {
                Coord mouseGraphPos = _info.Corner + (Coord)_info.MouseDrag / zoomAmt;
                Coord center;
                if (currentTool.Equals(SelectedTool.Shape) && toolInfo.ShapeInfo.CenterPoint)
                    center = FindVertexRepositioning(graph, _info, mouseGraphPos, _info.Corner + (Coord)_info.MouseDown / zoomAmt);
                else
                    center = FindVertexRepositioning(graph, _info, mouseGraphPos);

                if (!center.Equals(mouseGraphPos) || graph.GetPointClosestTo(mouseGraphPos) != null && Coord.DistanceFrom(mouseGraphPos, graph.GetPointClosestTo(mouseGraphPos).Offset) <= MAX_ANGLE_LINK * UNIT_SIZE)
                {
                    Coord origin;
                    if (currentTool.Equals(SelectedTool.Shape) && toolInfo.ShapeInfo.CenterPoint)
                        origin = _info.Corner + (Coord)_info.MouseDown / zoomAmt;
                    else
                        origin = graph.GetSubgraphWithin(_info.Corner, screen.Width, screen.Height).GetPointClosestTo(mouseGraphPos)?.Offset;
                    Coord adjustedMousePos = (origin ?? center) - _info.Corner;

                    if (!_info.SnapToUnitGrid && _info.SnapToUnit && origin != null) //Unit snap rendering
                    {
                        for (float dist = 0; dist <= MAX_ANGLE_LINK * UNIT_SIZE; dist += UNIT_SIZE)
                            e.Graphics.DrawEllipse(new Pen(new SolidBrush(ACTION_HIGHLIGHT_COLOR)), adjustedMousePos.X - dist, adjustedMousePos.Y - dist, 2 * dist, 2 * dist);
                    }
                    if (!_info.SnapToUnitGrid && _info.SnapToAngle && origin != null) //Angle snap rendering
                    {
                        for (float sum = 0; sum <= 2 * MathF.PI; sum += _info.AngleSnapAmt)
                            e.Graphics.DrawLine(new Pen(new SolidBrush(ACTION_HIGHLIGHT_COLOR)), adjustedMousePos, adjustedMousePos + (MAX_ANGLE_LINK * UNIT_SIZE) * new Coord(MathF.Cos(sum), MathF.Sin(sum)));
                    }
                }
            }
        }

        private void DrawVertex(Graphics g, Vertex v, Coord pos) //TODO: Add support for other edge/vertex types, text in vertices, etc.
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
        private void DrawHighlightedVertex(Graphics g, Vertex v, Coord pos)
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
                    Coord start = e.GetSourceOffset() - _info.Corner;
                    Coord end = e.GetDestinationOffset() - _info.Corner;
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
                    Coord start = e.GetSourceOffset() - _info.Corner;
                    Coord end = e.GetDestinationOffset() - _info.Corner;
                    g.DrawLine(p, start.X, start.Y, end.X, end.Y);
                    break;
                default:
                    throw new NotImplementedException();
            }

            DrawHighlightedEdgeCaps(g, e);
        }

        private void DrawEdgeCaps(Graphics g, Edge e)
        {
            DrawSpecificEdgeCap(g, e.GetSourceOffset() - _info.Corner, Coord.AngleBetween(e.ViewDestination().Offset, e.ViewSource().Offset), e.Style.SDirectionCap, e.Style.LineInfo.Color, e.Style.LineInfo.Thickness, false);
            DrawSpecificEdgeCap(g, e.GetDestinationOffset() - _info.Corner, Coord.AngleBetween(e.ViewSource().Offset, e.ViewDestination().Offset), e.Style.DDirectionCap, e.Style.LineInfo.Color, e.Style.LineInfo.Thickness, false);
        }
        private void DrawHighlightedEdgeCaps(Graphics g, Edge e)
        {
            DrawSpecificEdgeCap(g, e.GetSourceOffset() - _info.Corner, Coord.AngleBetween(e.ViewDestination().Offset, e.ViewSource().Offset), e.Style.SDirectionCap, SELECTED_HIGHLIGHT_COLOR, e.Style.LineInfo.Thickness, true);
            DrawSpecificEdgeCap(g, e.GetDestinationOffset() - _info.Corner, Coord.AngleBetween(e.ViewSource().Offset, e.ViewDestination().Offset), e.Style.DDirectionCap, SELECTED_HIGHLIGHT_COLOR, e.Style.LineInfo.Thickness, true);
        }
        //TODO: Modify drawing edge caps to properly follow dimensions of PGF arrow tips
        private void DrawSpecificEdgeCap(Graphics g, Coord capEdge, float pointDirection, EdgeCapToolInfo style, Color color, float lineWidth, bool isHighlight)
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
                    if (isHighlight)
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
    }
}
