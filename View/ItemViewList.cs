using OxLibrary.Panels;
using OxDAOEngine.Data;

namespace OxDAOEngine.View
{
    public class ItemViewList<TField, TDAO> : List<IItemView<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    {
        public IItemView<TField, TDAO>? Last =>
            Count > 0
                ? this[Count - 1]
                : null;

        public IItemView<TField, TDAO>? First =>
            Count > 0
                ? this[0]
                : null;

        public int Bottom
        {
            get
            {
                IItemView<TField, TDAO>? last = Last;
                return last == null
                    ? 0
                    : last.Bottom + 24;
            }
        }

        public OxPaneList AsPaneList =>
            new OxPaneList().AddRange(this.Select(item => item.AsPane));
    }
}