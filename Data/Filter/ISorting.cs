namespace OxXMLEngine.Data.Filter
{
    public interface ISorting<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        SortOrder SortOrder { get; set; }
        int Compare(TDAO? x, TDAO? y);
    }
}