using System;
using System.Linq;

namespace TikzGraphGen.GraphData
{
    public class VertexEditData : GraphEditData
    {
        private readonly Vertex[] _vertices;
        private readonly Vertex[] _editedVertices;
        private readonly EditDataQuantifier _quantifiers;

        public VertexEditData(Vertex v, Vertex ev)
        {
            _vertices = new Vertex[] { v };
            _editedVertices = new Vertex[] { ev };
            _quantifiers = EditDataQuantifier.Edit;
        }
        public VertexEditData(Vertex v, EditDataQuantifier quantifiers)
        {
            _vertices = new Vertex[] { v };
            _editedVertices = new Vertex[0];
            _quantifiers = quantifiers & ~EditDataQuantifier.Plural;
        }
        public VertexEditData(Vertex[] vs, Vertex[] evs)
        {
            _vertices = vs;
            _editedVertices = evs;
            _quantifiers = EditDataQuantifier.Edit | EditDataQuantifier.Plural;
        }
        public VertexEditData(Vertex[] vs, EditDataQuantifier quantifiers)
        {
            _vertices = vs;
            _editedVertices = new Vertex[0];
            _quantifiers = quantifiers & EditDataQuantifier.Plural;
        }

        public override string GetEditDescription()
        {
            switch (_quantifiers)
            {
                case EditDataQuantifier.Add:
                    if (_quantifiers.HasFlag(EditDataQuantifier.Plural))
                        return $"Add vertices {_vertices.Select(v => $"\"{v.Label}\"").Aggregate((a, b) => $"{a}, {b}")} to graph";
                    else
                        return $"Add vertex {_vertices[0].Label} to graph";
                case EditDataQuantifier.Remove:
                    if (_quantifiers.HasFlag(EditDataQuantifier.Plural))
                        return $"Remove vertices {_vertices.Select(v => $"\"{v.Label}\"").Aggregate((a, b) => $"{a}, {b}")} from graph";
                    else
                        return $"Remove vertex {_vertices[0].Label} from graph";
                case EditDataQuantifier.Edit:
                    if (_quantifiers.HasFlag(EditDataQuantifier.Plural))
                        return $"Edit vertices {_vertices.Select(v => $"\"{v.Label}\"").Aggregate((a, b) => $"{a}, {b}")}";
                    else
                        return $"Edit vertex {_vertices[0].Label}";
                default:
                    throw new NotImplementedException();
            }
        }

        public override Graph RedoEdit(Graph original)
        {
            switch (_quantifiers)
            {
                case EditDataQuantifier.Add:
                    foreach (Vertex v in _vertices)
                        original.AddVertex(v, true);
                    break;
                case EditDataQuantifier.Edit:
                    for (int i = 0; i < _vertices.Length; i++)
                    {
                        original.AddVertex(_editedVertices[i], true);
                        original.RemoveVertex(_vertices[i], true);
                    }
                    break;
                case EditDataQuantifier.Remove:
                    foreach (Vertex v in _vertices)
                        original.RemoveVertex(v, true);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return original;
        }

        public override Graph UndoEdit(Graph edited)
        {
            switch(_quantifiers)
            {
                case EditDataQuantifier.Add:
                    foreach (Vertex v in _vertices)
                        edited.RemoveVertex(v, true);
                    break;
                case EditDataQuantifier.Edit:
                    for(int i = 0; i < _vertices.Length; i++)
                    {
                        edited.AddVertex(_vertices[i], true);
                        edited.RemoveVertex(_editedVertices[i], true);
                    }
                    break;
                case EditDataQuantifier.Remove:
                    foreach (Vertex v in _vertices)
                        edited.AddVertex(v, true);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return edited;
        }
    }
}
