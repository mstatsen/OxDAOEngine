using OxLibrary.Panels;
using OxXMLEngine.Data;

namespace OxXMLEngine.View
{
    public interface IItemView<TField, TDAO> : IOxFrame
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    {
        TDAO? Item { get; set; }
        OxPane AsPane { get; }
        void ApplySettings();
    }

    public interface IItemInfo<TField, TDAO> : IItemView<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    { 
        bool Expanded { get; set; }

        EventHandler? OnExpandedChanged { get; set; }
        EventHandler? OnAfterExpand { get; set; }
        EventHandler? OnAfterCollapse { get; set; }
    }
}
