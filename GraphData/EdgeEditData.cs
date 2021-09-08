using System;
using System.Linq;

namespace TikzGraphGen.GraphData
{
    public class EdgeEditData : GraphEditData
    {
        private readonly Edge[] _edges;
        private readonly Edge[] _editedEdges;
        private readonly EditDataQuantifier _quantifiers;

        public EdgeEditData(Edge v, Edge ev)
        {
            _edges = new Edge[] { v };
            _editedEdges = new Edge[] { ev };
            _quantifiers = EditDataQuantifier.Edit;
        }
        public EdgeEditData(Edge v, EditDataQuantifier quantifiers)
        {
            _edges = new Edge[] { v };
            _editedEdges = new Edge[0];
            _quantifiers = quantifiers & ~EditDataQuantifier.Plural;
        }
        public EdgeEditData(Edge[] vs, Edge[] evs)
        {
            _edges = vs;
            _editedEdges = evs;
            _quantifiers = EditDataQuantifier.Edit | EditDataQuantifier.Plural;
        }
        public EdgeEditData(Edge[] vs, EditDataQuantifier quantifiers)
        {
            _edges = vs;
            _editedEdges = new Edge[0];
            _quantifiers = quantifiers & EditDataQuantifier.Plural;
        }

        public override string GetEditDescription()
        {
            switch (_quantifiers)
            {
                case EditDataQuantifier.Add:
                    if (_quantifiers.HasFlag(EditDataQuantifier.Plural))
                        return $"Add edges {_edges.Select(v => $"\"{v.Label}\"").Aggregate((a, b) => $"{a}, {b}")} to graph";
                    else
                        return $"Add edge {_edges[0].Label} to graph";
                case EditDataQuantifier.Remove:
                    if (_quantifiers.HasFlag(EditDataQuantifier.Plural))
                        return $"Remove edges {_edges.Select(v => $"\"{v.Label}\"").Aggregate((a, b) => $"{a}, {b}")} from graph";
                    else
                        return $"Remove edge {_edges[0].Label} from graph";
                case EditDataQuantifier.Edit:
                    if (_quantifiers.HasFlag(EditDataQuantifier.Plural))
                        return $"Edit edges {_edges.Select(v => $"\"{v.Label}\"").Aggregate((a, b) => $"{a}, {b}")}";
                    else
                        return $"Edit edge {_edges[0].Label}";
                default:
                    throw new NotImplementedException();
            }
        }

        public override Graph RedoEdit(Graph original)
        {
            switch (_quantifiers)
            {
                case EditDataQuantifier.Add:
                    foreach (Edge v in _edges)
                        original.AddConnectedEdge(v, true);
                    break;
                case EditDataQuantifier.Edit:
                    for (int i = 0; i < _edges.Length; i++)
                    {
                        original.AddConnectedEdge(_editedEdges[i], true);
                        original.RemoveEdge(_edges[i], true);
                    }
                    break;
                case EditDataQuantifier.Remove:
                    foreach (Edge v in _edges)
                        original.RemoveEdge(v, true);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return original;
        }

        public override Graph UndoEdit(Graph edited)
        {
            switch (_quantifiers)
            {
                case EditDataQuantifier.Add:
                    foreach (Edge v in _edges)
                        edited.RemoveEdge(v, true);
                    break;
                case EditDataQuantifier.Edit:
                    for (int i = 0; i < _edges.Length; i++)
                    {
                        edited.AddConnectedEdge(_edges[i], true);
                        edited.RemoveEdge(_editedEdges[i], true);
                    }
                    break;
                case EditDataQuantifier.Remove:
                    foreach (Edge v in _edges)
                        edited.AddConnectedEdge(v, true);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return edited;
        }
    }
}
