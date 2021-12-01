using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TikzGraphGen.GraphData;
using static TikzGraphGen.Visualization.TikzDrawingWindow;

namespace TikzGraphGen.Visualization
{
    public class GraphicalInteractionManager
    {
        private readonly ToolActionData _info;
        private readonly Action _refresh;
        private readonly Action<bool> _modifyToolPermission;

        public GraphicalInteractionManager(ToolActionData info, Action refresh, Action<bool> modifyToolPermission)
        {
            _info = info;
            _refresh = refresh;
            _modifyToolPermission = modifyToolPermission;
        }

        public void Click(MouseEventArgs e, float zoomAmt, Graph graph, Rectangle screen, SelectedTool currentTool, ToolSettingDictionary toolInfo)
        {
            Coord mousePos = _info.Corner + (Coord)e.Location / zoomAmt;

            switch (currentTool)
            {
                case SelectedTool.Vertex:
                    if (!_info.Dragging)
                        graph.CreateVertex(FindVertexRepositioning(graph, _info, mousePos), toolInfo.VertexInfo);
                    break;
                case SelectedTool.Edge:
                    if (_info.FirstVertex == null)
                        _info.FirstVertex = GetVerticesInCircle(graph, mousePos).FirstOrDefault();
                    else
                    {
                        Vertex _secondVertex = GetVerticesInCircle(graph, mousePos).FirstOrDefault();
                        if (_info.FirstVertex != null && _secondVertex != null && _info.FirstVertex != _secondVertex && !_info.FirstVertex.IsAdjacentTo(_secondVertex)) //TODO: Make support loops & multiedges later (give warning or automatically curve lines) (A -> A)
                            graph.CreateEdge(_info.FirstVertex, _secondVertex, toolInfo.EdgeInfo, toolInfo.EdgeCapInfo);

                        _info.FirstVertex = null;
                    }
                    break;
                case SelectedTool.EdgeCap:
                    List<Edge> edges = graph.GetSubgraphTouchingCircle(mousePos, toolInfo.EraserInfo.Radius)?.ViewEdges();
                    if (edges.Count > 0)
                    {
                        Edge ed = edges.Aggregate((close, e) => {
                            float sourceDist = Coord.DistanceFrom(e.GetSourceOffset(), mousePos);
                            float destinationDist = Coord.DistanceFrom(e.GetDestinationOffset(), mousePos);
                            float sourceDistR = Coord.DistanceFrom(close.GetSourceOffset(), mousePos);
                            float destinationDistR = Coord.DistanceFrom(close.GetDestinationOffset(), mousePos);
                            return (sourceDist <= sourceDistR && sourceDist <= destinationDistR) || (destinationDist <= sourceDistR && destinationDist <= destinationDistR) ? e : close;
                        });
                        if (Coord.DistanceFrom(ed.GetSourceOffset(), mousePos) <= Coord.DistanceFrom(ed.GetDestinationOffset(), mousePos))
                            ed.Style.SDirectionCap = toolInfo.EdgeCapInfo;
                        else
                            ed.Style.DDirectionCap = toolInfo.EdgeCapInfo;
                    }
                    break;
                case SelectedTool.Eraser:
                    Graph sub = graph.GetSubgraphTouchingCircle(mousePos, 1); //Very small eraser to try and only erase what is directly clicked on
                    if (sub.ViewEdges().Count > 0 || sub.ViewVertices().Count > 0)
                        graph.DeleteSubgraph(sub);
                    break;
                case SelectedTool.Select:
                    if (!_info.Dragging)
                        _info.Selected = graph.GetSubgraphTouchingCircle(mousePos, 1); //Very small selector to try and only select what is directly clicked on
                    break;
                case SelectedTool.AreaSelect:
                case SelectedTool.Shape:
                    break;
                case SelectedTool.Lasso:
                    if (!_info.Dragging)
                    {
                        if (_info.LassoPoints.Count > 1 && (Coord.DistanceFrom(mousePos, _info.LassoPoints[0]) < toolInfo.EraserInfo.Radius || Coord.DistanceFrom(mousePos, _info.LassoPoints[^1]) < DRAG_SENSITIVITY))
                        {
                            _info.Selected = graph.GetSubgraphTouchingPolygon(_info.LassoPoints.Append(_info.LassoPoints[0]).Select(p => (Coord)p).ToList());
                            _info.LassoPoints.Clear();
                        }
                        else
                            _info.LassoPoints.Add(mousePos);
                    }
                    break;
                case SelectedTool.Merge:
                    if (_info.FirstVertex == null)
                        _info.FirstVertex = GetVerticesInCircle(graph, mousePos).FirstOrDefault();
                    else
                    {
                        Vertex _secondVertex = GetVerticesInCircle(graph, mousePos).FirstOrDefault();
                        if (_info.FirstVertex != null && _secondVertex != null && _info.FirstVertex != _secondVertex)
                        {
                            graph.RemoveVertex(_info.FirstVertex);
                            graph.RemoveVertex(_secondVertex);
                            Vertex result = graph.CreateVertex(_info.FirstVertex.Offset + (_secondVertex.Offset - _info.FirstVertex.Offset) / 2, _info.FirstVertex.Style);
                            foreach (Edge ed in _info.FirstVertex.ViewEdges())
                            {
                                if (ed.ViewDestination().Equals(_secondVertex) || ed.ViewSource().Equals(_secondVertex))
                                    break;
                                else if (ed.ViewDestination().Equals(_info.FirstVertex))
                                    graph.CreateEdge(ed.ViewSource(), result, ed.Style.LineInfo, ed.Style.SDirectionCap, ed.Style.DDirectionCap);
                                else
                                    graph.CreateEdge(result, ed.ViewDestination(), ed.Style.LineInfo, ed.Style.SDirectionCap, ed.Style.DDirectionCap);
                            }
                            foreach (Edge ed in _secondVertex.ViewEdges())
                            {
                                if (ed.ViewDestination().Equals(_info.FirstVertex) || ed.ViewSource().Equals(_info.FirstVertex))
                                    break;
                                else if (ed.ViewDestination().Equals(_secondVertex))
                                    graph.CreateEdge(ed.ViewSource(), result, ed.Style.LineInfo, ed.Style.SDirectionCap, ed.Style.DDirectionCap);
                                else
                                    graph.CreateEdge(result, ed.ViewDestination(), ed.Style.LineInfo, ed.Style.SDirectionCap, ed.Style.DDirectionCap);
                            }
                        }

                        _info.FirstVertex = null;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void MouseDown(object sender, MouseEventArgs e)
        {
            _info.MouseDown = e.Location;
            _info.MouseDrag = e.Location;
        }
        public void MouseMove(MouseEventArgs e, float zoomAmt, Graph graph, Rectangle screen, SelectedTool currentTool, ToolSettingDictionary toolInfo)
        {

            if (!_info.Dragging && currentTool.Equals(SelectedTool.Vertex) && (_info.SnapToUnit || _info.SnapToAngle || _info.SnapToUnitGrid))
                _refresh();

            if (_info.Dragging || (_info.MouseDown != null && Math.Sqrt(Math.Pow(e.Location.X - ((Point)_info.MouseDown).X, 2) + Math.Pow(e.Location.Y - ((Point)_info.MouseDown).Y, 2)) > DRAG_SENSITIVITY))
                DragEvent(e, zoomAmt, graph, screen, currentTool, toolInfo);

            _info.MouseDrag = e.Location;
        }

        private void DragEvent(MouseEventArgs e, float zoomAmt, Graph graph, Rectangle screen, SelectedTool currentTool, ToolSettingDictionary toolInfo)
        {
            Coord mousePos = _info.Corner + (Coord)e.Location / zoomAmt;

            switch (currentTool)
            {
                case SelectedTool.Vertex:
                case SelectedTool.EdgeCap:
                case SelectedTool.AreaSelect:
                case SelectedTool.Shape:
                case SelectedTool.Merge:
                    break;
                case SelectedTool.Edge:
                    _info.FirstVertex = GetVerticesInCircle(graph, _info.Corner + (Coord)_info.MouseDown / zoomAmt).FirstOrDefault();
                    if (_info.FirstVertex != null)
                        _modifyToolPermission(false);
                    break;
                case SelectedTool.Eraser:
                    Graph sub = graph.GetSubgraphTouchingCircle(mousePos, toolInfo.EraserInfo.Radius);
                    if (sub.ViewEdges().Count > 0 || sub.ViewVertices().Count > 0)
                        graph.DeleteSubgraph(sub);
                    break;
                case SelectedTool.Select:
                    if (!_info.Dragging) //Only sets subgraph when first moving mouse enough to start dragging (instead of every frame)
                    {
                        //Only sets subgraph if the selected subgraph is not being clicked
                        if (_info.Selected == null || _info.Selected.GetSubgraphTouchingCircle(_info.Corner + (Coord)_info.MouseDown / zoomAmt, 1).IsEmpty())
                            _info.Selected = graph.GetSubgraphTouchingCircle(_info.Corner + (Coord)_info.MouseDown / zoomAmt, 1); //Very small selector to try and only click what is directly clicked on

                        graph.RemoveSubgraph(_info.Selected, true);
                        _modifyToolPermission(false);
                    }
                    _info.Selected.Translate(((Coord)e.Location - _info.MouseDrag) / zoomAmt);
                    break;
                case SelectedTool.Lasso:
                    if (_info.Dragging)
                        _info.LassoPoints.RemoveAt(_info.LassoPoints.Count - 1);

                    if (_info.LassoPoints.Count > 0 && Coord.DistanceFrom(mousePos, _info.LassoPoints[0]) < toolInfo.EraserInfo.Radius)
                        _info.LassoPoints.Add(_info.LassoPoints[0]);
                    else if (_info.LassoPoints.Count == 0) //If first lasso movement is a mouse drag, instantly creates line rather than ignoring dragging part
                    {
                        _info.LassoPoints.Add(_info.Corner + (Coord)_info.MouseDown / zoomAmt);
                        _info.LassoPoints.Add(mousePos);
                    }
                    else
                        _info.LassoPoints.Add(mousePos);

                    _modifyToolPermission(false);
                    break;
                default:
                    throw new NotImplementedException();
            }

            _info.Dragging = true;
            _refresh();
        }

        public void MouseUp(MouseEventArgs e, float zoomAmt, Graph graph, Rectangle screen, SelectedTool currentTool, ToolSettingDictionary toolInfo)
        {
            switch (currentTool)
            {
                case SelectedTool.Vertex:
                case SelectedTool.EdgeCap:
                case SelectedTool.Eraser:
                case SelectedTool.Merge:
                    break;
                case SelectedTool.Edge:
                    Vertex _secondVertex = GetVerticesInCircle(graph, _info.Corner + e.Location).FirstOrDefault();

                    if (_info.FirstVertex != null && _secondVertex != null && _info.Dragging && _info.FirstVertex != _secondVertex && !_info.FirstVertex.IsAdjacentTo(_secondVertex)) //TODO: Make support loops & multiedges later (give warning or automatically curve lines) (A -> A)
                        graph.CreateEdge(_info.FirstVertex, _secondVertex, toolInfo.EdgeInfo, toolInfo.EdgeCapInfo);

                    _modifyToolPermission(true);
                    break;
                case SelectedTool.Shape:
                    if (_info.Dragging)
                    {
                        Coord rescaledPoint;
                        if (toolInfo.ShapeInfo.CenterPoint)
                            rescaledPoint = FindVertexRepositioning(graph, _info, (Coord)_info.MouseDrag / zoomAmt, (Coord)_info.MouseDown / zoomAmt) * zoomAmt;
                        else
                            rescaledPoint = FindVertexRepositioning(graph, _info, (Coord)_info.MouseDrag / zoomAmt) * zoomAmt;

                        float startAngle = Coord.AngleBetween(rescaledPoint, _info.MouseDown);
                        float rotation = 2 * MathF.PI / toolInfo.ShapeInfo.Points;
                        float pointDistance = Coord.DistanceFrom(rescaledPoint, _info.MouseDown) / zoomAmt;
                        IEnumerable<Coord> points = Enumerable.Range(0, toolInfo.ShapeInfo.Points)
                                                    .Select(i => (Coord)_info.MouseDown / zoomAmt + _info.Corner + pointDistance * Coord.AngleUnit(startAngle + i * rotation));

                        Graph sub = new();
                        foreach (Coord c in points)
                            sub.CreateVertex(c, toolInfo.VertexInfo, true);

                        if (toolInfo.ShapeInfo.OuterRing)
                        {
                            for (int i = 0; i < sub.ViewVertices().Count; i++)
                                sub.CreateEdge(sub.ViewVertices()[i], sub.ViewVertices()[(i + 1) % sub.ViewVertices().Count], toolInfo.EdgeInfo, toolInfo.EdgeCapInfo, true);
                        }
                        if (toolInfo.ShapeInfo.CenterPoint)
                            sub.CreateVertex((Coord)_info.MouseDown / zoomAmt + _info.Corner, toolInfo.VertexInfo, true);

                        graph.AddSubgraph(sub);
                    }
                    break;
                case SelectedTool.Select:
                    graph.AddSubgraph(_info.Selected, true);
                    _modifyToolPermission(true);
                    break;
                case SelectedTool.AreaSelect:
                    Coord downPos = _info.Corner + (Coord)_info.MouseDown / zoomAmt, dragPos = _info.Corner + (Coord)_info.MouseDrag / zoomAmt;
                    Coord tlCorner = new(MathF.Min(downPos.X, dragPos.X), MathF.Min(downPos.Y, dragPos.Y));
                    Coord brCorner = new(MathF.Max(downPos.X, dragPos.X), MathF.Max(downPos.Y, dragPos.Y));
                    _info.Selected = graph.GetSubgraphWithin(tlCorner, brCorner.X - tlCorner.X, brCorner.Y - tlCorner.Y);
                    break;
                case SelectedTool.Lasso:
                    if (_info.Dragging && _info.LassoPoints.Count > 2 && _info.LassoPoints[^1].Equals(_info.LassoPoints[0]))
                    {
                        _info.Selected = graph.GetSubgraphTouchingPolygon(_info.LassoPoints.Select(p => (Coord)p).ToList());
                        _info.LassoPoints.Clear();
                    }
                    _modifyToolPermission(true);
                    break;
                default:
                    throw new NotImplementedException();
            }
            _info.MouseDown = null;
            if (_info.Dragging)
                _info.FirstVertex = null;
            _info.Dragging = false;

            _refresh();
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