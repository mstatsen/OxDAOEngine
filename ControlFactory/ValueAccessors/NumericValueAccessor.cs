using OxLibrary.Controls;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class NumericValueAccessor : ValueAccessor
    {
        private OxSpinEdit SpinEdit =>
            (OxSpinEdit)Control;

        public override object? GetValue() =>
            SpinEdit.Value;

        public override void SetValue(object? value) =>
            SpinEdit.Value = DAO.IntValue(value);
    }
}