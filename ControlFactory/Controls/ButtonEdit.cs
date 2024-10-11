using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Dialogs;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
{
    public class ButtonEdit<TField, TDAO, TItems, TItem, TListControl>
        : CustomListControl<TField, TDAO, TItems, TItem>,
        IButtonEdit
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TItems : ListDAO<TItem>, new() 
        where TItem : DAO, new() 
        where TListControl : CustomListControl<TField, TDAO, TItems, TItem>, new()
    {
        private readonly TItems internalValue = new();

        public ButtonEdit(IBuilderContext<TField, TDAO> context) =>
            Init(context);

        private readonly OxButtonEdit buttonEditControl = new()
        { 
            Dock = DockStyle.Fill
        };
        public OxButtonEdit ButtonEditControl => buttonEditControl;

        protected override Control GetControl() => 
            ButtonEditControl.TextBox;

        protected override void InitComponents()
        {
            buttonEditControl.Parent = this;
            buttonEditControl.Dock = DockStyle.Fill;
            buttonEditControl.OnButtonClick += ButtonClick;
        }

        private void ButtonClick(object? sender, EventArgs e)
        {
            OxDialog dialog = new();
            Color dialogBaseColor = new OxColorHelper(ControlColor).Darker(7);
            TListControl editor = new()
            {
                Parent = dialog,
                Dock = DockStyle.Fill,
                ParentItem = ParentItem,
                Font = Font,
                BaseColor = dialogBaseColor
            };
            editor.Init(Context);
            editor.Value = internalValue;
            dialog.FirstFocusControl = editor.Control;
            dialog.SetKeyUpHandler(editor.Control);
            dialog.SetContentSize(360, 240);
            dialog.Text = editor.Text;
            dialog.BaseColor = dialogBaseColor;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                internalValue.CopyFrom(editor.Value);
                FillTextBox();
            }

            dialog.Dispose();
        }

        protected override void ClearValue() => 
            internalValue.Clear();

        protected override void SetValuePart(TItem valuePart) => 
            internalValue.Add(valuePart);

        protected override void GrabList(TItems list) => 
            list.CopyFrom(internalValue);

        private void FillTextBox() => 
            buttonEditControl.Value = internalValue.ShortString;

        protected override void AfterSetValue()
        {
            base.AfterSetValue();
            FillTextBox();
        }

        protected override bool GetReadOnly() => 
            buttonEditControl.ReadOnly;

        protected override void SetReadOnly(bool value) => 
            buttonEditControl.ReadOnly = value;

        protected override void SetControlColor(Color value)
        {
            if (buttonEditControl != null)
                buttonEditControl.BaseColor = new OxColorHelper(value).Darker(6);
        }
    }
}