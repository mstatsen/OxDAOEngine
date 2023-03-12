using OxLibrary.Controls;

namespace OxXMLEngine.ControlFactory.ValueAccessors
{
    public class TextBoxValueAccessor : ValueAccessor
    {
        private OxTextBox TextBox => 
            (OxTextBox)Control;

        public override object? GetValue() => 
            TextBox.Text;

        public override void SetValue(object? value) =>
            TextBox.Text = value != null
                ? value.ToString()
                : string.Empty;
    }
}