using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class CheckBoxValueAccessor : ValueAccessor
    {
        private OxCheckBox CheckBox => 
            (OxCheckBox)Control;

        public override object? GetValue() => 
            CheckBox.Checked;

        public override void SetValue(object? value) => 
            CheckBox.Checked = value != null
                && (value is int @int
                    ? @int == 1
                    : value is string @string
                        ? bool.Parse(@string)
                        : (bool)value);
    }
}