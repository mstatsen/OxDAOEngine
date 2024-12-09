using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.Initializers;

public class EmptyControlInitializer : IInitializer
{
    public virtual void InitControl(IOxControl control) { }
}