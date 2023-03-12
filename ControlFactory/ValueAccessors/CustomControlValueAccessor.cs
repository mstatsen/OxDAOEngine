using OxXMLEngine.ControlFactory.Controls;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.ValueAccessors
{
    public class CustomControlValueAccessor<TField, TDAO, TControl, TItem> : ValueAccessor
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TControl : CustomControl<TField, TDAO, TItem>
        where TItem : DAO, new()
    {
        public override object? GetValue() =>
            ((TControl)Control).Value;

        public override void SetValue(object? value) =>
            ((TControl)Control).Value = value != null ? (TItem)value : default!;
    }
}