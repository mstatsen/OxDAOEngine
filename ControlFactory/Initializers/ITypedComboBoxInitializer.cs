namespace OxXMLEngine.ControlFactory.Initializers
{
    public interface ITypedComboBoxInitializer<TItem> : IComboBoxInitializer
        where TItem : Enum
    {
        bool AvailableValue(TItem value);
    }
}
