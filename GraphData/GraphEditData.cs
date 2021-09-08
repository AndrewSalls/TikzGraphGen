using System;

namespace TikzGraphGen.GraphData
{
    public abstract class GraphEditData
    {
        public enum EditDataType { Vertex, Edge, Subgraph }
        [Flags()]
        public enum EditDataQuantifier { Plural, Add, Remove, Edit, Saved }

        public EditDataType Type { get; protected set; }
        public EditDataQuantifier Quantifier { get; protected set; }

        public abstract Graph UndoEdit(Graph edited);
        public abstract Graph RedoEdit(Graph original);
        public abstract string GetEditDescription();
    }
}
