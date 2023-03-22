using OxLibrary.Controls;
using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.ControlFactory.ValueAccessors;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.ControlFactory.Accessors
{
    public class ComboBoxAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override ValueAccessor CreateValueAccessor() =>
            Context.MultipleValue
            ? new CheckComboBoxValueAccessor()
            : new SimpleComboBoxValueAccessor();

        protected override Control CreateControl() =>
            Context.MultipleValue
                ? new OxCheckComboBox() 
                : (Control)new OxComboBox();

        protected override void AfterControlCreated()
        {
            base.AfterControlCreated();
            ComboBox.SizeChanged += ComboBoxSizeChangeHandler;
            ComboBox.LocationChanged += ComboBoxLocationChangeHandler;
            ComboBox.SelectionChangeCommitted += (s, e) => ReadOnlyControl.Text = ComboBox.Text;
            ComboBox.TextChanged += (s, e) => ReadOnlyControl.Text = ComboBox.Text;
            ComboBox.ParentChanged += (s, e) => ReadOnlyControl.Parent = ComboBox.Parent;
            ComboBox.BackColorChanged += (s, e) => ReadOnlyControl.BackColor = ComboBox.BackColor;
            ComboBox.FontChanged += (s, e) => ReadOnlyControl.Font = ComboBox.Font;
            ComboBox.ForeColorChanged += (s, e) => ReadOnlyControl.ForeColor = ComboBox.ForeColor;
            ComboBox.VisibleChanged += (s, e) => ReadOnlyControl.Visible = visible && ReadOnly && !ComboBox.Visible;
        }

        private void ComboBoxLocationChangeHandler(object? sender, EventArgs e)
        {
            ReadOnlyControl.Left = ComboBox.Left;
            ReadOnlyControl.Top = ComboBox.Top;
        }

        private void ComboBoxSizeChangeHandler(object? sender, EventArgs e)
        {
            ReadOnlyControl.Width = ComboBox.Width;
            ReadOnlyControl.Height = ComboBox.Height;
        }

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

        private bool readOnly = false;

        protected override void SetReadOnly(bool value)
        {
            readOnly = value;
            ComboBox.Visible = visible && !readOnly;
            ReadOnlyControl.Visible = visible && readOnly;
        }

        protected override bool GetReadOnly() => 
            readOnly;

        private bool visible = true;
        protected override bool GetVisible() => visible;

        protected override void SetVisible(bool value)
        {
            visible = value;
            ComboBox.Visible = visible && !readOnly;
        }

        public OxComboBox ComboBox =>
            (OxComboBox)Control;

        private readonly OxTextBox ReadOnlyControl = new()
        { 
            ReadOnly = true
        };

        public ComboBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        public override bool IsEmpty => 
            base.IsEmpty || (Value is NullObject);
    }
}
