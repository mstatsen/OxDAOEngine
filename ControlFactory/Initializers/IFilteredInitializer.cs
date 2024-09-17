using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.ControlFactory.Initializers
{
    public interface IFilteredInitializer<TField, TDAO> : IInitializer
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        IMatcher<TField>? Filter { get; set; }
    }
}