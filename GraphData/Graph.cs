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

        public int vertexCount;
        public int edgeCount;
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

        public void CreateVertex(Coord position, bool undoing = false) //TODO: For this and AddVertex, adjust all vertices if necessary so that resulting graph is contained within (0, 0) to (x, y), with the minimal X and Y of any vertex being 0. Also needs to adjust DrawingWindow position so that graph stays in same position
        {
            Vertex output = new(_info, position);
            _vertices.Add(output);
            _info.vertexCount++;
            if (!undoing)
                AddHistoryUpdate(new VertexEditData(output, EditDataQuantifier.Add));
        }
        public void AddVertex(Vertex toAdd, bool undoing = false)
        {
            _vertices.Add(toAdd);
            _info.vertexCount++;
            if (!undoing)
                AddHistoryUpdate(new VertexEditData(toAdd, EditDataQuantifier.Add));
        }

        public List<Vertex> ViewVertices()
        {
            return _vertices.ToList();
        }
        public void CreateEdge(Vertex from, Vertex to, bool undoing = false)
        {
            Edge output = new(_info, from, to);
            _edges.Add(output);
            from.Connect(output);
            to.Connect(output);
            output.Connect(from, to);
            _info.edgeCount++;
            if (!undoing)
                AddHistoryUpdate(new EdgeEditData(output, EditDataQuantifier.Add));
        }
        public void AddEdge(Vertex from, Vertex to, Edge toAdd, bool undoing = false)
        {
            _edges.Add(toAdd);
            from.Connect(toAdd);
            to.Connect(toAdd);
            toAdd.Connect(from, to);
            _info.edgeCount++;
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
            _info.edgeCount++;
            if (!undoing)
                AddHistoryUpdate(new EdgeEditData(toAdd, EditDataQuantifier.Add));
        }

        public void RemoveVertex(Vertex toRemove, bool undoing = false)
        {
            Graph removedSub = new(_info);

            _vertices.Remove(toRemove);
            _info.vertexCount--;
            if (!undoing)
                removedSub.AddVertex(toRemove);
            foreach(Edge e in _edges.Where(e => e.IsIncidentTo(toRemove)))
            {
                _edges.Remove(e);
                _vertices.First(v => v.IsIncidentTo(e)).Disconnect(e);
                _info.edgeCount--;
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
            _info.edgeCount--;
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

        public override string ToString()
        {
            return $"Vertices: {_info.vertexCount} | Edges: {_info.edgeCount}\nConnections: {_edges.Aggregate("", (e, s) => $"{s}\n{e}")}";
        }
    }
}