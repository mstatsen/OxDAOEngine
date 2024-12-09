using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.ValueAccessors;

public abstract class ValueAccessor
{
    private IOxControl control = default!;

    public abstract object? GetValue();
    public abstract void SetValue(object? value);

    public void SetControl(IOxControl valueControl) =>
        control = valueControl;

    public IOxControl Control =>
        control;
}
