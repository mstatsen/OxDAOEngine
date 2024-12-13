using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;
using OxLibrary.Interfaces;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls;

public class ButtonEdit<TField, TDAO, TItems, TItem, TListControl>
    : CustomItemsControl<TField, TDAO, TItems, TItem>,
    IButtonEdit
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
    where TItems : ListDAO<TItem>, new() 
    where TItem : DAO, new() 
    where TListControl : CustomItemsControl<TField, TDAO, TItems, TItem>, new()
{
    private readonly TItems internalValue = new();

    public ButtonEdit(IBuilderContext<TField, TDAO> context) =>
        Init(context);

    private readonly OxButtonEdit buttonEditControl = new()
    { 
        Dock = OxDock.Fill
    };
    public OxButtonEdit ButtonEditControl => buttonEditControl;

    protected override IOxControl GetControl() => 
        ButtonEditControl.TextBox;

    protected override void InitComponents()
    {
        buttonEditControl.Parent = this;
        buttonEditControl.Dock = OxDock.Fill;
        buttonEditControl.OnButtonClick += ButtonClick;
    }

    private void ButtonClick(object? sender, EventArgs e)
    {
        OxDialog dialog = new();
        Color dialogBaseColor = new OxColorHelper(ControlColor).Darker(7);
        TListControl editor = new()
        {
            Parent = dialog.FormPanel,
            Dock = OxDock.Fill,
            OwnerDAO = OwnerDAO,
            Font = Font,
            BaseColor = dialogBaseColor
        };
        editor.Init(Context);
        editor.Value = internalValue;
        editor.FixedItems = FixedItems;
        dialog.FirstFocusControl = editor.Control;
        dialog.SetKeyUpHandler(editor.Control);
        dialog.Size = new(360, 240);
        dialog.Text = editor.Text;
        dialog.BaseColor = dialogBaseColor;

        if (dialog.ShowDialogIsOK(this))
        {
            internalValue.CopyFrom(editor.Value);
            FillTextBox();
            ValueChangeHandler?.Invoke(this, EventArgs.Empty);
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

    protected override OxBool GetReadOnly() => 
        buttonEditControl.ReadOnly;

    protected override void SetReadOnly(OxBool value) => 
        buttonEditControl.ReadOnly = value;

    protected override void SetControlColor(Color value)
    {
        if (buttonEditControl is not null)
            buttonEditControl.BaseColor = new OxColorHelper(value).Darker(6);

        OnBackColorChanged(EventArgs.Empty);
    }
}