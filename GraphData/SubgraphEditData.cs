using System;

namespace TikzGraphGen.GraphData
{
    public class SubgraphEditData : GraphEditData
    {
        private readonly Graph _edit;
        private readonly EditDataQuantifier _quantifiers;

        public SubgraphEditData(Graph edit, EditDataQuantifier quantifiers)
        {
            _edit = edit;
            _quantifiers = quantifiers & ~EditDataQuantifier.Plural;
        }

        public override string GetEditDescription()
        {
            if(_quantifiers.HasFlag(EditDataQuantifier.Add))
                return "Add subgraph to graph";
            if(_quantifiers.HasFlag(EditDataQuantifier.Remove))
                return "Remove subgraph from graph";
                
            throw new NotImplementedException();
        }

        public override Graph RedoEdit(Graph original)
        {
            switch (_quantifiers)
            {
                case EditDataQuantifier.Add:
                    original.AddSubgraph(_edit, true);
                    break;
                case EditDataQuantifier.Remove:
                    original.RemoveSubgraph(_edit, true);
                    break;
                case EditDataQuantifier.Edit:
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
                    edited.RemoveSubgraph(_edit, true);
                    break;
                case EditDataQuantifier.Remove:
                    edited.AddSubgraph(_edit, true);
                    break;
                case EditDataQuantifier.Edit:
                default:
                    throw new NotImplementedException();
            }

            return edited;
        }
    }
}