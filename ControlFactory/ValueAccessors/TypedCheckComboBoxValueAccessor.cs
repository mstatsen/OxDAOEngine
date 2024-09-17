using OxLibrary.Controls;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class TypedCheckComboBoxValueAccessor<T> : ValueAccessor
        where T : notnull, Enum
    {
        private OxCheckComboBox<EnumItemObject<T>> CheckedComboBox =>
            (OxCheckComboBox<EnumItemObject<T>>)Control;

        public override object? GetValue() => 
            CheckedComboBox.CheckedList;

        public override void SetValue(object? value) =>
            CheckedComboBox.CheckedList = (List<EnumItemObject<T>>?)value;
    }
}