using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TikzGraphGen.GraphData;
using static TikzGraphGen.GraphData.GraphEditData;

namespace TikzGraphGen
{
    //TODO: bend support, shade support, custom path pattern support
    //TODO: Decoration/Shapes/Snake/Arrows, maybe Patterns/Mindmap library support
    //TODO: Option to attempt to planarize graph as much as possible/attempt to create symmetry in graph
    //TODO: Saving to file and loading, copy/paste/save tikz output, UI, display output as actual tikz pdf
    //TODO: Convert Tikz output back into code
    //TODO: Add layer system
    public struct GraphInfo
    {
        public Color defaultBGColor;
        public VertexBorderStyle defaultBorders;
        public EdgeLineStyle defaultLines;
    }

    public class Graph
    {
        public static readonly int MAX_HISTORY = 500;

        public bool UpdateFlag { get; private set; } //TODO: Set this so that it is only true when the graph has values changed

        public Color BGColor { get; set; }
        private GraphInfo _info;
        public GraphInfo Info { get { return _info; } }
        private readonly HashSet<Vertex> _vertices;
        private readonly HashSet<Edge> _edges;

        private readonly GraphEditData[] _history;
        private int _historyPos;

        public Graph(GraphInfo defaultSettings)
        {
            UpdateFlag = false;
            _info = defaultSettings;
            BGColor = _info.defaultBGColor;
            _vertices = new HashSet<Vertex>();
            _edges = new HashSet<Edge>();

            _history = new GraphEditData[MAX_HISTORY];
            _historyPos = MAX_HISTORY;
        }

        public bool IsEmpty()
        {
            return _vertices.Count + _edges.Count == 0;
        }

        public Vertex CreateVertex(Coord position, bool undoing = false) //TODO: For this and AddVertex, adjust all vertices if necessary so that resulting graph is contained within (0, 0) to (x, y), with the minimal X and Y of any vertex being 0. Also needs to adjust DrawingWindow position so that graph stays in same position
        {
            Vertex output = new(_info, position);
            _vertices.Add(output);

            if (!undoing)
                AddHistoryUpdate(new VertexEditData(output, EditDataQuantifier.Add));

            return output;
        }
        public void AddVertex(Vertex toAdd, bool undoing = false)
        {
            _vertices.Add(toAdd);
            if (!undoing)
                AddHistoryUpdate(new VertexEditData(toAdd, EditDataQuantifier.Add));
        }

        public List<Vertex> ViewVertices()
        {
            return _vertices.ToList();
        }
        public Edge CreateEdge(Vertex from, Vertex to, bool undoing = false)
        {
            Edge output = new(_info, from, to);
            _edges.Add(output);
            from.Connect(output);
            to.Connect(output);
            output.Connect(from, to);

            if (!undoing)
                AddHistoryUpdate(new EdgeEditData(output, EditDataQuantifier.Add));

            return output;
        }
        public void AddEdge(Vertex from, Vertex to, Edge toAdd, bool undoing = false)
        {
            _edges.Add(toAdd);
            from.Connect(toAdd);
            to.Connect(toAdd);
            toAdd.Connect(from, to);
            if (!undoing)
                AddHistoryUpdate(new EdgeEditData(toAdd, EditDataQuantifier.Add));
        }
        public List<Edge> ViewEdges()
        {
            return _edges.ToList();
        }

        /**
         * SHOULD ONLY BE USED IF .Connect was already called for the edge and the two incident vertices
         **/
        public void AddConnectedEdge(Edge toAdd, bool undoing = false)
        {
            _edges.Add(toAdd);
            if (!undoing)
                AddHistoryUpdate(new EdgeEditData(toAdd, EditDataQuantifier.Add));
        }

        public void RemoveVertex(Vertex toRemove, bool undoing = false)
        {
            Graph removedSub = new(_info);

            _vertices.Remove(toRemove);
            if (!undoing)
                removedSub.AddVertex(toRemove);
            foreach(Edge e in _edges.Where(e => e.IsIncidentTo(toRemove)))
            {
                _edges.Remove(e);
                _vertices.First(v => v.IsIncidentTo(e)).Disconnect(e);
                if (!undoing)
                    removedSub.AddConnectedEdge(e);
            }

            if(!undoing)
                AddHistoryUpdate(new SubgraphEditData(removedSub, EditDataQuantifier.Remove));
        }
        public void RemoveEdge(Edge toRemove, bool undoing = false)
        {
            _edges.Remove(toRemove);
            toRemove.Disconnect();
            if (!undoing)
                AddHistoryUpdate(new EdgeEditData(toRemove, EditDataQuantifier.Remove));
        }

