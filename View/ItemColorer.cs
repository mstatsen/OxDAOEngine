using OxXMLEngine.Data;

namespace OxXMLEngine.View
{
    public class ItemColorer<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public virtual Color BaseColor(TDAO? item) => default;
        public virtual Color ForeColor(TDAO? item) => default;
    }
}