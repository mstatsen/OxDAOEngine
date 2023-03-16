using OxLibrary.Panels;
using OxXMLEngine.Data;

namespace OxXMLEngine.View
{
    public interface IItemInfo<TField, TDAO> : IItemView<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    { 
        bool Expanded { get; set; }
        bool Pinned { get; set; }
        OxPanel Sider { get; }

        EventHandler? OnPinnedChanged { get; set; }
        EventHandler? OnExpandedChanged { get; set; }
        EventHandler? OnAfterExpand { get; set; }
        EventHandler? OnAfterCollapse { get; set; }
    }
}
