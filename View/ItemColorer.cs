using OxLibrary;
using OxDAOEngine.Data;

namespace OxDAOEngine.View
{
    public class ItemColorer<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public virtual Color BaseColor(TDAO? item) => default;
        public Color BackColor(TDAO? item) => new OxColorHelper(BaseColor(item)).Lighter(7);
        public virtual Color ForeColor(TDAO? item) => default;
    }
}