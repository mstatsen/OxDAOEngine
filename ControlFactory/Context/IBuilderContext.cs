using OxXMLEngine.ControlFactory.Initializers;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Context
{
    public interface IBuilderContext<TField, TDAO> : IAccessorContext
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        ControlBuilder<TField, TDAO> Builder { get; }
        new IBuilderContext<TField, TDAO> SetInitializer(IInitializer initializer);
    }
}