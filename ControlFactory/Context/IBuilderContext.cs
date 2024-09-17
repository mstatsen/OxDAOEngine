using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Context
{
    public interface IBuilderContext<TField, TDAO> : IAccessorContext
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        ControlBuilder<TField, TDAO> Builder { get; }
        new IBuilderContext<TField, TDAO> SetInitializer(IInitializer initializer);
    }
}