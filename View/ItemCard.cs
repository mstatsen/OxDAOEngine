using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data;
using OxLibrary.Handlers;

namespace OxDAOEngine.View;

public abstract class ItemCard<TField, TDAO, TFieldGroup>
    : OxCard, IItemCard<TField, TDAO>
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
    where TFieldGroup : notnull, Enum
{
    protected virtual short CardHeight => 240;
    protected virtual short CardWidth => 440;

    private readonly IListController<TField, TDAO> ListController =
        DataManager.ListController<TField, TDAO>();

    public ItemCard(ItemViewMode viewMode) : base()
    {
        ViewMode = viewMode;
        PrepareEditButton(
            EditButton,
            $"Edit {ListController.ItemName}",
            EditItemHandler
        );
        PrepareEditButton(
            DeleteButton,
            $"Delete {ListController.ItemName}",
            DeleteItemHandler
        );
        Builder = DataManager.Builder<TField, TDAO>(ControlScope.CardView, true);
        Layouter = Builder.Layouter;
        Margin.Size = 8;
        Padding.Size = 4;
        HeaderHeight = 28;
        Size = new(CardWidth, CardHeight);
        Icon = ListController.Icon;
    }

    private void EditItemHandler(object? sender, EventArgs e) =>
        DataManager.EditItem<TField, TDAO>(item);

    private void DeleteItemHandler(object? sender, EventArgs e)
    {
        DataManager.DeleteItem<TField, TDAO>(item);
        ItemsView?.RenewCards();
    }

    private void PrepareEditButton(OxIconButton button, string toolTipText, EventHandler clickHanler)
    {
        button.Size = new(25, 20);
        button.ToolTipText = toolTipText;
        button.Click += clickHanler;
        Header.AddButton(button);
    }

    public override Color DefaultColor => EngineStyles.CardColor;
    private readonly OxIconButton EditButton = new(OxIcons.Pencil, 20);
    private readonly OxIconButton DeleteButton = new(OxIcons.Trash, 20);

    public override void PrepareColors()
    {
        base.PrepareColors();
        Header.Title.ForeColor = fontColors.BaseColor;
    }

    private void LayoutControls()
    {
        Layouter.LayoutControls();
        AlignControls();
        AfterLayoutControls();
    }

    protected virtual void AfterLayoutControls() { }

    protected virtual void AlignControls() { }

    public override void OnVisibleChanged(OxBoolChangedEventArgs e)
    {
        base.OnVisibleChanged(e);

        if (IsControlsReady)
            AfterLayoutControls();
    }

    protected void ClearLayoutTemplate()
    {
        Layouter.Template.Parent = this;
        Layouter.Template.Left = 0;
        Layouter.Template.Top = 0;
        Layouter.Template.CaptionVariant = ControlCaptionVariant.Left;
        Layouter.Template.WrapLabel = true;
        Layouter.Template.MaximumLabelWidth = 80;
        Layouter.Template.BackColor = Color.Transparent;
        Layouter.Template.FontColor = fontColors.BaseColor;
        Layouter.Template.FontStyle = FontStyle.Bold;
        Layouter.Template.LabelColor = fontColors.Lighter();
        Layouter.Template.LabelStyle = FontStyle.Italic;
        Layouter.Template.AutoSize = OxB.T;
    }

    protected abstract void PrepareLayouts();

    private void PrepareLayoutsInternal()
    {
        ClearLayoutTemplate();
        PrepareLayouts();
    }

    private void ItemChangeHandler(object sender, DAOEntityEventArgs e) =>
        PrepareControls();

    protected void SetColors()
    {
        if (item is null)
            return;
        
        ItemColorer<TField, TDAO> itemColorer = DataManager.ControlFactory<TField, TDAO>().ItemColorer;
        BaseColor = itemColorer.BaseColor(item);
        fontColors.BaseColor = itemColorer.ForeColor(item);
    }

    private bool IsControlsReady = false;
    private void PrepareControls()
    {
        IsControlsReady = false;

        try
        {
            SetColors();
            SetTitle();
            FillControls();
            ClearLayoutsInternal();
            PrepareLayoutsInternal();
            LayoutControls();
        }
        finally
        {
            IsControlsReady = true;
        }
    }

    private void ClearLayoutsInternal()
    {
        ClearLayouts();
        Layouter?.Clear();
    }

    protected virtual void ClearLayouts() { }

    private void FillControls()
    {
        if (item is not null)
            Builder.FillControls(item);
    }

    private void SetTitle() =>
        Text = GetTitle();

    protected virtual string GetTitle() =>
        item is null || item.ToString() is null 
            ? string.Empty 
            : item?.ToString()!;

    public void ApplySettings() { }

    private TDAO? item;

    public TDAO? Item
    {
        get => item;
        set
        {
            if (item is not null)
                item.ChangeHandler -= ItemChangeHandler;

            item = value;

            if (item is not null)
                item.ChangeHandler += ItemChangeHandler;

            PrepareControls();
            PrepareColors();
            SetButtonsVisible();
            SetTitle();
        }
    }

    private void SetButtonsVisible()
    {
        SetExpandButtonVisible(ListController.Settings.CardsAllowExpand);
        EditButton.SetVisible(
            ListController.Settings.CardsAllowEdit
            && ViewMode is ItemViewMode.WithEditLinks
        );
        DeleteButton.SetVisible(
            ListController.Settings.CardsAllowDelete
            && ViewMode is ItemViewMode.WithEditLinks
        );
    }

    private readonly ItemViewMode ViewMode;

    public OxPanel AsPane => this;

    private ItemsView<TField, TDAO>? itemsView;
    public ItemsView<TField, TDAO>? ItemsView 
    { 
        get => itemsView;
        set => itemsView = value;
    }

    protected readonly ControlBuilder<TField, TDAO> Builder;
    protected readonly ControlLayouter<TField, TDAO> Layouter;
    private readonly OxColorHelper fontColors = new(default);
}