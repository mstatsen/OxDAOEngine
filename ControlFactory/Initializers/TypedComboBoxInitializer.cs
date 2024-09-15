namespace OxXMLEngine.ControlFactory.Initializers
{
    public class TypedComboBoxInitializer<TItem> : ITypedComboBoxInitializer<TItem>
        where TItem : Enum
    {
        public virtual bool AvailableValue(TItem value) => true;

        public bool AvailableValue(object value) => 
            value is not TItem || 
            AvailableValue((TItem)value);

        public virtual void InitControl(Control control) { }

    }
}
