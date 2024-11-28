using OxLibrary.Panels;
using OxDAOEngine.Data;

namespace OxDAOEngine.View
{
    public interface IItemView<TField, TDAO> : IOxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        TDAO? Item { get; set; }
        OxPanel AsPane { get; }

        ItemsView<TField, TDAO>? ItemsView { get; set; }
    }
}
