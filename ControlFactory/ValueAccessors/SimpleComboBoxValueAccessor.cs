using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class SimpleComboBoxValueAccessor<TItem, TComboBox> : ValueAccessor
        where TComboBox : OxComboBox<TItem>
    {
        private TComboBox ComboBox => 
            (TComboBox)Control;

        private static bool IsEmptyValue(object? value) =>
            (value == null) ||
            ((value is string stringValue) && stringValue == string.Empty);

        public override object GetValue() => 
            ComboBox.SelectedItemObject ?? ComboBox.Text;

        public override void SetValue(object? value)
        {
            ComboBox.SelectedItemObject =
                IsEmptyValue(value) ? null : value;

            if (ComboBox.SelectedItem == null)
                ComboBox.Text = string.Empty;
        }
    }
}