        private void AddHistoryUpdate(GraphEditData data)
        {
            _historyPos--;

            if (CanRedo())
            {
                for (int i = 0; i < _historyPos; i++)
                    _history[i] = null;
            }

            if (_historyPos <= 0)
            {
                _history[MAX_HISTORY - 1] = null;
                for (int i = MAX_HISTORY - 1; i >= 0; i--)
                    _history[i] = _history[i - 1];
            }

            _history[_historyPos] = data;
        }

        public static Graph Undo(Graph g)
        {
            if (g.CanUndo())
            {
                g._historyPos++;
                return g._history[g._historyPos].UndoEdit(g);
            }

            return g;
        }

        public static Graph Redo(Graph g)
        {
            if (g.CanRedo())
            {
                g._historyPos--;
                return g._history[g._historyPos].RedoEdit(g);
            }

            return g;
        }

        public bool CanUndo()
        {
            return _historyPos < MAX_HISTORY - 1;
        }
        public bool CanRedo()
        {
            return _historyPos > 0 && _history[_historyPos - 1] != null;
        }

        public void ClearEditHistory()
        {
            for (int i = 0; i < MAX_HISTORY; i++)
                _history[i] = null;

            _historyPos = MAX_HISTORY;
        }

        public void RemoveSubgraph(Graph subgraph, bool undoing = false)
        {
            foreach(Edge e in subgraph._edges)
                RemoveEdge(e);

            foreach (Vertex v in subgraph._vertices)
                RemoveVertex(v);

            if (!undoing)
                AddHistoryUpdate(new SubgraphEditData(subgraph, EditDataQuantifier.Remove));
        }
        public void AddSubgraph(Graph subgraph, bool undoing = false)
        {
            foreach (Vertex v in subgraph._vertices)
                AddVertex(v);

            foreach (Edge e in subgraph._edges) //TODO: option to either connect to old vertex, create new vertex using default settings, or delete edges. Default for now is connecting to old
                //Should change to have default being creating a new vertex
                AddConnectedEdge(e);

            if (!undoing)
                AddHistoryUpdate(new SubgraphEditData(subgraph, EditDataQuantifier.Add));
        }

        public void AddSubgraph(Graph subgraph, Coord offset, bool undoing = false)
        {
            Graph newSubgraph = new(subgraph._info);
            foreach(Vertex v in subgraph.ViewVertices())
            {
                v.Offset += offset;
                _vertices.Add(v);
                newSubgraph.AddVertex(v, true);
            }

            foreach (Edge e in subgraph.ViewEdges())
            {
                _edges.Add(e);
                newSubgraph.AddConnectedEdge(e, true);
            }

            if (!undoing)
                AddHistoryUpdate(new SubgraphEditData(newSubgraph, EditDataQuantifier.Add));
        }

        /**
         * Gets outer bound. Minimal bounds are x = 0 and y = 0
         **/
        public Coord GetBounds()
        {
            float xMax = _vertices.Select(v => v.Offset.X + v.Style.Radius + (v.Style.OblongWidth / 2)).Max();
            float yMax = _vertices.Select(v => v.Offset.Y + v.Style.Radius + (v.Style.OblongHeight / 2)).Max();
            return new Coord(xMax, yMax);
        }

