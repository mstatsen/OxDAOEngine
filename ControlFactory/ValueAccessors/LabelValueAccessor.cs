using OxLibrary.Controls;

namespace OxXMLEngine.ControlFactory.ValueAccessors
{
    public class LabelValueAccessor : ValueAccessor
    {
        private OxLabel Label =>
            (OxLabel)Control;

        public override object? GetValue() =>
            Label.Text;

        public override void SetValue(object? value) =>
            Label.Text = value?.ToString();
    }
}