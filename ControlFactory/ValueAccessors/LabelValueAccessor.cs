using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class LabelValueAccessor : ValueAccessor
    {
        private OxLabel Label =>
            (OxLabel)Control;

        public object? ObjectValue { get; private set; }

        public override object? GetValue() =>
            Label.Text;

        public override void SetValue(object? value)
        {
            ObjectValue = value;
            Label.Text = value?.ToString();
        }
    }
}