using OxLibrary.Controls;

namespace OxXMLEngine.ControlFactory.ValueAccessors
{
    public class SimpleComboBoxValueAccessor : ValueAccessor
    {
        private OxComboBox ComboBox => 
            (OxComboBox)Control;

        private static bool IsEmptyValue(object? value) =>
            (value == null) ||
            ((value is string stringValue) && stringValue == string.Empty);

        public override object GetValue() => 
            ComboBox.SelectedItem ?? ComboBox.Text;

        public override void SetValue(object? value)
        {
            ComboBox.SelectedItem =
                IsEmptyValue(value) ? null : value;

            if (ComboBox.SelectedItem == null)
                ComboBox.Text = string.Empty;
        }
    }
}
