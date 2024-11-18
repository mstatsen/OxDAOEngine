using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class ComboBoxAccessor<TField, TDAO, TItem, TComboBox> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TComboBox : OxComboBox<TItem>, new()
    {
        protected override ValueAccessor CreateValueAccessor() =>
            Context.MultipleValue
            ? new CheckComboBoxValueAccessor()
            : new SimpleComboBoxValueAccessor<TItem, TComboBox>();

        protected override Control CreateControl() =>
            Context.MultipleValue
                ? new OxCheckComboBox() 
                : new TComboBox();

        protected virtual bool AvailableValue(object value) =>
            Context is null
            || Context.Initializer is null
            || Context.Initializer is not IComboBoxInitializer initializer
            || initializer.AvailableValue(value);

        protected override void AssignValueChangeHanlderToControl(EventHandler? value)
        {
            ComboBox.SelectionChangeCommitted += value;
            ComboBox.TextChanged += value;
        }

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value)
        {
            ComboBox.SelectionChangeCommitted -= value;
            ComboBox.TextChanged -= value;
        }

        public override void Clear()
        {
            if (ComboBox.Items.Count > 0)
                ComboBox.SelectedIndex = 0;
            else Value = null;
        }

        public TComboBox ComboBox =>
            (TComboBox)Control;

        public ComboBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        protected override void AfterInitControl() 
        { 
            foreach (var item in ComboBox.Items)
                if (!AvailableValue(item))
                    ComboBox.Items.Remove(item);

            ComboBox.SelectionChangeCommitted += ReadOnlyControlTextChangeHandler;
            SetDefaultValue();
        }

        private void ReadOnlyControlTextChangeHandler(object? sender, EventArgs e) =>
            OnControlValueChanged(ComboBox.SelectedItem);

        public override bool IsEmpty =>
            base.IsEmpty
            || (Value is IEmptyChecked e && e.IsEmpty)
            || (ComboBox.SelectedItem is IEmptyChecked ec && ec.IsEmpty);

        public override void SetDefaultValue()
        {
            if (ComboBox.Items.Count > 0)
                ComboBox.SelectedIndex = 0;
        }

        public void SelectFirst() => 
            ComboBox.SelectedIndex = ComboBox.Items.Count > 0 ? 0 : -1;
    }

    public class ComboBoxAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO, object, OxComboBox>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public ComboBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context)
        {
        }
    }
}
