using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;
using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;

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

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value)
        {
            if (CustomControl is not null)
                CustomControl.ValueChangeHandler -= value;
        }

        protected override void AssignValueChangeHanlderToControl(EventHandler? value)
        {
            if (CustomControl is not null)
                CustomControl.ValueChangeHandler += value;
        }

        protected override void AfterControlsCreated()
        {
            base.AfterControlsCreated();

            if (CustomControl is null)
                return;

            CustomControl.ItemEdited += ItemEditedHandler;

            if (CustomControl is IItemsContainerControl<TField, TDAO> listControl)
            {
                listControl.ItemAdded += ItemAddedHandler;
                listControl.ItemRemoved += ItemRemovedHandler;
            }
        }

        protected override void OnControlValueChanged(object? value)
        {
            if (ReadOnlyControl is null)
                return;

            base.OnControlValueChanged(
                CustomControl is null 
                    ? value
                    : CustomControl.PrepareValueToReadOnly((TItem?)value));
        }

        private void ItemRemovedHandler(object? sender, EventArgs e) => 
            OnControlValueChanged(Value);

        private void ItemAddedHandler(object? sender, EventArgs e) =>
            OnControlValueChanged(Value);

        private void ItemEditedHandler(object? sender, EventArgs e) =>
            OnControlValueChanged(Value);

        public override void Clear() =>
            Value = null;

        public TControl? CustomControl =>
            Control is TControl 
                ? (TControl?)Control 
                : null;

        protected override void SetReadOnly(bool value)
        {
            if (CustomControl is not null 
                && CustomControl.ReadonlyMode is ReadonlyMode.EditAsReadonly)
            {
                CustomControl.ReadOnly = value;
                return;
            }

            base.SetReadOnly(value);

            if (CustomControl is not null 
                && CustomControl.Parent?.Parent is OxPane panel)
            {
                if (value)
                {
                    panel.Padding.Top = OxWh.W2;
                    panel.Padding.Bottom = OxWh.W2;
                    panel.Padding.Left = OxWh.W8;
                    panel.Padding.Right = OxWh.W8;
                }
                else
                    panel.Padding.Size = OxWh.W0;
            }
        }

        protected override Control? CreateReadOnlyControl() => 
            new OxTextBox()
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
            };
    }
}