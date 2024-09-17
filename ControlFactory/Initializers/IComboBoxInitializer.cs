namespace OxDAOEngine.ControlFactory.Initializers
{
    public interface IComboBoxInitializer : IInitializer
    {
        bool AvailableValue(object value);
    }
}