        public Graph GetSubgraphWithin(Coord visibleCorner, float width, float height)
        {
            Graph output = new(_info);
            foreach (Vertex v in _vertices)
            {
                if (v.Offset.X - v.Style.Radius - (v.Style.OblongWidth / 2) >= visibleCorner.X &&
                   v.Offset.Y - v.Style.Radius - (v.Style.OblongHeight / 2) >= visibleCorner.Y &&
                   v.Offset.X + v.Style.Radius + (v.Style.OblongWidth / 2) <= visibleCorner.X + width &&
                   v.Offset.Y + v.Style.Radius + (v.Style.OblongWidth / 2) <= visibleCorner.Y + height)
                {
                    output.AddVertex(v, true);
                    v.ViewEdges().Distinct().ToList().ForEach(e => output.AddConnectedEdge(e, true)); //TODO: Account for case where vertex is on edge of area, and edge goes further outwards so it isn't in area
                }       
            }

            return output;
        }
        public Graph GetSubgraphTouchingCircle(Coord center, float radius)
        {
            Graph output = new(_info);
            foreach (Vertex v in _vertices)
            {
                bool touching = false;
                switch(v.Style.Style)
                {
                    case VertexBorderStyle.BorderStyle.Circle:
                    case VertexBorderStyle.BorderStyle.CircleSplit:
                    case VertexBorderStyle.BorderStyle.NoSign:
                        touching = Math.Sqrt(Math.Pow(center.X - v.Offset.X, 2) + Math.Pow(center.Y - v.Offset.Y, 2)) <= radius + v.Style.Radius;
                        break;
                    case VertexBorderStyle.BorderStyle.Ellipse:
                        //Edge Intersection
                        float ar = v.Style.OblongWidth, br = v.Style.OblongHeight;
                        float a = (br*br) / (2 * v.Offset.Y * ar*ar) - 1 / (2 * v.Offset.Y);
                        float b = (br*br * v.Offset.X) / (ar*ar * v.Offset.Y);
                        float c = (br*br * v.Offset.X*v.Offset.X) / (2 * ar*ar * v.Offset.Y) + (radius*radius) / (2 * v.Offset.Y) + v.Offset.Y / 2 - (br*br) / (2 * v.Offset.Y);
                        touching = HasRealRoots(a*a, 2 * a * b, b*b + 1, 2 * b * c, c*c - radius*radius);
                        //One shape entirely contains other
                        touching |= Coord.DistanceFrom(center, v.Offset) <= radius;
                        float angle = Coord.AngleBetween(v.Offset, center);
                        touching |= Coord.DistanceFrom(v.Offset, center) <= ar * br / MathF.Sqrt(ar*ar*MathF.Pow(MathF.Sin(angle), 2) + br*br*MathF.Pow(MathF.Cos(angle), 2));
                        break;
                    case VertexBorderStyle.BorderStyle.Rectangle:
                        touching = (v.Offset.X + (v.Style.OblongWidth / 2) >= center.X - radius) &&
                                   (v.Offset.X - (v.Style.OblongWidth / 2) <= center.X + radius) &&
                                   (v.Offset.Y + (v.Style.OblongHeight / 2) >= center.Y - radius) &&
                                   (v.Offset.Y - (v.Style.OblongHeight / 2) <= center.Y + radius);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                if(touching)
                {
                    output.AddVertex(v, true);
                    v.ViewEdges().Distinct().ToList().ForEach(e => output.AddConnectedEdge(e, true)); //TODO: Account for case where vertex is on edge of area, and edge goes further outwards so it isn't in area
                }
            }

            return output;
        }

        private static bool HasRealRoots(float a, float b, float c, float d, float e)
        {
            float s1 = 2 * c * c * c - 9 * b * c * d + 27 * (a * d * d + b * b * e) - 72 * a * c * e,
                  q1 = c * c - 3 * b * d + 12 * a * e,
                  discrim1 = -4 * q1 * q1 * q1 + s1 * s1;
            if (discrim1 > 0)
            {
                float s2 = s1 + MathF.Sqrt(discrim1),
                      q2 = MathF.Pow(s2 / 2, 1/3f),
                      s3 = q1 / (3 * a * q2) + q2 / (3 * a),
                      discrim2 = (b * b) / (4 * a * a) - (2 * c) / (3 * a) + s3;
                if (discrim2 > 0)
                {
                    float s5 = (b * b) / (2 * a * a) - (4 * c) / (3 * a) - s3,
                          s6 = (-(b * b * b) / (a * a * a) + (4 * b * c) / (a * a) - (8 * d) / a) / (4 * MathF.Sqrt(discrim2)),
                          discrim3 = s5 - s6,
                          discrim4 = s5 + s6;
                    if (discrim3 < 0 && discrim4 < 0)
                        return false;

                    return false;
                }
            }

            return false;
        }
            public Vertex GetPointClosestTo(Coord pos)
        {
            if (_vertices.Count == 0)
                return null;

            return _vertices.Aggregate((a, b) => Coord.DistanceFrom(pos, a.Offset) <= Coord.DistanceFrom(pos, b.Offset) ? a : b);
        }

        public override string ToString()
        {
            return $"Vertices: {_vertices.Count} | Edges: {_edges.Count}\nConnections:\n {_edges.Aggregate("", (e, s) => $"{s}\n{e}")}";
        }
    }
}