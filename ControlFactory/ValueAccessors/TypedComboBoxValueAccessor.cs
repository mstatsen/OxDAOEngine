using OxLibrary.Controls;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class TypedComboBoxValueAccessor<T> : ValueAccessor
        where T : Enum
    {
        public new OxComboBox Control => (OxComboBox)base.Control;

        public override object? GetValue() => 
            TypeHelper.Value(Control.SelectedItem);

        public override void SetValue(object? value) =>
            Control.SelectedItem = 
                (value == null) || (value is IEmptyChecked ec && ec.IsEmpty)
                    ? value 
                    : value is string
                        ? TypeHelper.Parse<T>(value.ToString() ?? string.Empty)
                        : TypeHelper.TypeObject(value);
    }
}