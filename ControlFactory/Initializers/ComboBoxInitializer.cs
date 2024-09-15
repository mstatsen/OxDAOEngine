namespace OxXMLEngine.ControlFactory.Initializers
{
    public class ComboBoxInitializer : IComboBoxInitializer
    {
        public virtual bool AvailableValue(object value) => true;

        public virtual void InitControl(Control control) { }

    }
}
