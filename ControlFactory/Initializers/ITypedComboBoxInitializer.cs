using System;

namespace OxXMLEngine.ControlFactory.Initializers
{
    public interface ITypedComboBoxInitializer<TItem> : IInitializer
        where TItem : Enum
    {
        bool AvailableValue(TItem value);
    }
}
