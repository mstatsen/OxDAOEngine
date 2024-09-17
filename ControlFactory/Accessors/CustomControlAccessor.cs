using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class CustomControlAccessor<TField, TDAO, TControl, TItem> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TControl : CustomControl<TField, TDAO, TItem>, new()
        where TItem : DAO, new()
    {
        public CustomControlAccessor(IBuilderContext<TField, TDAO> context) : base(context){ }

        protected override bool AutoInit => false;

        protected override Control CreateControl() =>
            new TControl().Init(Context);

        protected override ValueAccessor CreateValueAccessor() =>
            new CustomControlValueAccessor<TField, TDAO, TControl, TItem>();

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) =>
            CustomControl.ValueChangeHandler -= value;

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) => 
            CustomControl.ValueChangeHandler += value;

        public override void Clear() =>
            Value = null;

        public TControl CustomControl =>
            (TControl)Control;

        protected override bool GetReadOnly() =>
            CustomControl.ReadOnly;

        protected override void SetReadOnly(bool value) =>
            CustomControl.ReadOnly = value;
    }
}
