using System;
using System.Collections.Generic;
using System.Linq;
using TikzGraphGen.GraphData;
using static TikzGraphGen.ToolSettingDictionary;

namespace TikzGraphGen
{
    //TODO: bend support, shade support, custom path pattern support
    //TODO: Decoration/Shapes/Snake/Arrows, maybe Patterns/Mindmap library support
    //TODO: Option to attempt to planarize graph as much as possible/attempt to create symmetry in graph
    //TODO: Saving to file and loading, copy/paste/save tikz output, UI, display output as actual tikz pdf
    //TODO: Convert Tikz output back into code
    //TODO: Add layer system

    public class Graph //TODO: Reimplement history
    {
        public static readonly int MAX_HISTORY = 500;

        public bool UpdateFlag { get; private set; } //TODO: Set this so that it is only true when the graph has values changed

        private readonly HashSet<Vertex> _vertices;
        private readonly HashSet<Edge> _edges;

        public Graph()
        {
            UpdateFlag = false;
            _vertices = new HashSet<Vertex>();
            _edges = new HashSet<Edge>();
        }

        public bool IsEmpty()
        {
            return _vertices.Count + _edges.Count == 0;
        }

        public Vertex CreateVertex(Coord position, VertexToolInfo settings, bool undoing = false) //TODO: For this and AddVertex, adjust all vertices if necessary so that resulting graph is contained within (0, 0) to (x, y), with the minimal X and Y of any vertex being 0. Also needs to adjust DrawingWindow position so that graph stays in same position
        {
            Vertex output = new(settings, position);
            _vertices.Add(output);

            return output;
        }
        public void AddVertex(Vertex toAdd, bool undoing = false)
        {
            _vertices.Add(toAdd);
        }

        public List<Vertex> ViewVertices()
        {
            return _vertices.ToList();
        }
        public Edge CreateEdge(Vertex from, Vertex to, EdgeToolInfo edgeSettings, EdgeCapToolInfo capSettings, bool undoing = false)
        {
            capSettings.Style = EdgeLineStyle.EdgeCapShape.None;
            Edge output = new(edgeSettings, capSettings, from, to);
            _edges.Add(output);
            from.Connect(output);
            to.Connect(output);
            output.Connect(from, to);

            return output;
        }
        public Edge CreateEdge(Vertex from, Vertex to, EdgeToolInfo edgeSettings, EdgeCapToolInfo sourceCapSettings, EdgeCapToolInfo destinationCapSettings, bool undoing = false)
        {
            Edge output = new(edgeSettings, sourceCapSettings, destinationCapSettings, from, to);
            _edges.Add(output);
            from.Connect(output);
            to.Connect(output);
            output.Connect(from, to);

            return output;
        }
        public void AddEdge(Vertex from, Vertex to, Edge toAdd, bool undoing = false)
        {
            _edges.Add(toAdd);
            from.Connect(toAdd);
            to.Connect(toAdd);
            toAdd.Connect(from, to);
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
        }

        public void RemoveVertex(Vertex toRemove, bool undoing = false)
        {
            _vertices.Remove(toRemove);

            foreach(Edge e in _edges.Where(e => e.IsIncidentTo(toRemove)))
            {
                _edges.Remove(e);
                _vertices.First(v => v.IsIncidentTo(e)).Disconnect(e);
            }
        }
        public void DeleteVertex(Vertex toDelete, bool undoing = false)
        {
            _vertices.Remove(toDelete);

            foreach (Edge e in _edges.Where(e => e.IsIncidentTo(toDelete)))
                _edges.Remove(e);
        }
        public void RemoveEdge(Edge toRemove, bool undoing = false)
        {
            _edges.Remove(toRemove);
        }
        public void DeleteEdge(Edge toDelete, bool undoing = false)
        {
            _edges.Remove(toDelete);
            toDelete.Disconnect();
        }

        public static Graph Undo(Graph g)
        {
            /*if (g.CanUndo())
            {
                g._historyPos++;
                return g._history[g._historyPos].UndoEdit(g);
            }
            */
            return g;
        }

        public static Graph Redo(Graph g)
        {
            /*if (g.CanRedo())
            {
                g._historyPos--;
                return g._history[g._historyPos].RedoEdit(g);
            }
            */
            return g;
        }

        public void RemoveSubgraph(Graph subgraph, bool undoing = false)
        {
            foreach(Edge e in subgraph._edges)
                RemoveEdge(e);

            foreach (Vertex v in subgraph._vertices)
                RemoveVertex(v);
        }
        public void DeleteSubgraph(Graph subgraph, bool undoing = false)
        {
            foreach (Edge e in subgraph._edges)
                DeleteEdge(e);

            foreach (Vertex v in subgraph._vertices)
                DeleteVertex(v);
        }
        public void AddSubgraph(Graph subgraph, bool undoing = false)
        {
            foreach (Vertex v in subgraph._vertices)
                AddVertex(v);

            foreach (Edge e in subgraph._edges) //TODO: option to either connect to old vertex, create new vertex using default settings, or delete edges. Default for now is connecting to old
                //Should change to have default being creating a new vertex
                AddConnectedEdge(e);
        }

        public void AddSubgraph(Graph subgraph, Coord offset, bool undoing = false)
        {
            Graph newSubgraph = new();
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
        }

