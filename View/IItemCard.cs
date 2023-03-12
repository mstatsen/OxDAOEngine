using OxXMLEngine.Data;

namespace OxXMLEngine.View
{
    public interface IItemCard<TField, TDAO> : IItemView<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    {
    }
}