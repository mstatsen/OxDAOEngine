using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;
using System;

namespace OxXMLEngine.ControlFactory.Initializers
{
    public interface IFilteredInitializer<TField, TDAO> : IInitializer
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        IMatcher<TDAO>? Filter { get; set; }
    }
}