        /**
         * Gets bounds. I know, very helpful.
         **/
        public (Coord, Coord) GetBounds()
        {
            float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
            foreach(Vertex v in _vertices)
            {
                xMin = MathF.Min(xMin, v.Offset.X - v.GetAngularRadius(MathF.PI));
                yMin = MathF.Min(yMin, v.Offset.Y - v.GetAngularRadius(MathF.PI / 2f));
                xMax = MathF.Max(xMax, v.Offset.X + v.GetAngularRadius(0));
                yMax = MathF.Max(yMax, v.Offset.Y + v.GetAngularRadius(MathF.PI * 3 / 2f));
            }
            return (new(xMin, yMin), new(xMax, yMax));
        }

        public Graph GetSubgraphWithin(Coord visibleCorner, float width, float height)
        {
            Graph output = new();
            foreach (Vertex v in _vertices)
            {
                if (v.Offset.X - v.GetAngularRadius(MathF.PI) >= visibleCorner.X &&
                   v.Offset.Y - v.GetAngularRadius(MathF.PI / 2f) >= visibleCorner.Y &&
                   v.Offset.X + v.GetAngularRadius(0) <= visibleCorner.X + width &&
                   v.Offset.Y + v.GetAngularRadius(MathF.PI * 3 / 2f) <= visibleCorner.Y + height)
                {
                    output.AddVertex(v, true);
                    v.ViewEdges().Where(e => !output.ViewEdges().Contains(e)).ToList().ForEach(e => output.AddConnectedEdge(e, true));
                }       
            }

            return output;
        }

        public Graph GetSubgraphTouchingCircle(Coord center, float radius)
        {
            Graph output = new();
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
                        float ar = v.Style.XRadius, br = v.Style.YRadius;
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
                        touching = (v.Offset.X + (v.Style.XRadius / 2) >= center.X - radius) &&
                                   (v.Offset.X - (v.Style.XRadius / 2) <= center.X + radius) &&
                                   (v.Offset.Y + (v.Style.YRadius / 2) >= center.Y - radius) &&
                                   (v.Offset.Y - (v.Style.YRadius / 2) <= center.Y + radius);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                if(touching)
                {
                    output.AddVertex(v, true);
                    v.ViewEdges().Distinct().ToList().ForEach(e => output.AddConnectedEdge(e, true));
                }
            }

            foreach(Edge e in _edges.Except(output.ViewEdges()))
            {
                Coord start = e.ViewSource().Offset;
                Coord end = e.ViewDestination().Offset;

                float ax = start.X - center.X, ay = start.Y - center.Y;
                float bx = end.X - center.X, by = end.Y - center.Y;
                float a = (bx - ax)*(bx - ax) + (by - ay)*(by - ay);
                float b = 2 * (ax * (bx - ax) + ay * (by - ay));
                float disc = b*b - 4 * a * (ax * ax + ay * ay - radius * radius);
                if (disc > 0)
                {
                    float t1 = (-b + MathF.Sqrt(disc)) / (2 * a);
                    float t2 = (-b - MathF.Sqrt(disc)) / (2 * a);
                    if ((0 < t1 && t1 < 1) || (0 < t2 && t2 < 1))
                        output.AddConnectedEdge(e, true);
                }
            }

            return output;
        }
        /**
         * Polygon should start and end with the same coordinates
         **/
        public Graph GetSubgraphTouchingPolygon(List<Coord> pts)
        {
            Graph output = new();
            IEnumerable<Vertex> vts = _vertices.Where(v => IsInPolygon(pts, v.Offset));
            foreach (Vertex v in vts)
            {
                output.AddVertex(v, true);
                v.ViewEdges().Where(e => !output.ViewEdges().Contains(e)).ToList().ForEach(e => output.AddConnectedEdge(e, true));
            }

            return output;
        }

        private static bool IsInPolygon(List<Coord> pts, Coord c)
        {
            (float Min, float Max) xRange = (pts.First().X, pts.First().X);
            (float Min, float Max) yRange = (pts.First().Y, pts.First().Y);
            foreach(Coord pt in pts)
            {
                xRange = (MathF.Min(xRange.Min, pt.X), MathF.Max(xRange.Max, pt.X));
                yRange = (MathF.Min(yRange.Min, pt.Y), MathF.Max(yRange.Max, pt.Y));
            }

            if (c.X < xRange.Min || c.X > xRange.Max || c.Y < yRange.Min || c.Y > yRange.Max)
                return false;

            int sum = 0;

            for (int i = 0; i < pts.Count - 1; i++) //pts ends with pts[0], so going to count - 2 will include every edge
            {
                if (Math.Max(pts[i].X, pts[i + 1].X) < c.X || Math.Min(pts[i].Y, pts[i + 1].Y) > c.Y || Math.Max(pts[i].Y, pts[i + 1].Y) < c.Y)
                    continue;
                if (pts[i + 1].X - pts[i].X == 0) //Vertical lines
                {
                    sum++;
                    continue;
                }

                float slope = (pts[i + 1].Y - pts[i].Y) / (pts[i + 1].X - pts[i].X);
                float xPt = (c.Y - pts[i].Y) / slope + pts[i].X;
                sum += (Math.Min(pts[i].X, pts[i + 1].X) <= xPt && xPt <= Math.Max(pts[i].X, pts[i + 1].X)) ? 1 : 0;
            }
            return sum % 2 == 1;
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

        public void Translate(Coord amt, bool undoing = false)
        {
            Vertex[] temp = new Vertex[_vertices.Count];
            _vertices.CopyTo(temp);
            _vertices.ToList().ForEach(v => v.Offset += amt);
        }

        public override string ToString()
        {
            return $"Vertices: {_vertices.Count} | Edges: {_edges.Count}\nConnections:\n {_edges.Aggregate("", (e, s) => $"{s}\n{e}")}";
        }
    }
}