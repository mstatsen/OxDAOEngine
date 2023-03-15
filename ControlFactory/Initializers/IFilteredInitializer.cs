using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;

namespace OxXMLEngine.ControlFactory.Initializers
{
    public interface IFilteredInitializer<TField, TDAO> : IInitializer
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        IMatcher<TField>? Filter { get; set; }
    }
}