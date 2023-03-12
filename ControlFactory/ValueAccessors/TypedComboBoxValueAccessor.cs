using OxLibrary.Controls;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.ControlFactory.ValueAccessors
{
    public class TypedComboBoxValueAccessor<T> : ValueAccessor
        where T : Enum
    {
        public new OxComboBox Control => (OxComboBox)base.Control;

        public override object? GetValue() => 
            TypeHelper.Value(Control.SelectedItem);

        public override void SetValue(object? value) =>
            Control.SelectedItem = 
                (value == null) || (value is NullObject)
                    ? value 
                    : value is string
                        ? TypeHelper.Parse<T>(value.ToString() ?? string.Empty)
                        : TypeHelper.TypeObject(value);
    }
}