namespace OxDAOEngine.Data.Filter
{
    public class Categories<TField, TDAO> : ListDAO<Category<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public override string DefaultXmlElementName => "Categories";
    }
}