using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class ButtonEditAccessor<TField, TDAO, TItems, TItem, TListControl> 
        : CustomControlAccessor<TField, TDAO, TListControl, TItems>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TItems : ListDAO<TItem>, new()
        where TItem : DAO, new()
        where TListControl : CustomListControl<TField, TDAO, TItems, TItem>, new()
    {
        public ButtonEditAccessor(IBuilderContext<TField, TDAO> context): base(context) { }

        protected override Control CreateControl() => 
            new ButtonEdit<TField, TDAO, TItems, TItem, TListControl>(Context);

        protected override ValueAccessor CreateValueAccessor() =>
            new CustomButtonControlValueAccessor<TField, TDAO, 
                ButtonEdit<TField, TDAO, TItems, TItem, TListControl>, TItems, TItem, TListControl>();

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) { }

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) { }

        public override void Clear() =>
            Value = null;
    }
}
