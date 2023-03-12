using OxXMLEngine.ControlFactory.Controls;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.ValueAccessors
{
    public class CustomButtonControlValueAccessor<TField, TDAO, TControl, TItem, TItemPart, TListControl>
        : CustomControlValueAccessor<TField, TDAO, TControl, TItem>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TControl : ButtonEdit<TField, TDAO, TItem, TItemPart, TListControl>
        where TItem : ListDAO<TItemPart>, new()
        where TItemPart : DAO, new()
        where TListControl : CustomListControl<TField, TDAO, TItem, TItemPart>, new()
    {
        public override object? GetValue() =>
            ((TControl)Control).Value;

        public override void SetValue(object? value) =>
            ((TControl)Control).Value = value != null ? (TItem)value : new TItem();
    }
}
