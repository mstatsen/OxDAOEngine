using System;

namespace OxDAOEngine.Data.Filter
{
    public class Categories<TField, TDAO> : ListDAO<Category<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    {
        protected override bool AutoSorting => false;
    }
}