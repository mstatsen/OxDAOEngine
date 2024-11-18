using OxLibrary.Panels;
using OxDAOEngine.Data;

namespace OxDAOEngine.View
{
    public interface IItemView<TField, TDAO> : IOxFrame
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        TDAO? Item { get; set; }
        OxPane AsPane { get; }

        ItemsView<TField, TDAO>? ItemsView { get; set; }
    }
}
