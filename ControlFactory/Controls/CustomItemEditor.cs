using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;
using OxLibrary.Interfaces;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;
using OxLibrary.Geometry;

namespace OxDAOEngine.ControlFactory.Controls;

public abstract partial class CustomItemEditor<TItem, TField, TDAO> : OxDialog
    where TItem : DAO, new()
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
{
    public IBuilderContext<TField, TDAO> Context { get; private set; } = default!;
    public ControlBuilder<TField, TDAO> Builder => Context.Builder;

    public CustomItemEditor() { }

    public TEditor Init<TEditor>(IBuilderContext<TField, TDAO> context)
        where TEditor : CustomItemEditor<TItem, TField, TDAO>
    {
        Context = context;
        InitializeComponent();
        SetPaddings();
        CreateControls();
        FirstLoad = true;
        return (TEditor)this;
    }

    protected bool FirstLoad { get; private set; }

    public TDAO? OwnerDAO { get; set; }

    public TItem? ParentItem { get; set; }

    public List<object>? ExistingItems { get; set; }

    public IMatcher<TField>? Filter { get; set; }

    public virtual void RenewData() { }

    protected virtual void SetPaddings() { }

    public IItemsContainerControl<TField,TDAO>? OwnerControl { get; internal set; }

    protected bool ReadOnly { get; set; }
    public DialogResult Edit(TItem item, bool readOnly = false)
    {
        ReadOnly = readOnly;
        PrepareReadonly();
        PrepareControlColors();
        FillControls(item);
        DialogResult result = ShowDialog(OwnerControl);

        if (result is DialogResult.OK)
            GrabControls(item);

        return result;
    }

    protected virtual void PrepareReadonly() { }

    protected virtual void PrepareControlColors() 
    {
        foreach (IOxControl control in FormPanel.OxControls)
            if (control is not OxLabel)
                ControlPainter.ColorizeControl(
                    control,
                    BaseColor
                );

        BackColor = Colors.Lighter(8);
    }

    protected override void OnShown(EventArgs e) => 
        RecalcSize();

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RecalcSize();

        if (FirstLoad)
            FirstLoad = false;
    }

    protected virtual void RecalcSize() => 
        Size = new(
            ContentWidth,
            ContentHeight + OxSH.IfElseZero(!FirstLoad, 6)
        );

    protected virtual void CreateControls() { }

    protected virtual short ContentWidth => 400;
    protected virtual short ContentHeight => 240;

    public TItem? Add()
    {
        TItem item = CreateNewItem();
        return 
            Edit(item) is DialogResult.OK 
                ? item 
                : null;
    }

    protected OxLabel CreateLabel(string caption, IControlAccessor accessor, 
        bool rightLabel = false) =>
        CreateLabel(caption, accessor.Control, rightLabel);

    private OxLabel CreateLabel(string caption, IOxControl control, bool rightLabel = false) => 
        OxControlHelper.AlignByBaseLine(control,
            new OxLabel()
            {
                Parent = this,
                AutoSize = true,
                Left = OxSH.Add(OxSH.IfElseZero(rightLabel, control.Right), 8),
                Text = caption,
                Font = OxStyles.DefaultFont
            }
        )!;

    protected virtual TItem CreateNewItem() => new();

    protected abstract void FillControls(TItem item);

    protected abstract void GrabControls(TItem item);
}