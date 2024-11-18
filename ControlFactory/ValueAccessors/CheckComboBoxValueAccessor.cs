using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class CheckComboBoxValueAccessor : ValueAccessor
    {
        private OxCheckComboBox CheckComboBox =>
            (OxCheckComboBox)Control;

        private static bool IsEmptyValue(object? value) =>
            (value is null) ||
            (value is string stringValue 
                && stringValue == string.Empty);

        public override object? GetValue() =>
            CheckComboBox.CheckedList ?? null;

        public override void SetValue(object? value) =>
            CheckComboBox.CheckedList =
                IsEmptyValue(value) ? null : (List<object>?)value;
    }
}