using OxLibrary.Panels;
using OxDAOEngine.Data;

namespace OxDAOEngine.View
{
    public interface IItemView<TField, TDAO> : IOxFrame
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    {
        TDAO? Item { get; set; }
        OxPane AsPane { get; }
    }
}
