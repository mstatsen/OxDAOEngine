using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.Initializers;

public class ComboBoxInitializer : IComboBoxInitializer
{
    public virtual bool AvailableValue(object value) => true;

    public virtual void InitControl(IOxControl control) { }
}
