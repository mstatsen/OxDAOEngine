namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public abstract class ValueAccessor
    {
        private Control control = default!;

        public abstract object? GetValue();
        public abstract void SetValue(object? value);

        public void SetControl(Control valueControl) =>
            control = valueControl;

        public Control Control =>
            control;
    }
}
