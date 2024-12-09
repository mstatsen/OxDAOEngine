using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.Initializers
{
    public interface IInitializer
    {
        void InitControl(IOxControl control);
    }
}
