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
    }
}
