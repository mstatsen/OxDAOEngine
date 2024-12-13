﻿using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Geometry;
using OxLibrary.Handlers;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings;

namespace OxDAOEngine.View;

public abstract class ItemInfo<TField, TDAO, TFieldGroup> : FunctionsPanel<TField, TDAO>, IItemInfo<TField, TDAO>
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
    where TFieldGroup : notnull, Enum
{
    private TDAO? item;

    public TDAO? Item
    {
        get => item;
        set
        {
            if (item is not null 
                && item.Equals(value))
                return;

            if (item is not null)
                item.ChangeHandler -= ItemChangeHandler;

            item = value;

            if (Item is not null)
                Item.ChangeHandler += ItemChangeHandler;

            RenewControls();
        }
    }

    private void RenewControls()
    {
        if (item is null)
            return;

        BaseColor = ControlFactory.ItemColorer.BaseColor(item);
        FontColors.BaseColor = ControlFactory.ItemColorer.ForeColor(item);

        if (IsExpanded)
            PrepareControls();

        PrepareColors();
    }

    protected override Color FunctionColor => DefaultColor;

    public ItemInfo() : base()
    {
        AutoScroll = true;
        Dock = OxDock.Right;
        //ContentBox.AutoScroll = true;
        PreparePanels();
        Layouter = Builder.Layouter;
        SetSizes();
        FontColors = new OxColorHelper(DefaultForeColor);
        SettingsAvailable = OxB.F;
        Icon = DataManager.ListController<TField, TDAO>().Icon;
    }

    protected override void OnExpandedChanged(OxBoolChangedEventArgs e)
    {
        base.OnExpandedChanged(e);

        if (e.IsChanged)
            RenewControls();
    }

    public override void OnVisibleChanged(OxBoolChangedEventArgs e)
    {
        base.OnVisibleChanged(e);

        if (e.IsChanged)
            RenewControls();
    }

    private void SetSizes()
    {
        Margin.Size = 2;
        Margin[OxDockHelper.Opposite(Dock)].Size = 0;

        if (Dock is OxDock.Right)
            Margin.Bottom = 0;

        Margin.Top =
            OxSh.Short(
                Dock is OxDock.Bottom
                ? 1
                : OxSh.Short(IsPinned ? 9 : 4)
            );

        Margin.Right = 0;
        Padding.Size = 4;
        HeaderHeight = 36;
    }

    protected override void OnPinnedChanged(OxBoolChangedEventArgs e)
    {
        if (!e.IsChanged)
            return;

        SetSizes();
    }

    public OxPanel AsPane => this;

    public override void PrepareColors()
    {
        base.PrepareColors();

        if (FontColors is not null)
            Header.Title.ForeColor = FontColors.BaseColor;

        foreach (OxPanel panel in panels)
            panel.BaseColor = BaseColor;

        foreach (OxHeader header in Headers.Values)
        {
            header.BaseColor = BaseColor;
            header.Title.ForeColor = Header.Title.ForeColor;
        }
    }

    private void LayoutControls()
    {
        Layouter.LayoutControls();
        AlignControlInternal();
        WrapTextControls();
    }

    protected virtual void WrapTextControls() { }

    protected void WrapControl(TField field) =>
        WrapControl(field, new(360, 60));
    
    private void WrapControl(TField field, Size size)
    {
        OxLabel? control = 
            (OxLabel?)Layouter.PlacedControl(field)?.Control;

        if (control is null)
            return;
        
        control.MaximumSize = new(size);
        control.TextAlign = ContentAlignment.TopLeft;
    }

    private void AlignControlInternal()
    {
        AlignControls();

        foreach (ControlLayouts<TField> list in LayoutsLists.Values)
            Layouter.AlignLabels(list);
    }

    protected virtual void AlignControls() { }

    protected void ClearLayoutTemplate()
    {
        Layouter.Template.Parent = this;
        Layouter.Template.Left = 0;
        Layouter.Template.Top = 8;
        Layouter.Template.CaptionVariant = ControlCaptionVariant.Left;
        Layouter.Template.WrapLabel = true;
        Layouter.Template.MaximumLabelWidth = 80;
        Layouter.Template.BackColor = Color.Transparent;
        Layouter.Template.FontColor = FontColors.BaseColor;
        Layouter.Template.FontStyle = FontStyle.Bold;
        Layouter.Template.LabelColor = FontColors.Lighter();
        Layouter.Template.LabelStyle = FontStyle.Italic;
        Layouter.Template.AutoSize = OxB.T;
    }

    private void PrepareLayoutsInternal()
    {
        ClearLayoutTemplate();
        LayoutsLists.Clear();
        PrepareLayouts();
    }

    protected abstract void PrepareLayouts();


    private void SetTitle() => 
        Text = Item is null 
            ? string.Empty 
            : GetTitle();

    protected virtual string GetTitle() =>
        Item is null || Item.ToString() is null
            ? string.Empty
            : Item?.ToString()!;

    protected void PreparePanel(OxPanel panel, string text)
    {
        panel.Parent = this;
        panel.Dock = OxDock.Top;

        if (!text.Equals(string.Empty))
            Headers.Add(panel, new OxHeader(text)
            {
                Parent = panel.Parent
            });

        panels.Add(panel);
    }


    protected virtual void PreparePanels() { }

    private void ItemChangeHandler(object sender, DAOEntityEventArgs e) =>
        PrepareControls();

    private void PrepareControls()
    {
        SetTitle();

        if (Item is not null)
            Builder.FillControls(Item);
        else
            foreach (ControlLayout<TField> layout in Layouter.Layouts)
            {
                IControlAccessor placedControl = Builder[layout.Field];

                if (placedControl is not null)
                    placedControl.Control.Visible = OxB.F;
            }

        ClearLayouts();
        PrepareLayoutsInternal();
        LayoutControls();
        RecalcControlsVisible();
        AfterControlLayout();
    }

    private void RecalcControlsVisible()
    {
        foreach (OxPanel parentPanel in LayoutsLists.Keys)
        {
            Headers.TryGetValue(parentPanel, out var header);

            short lastBottom = OxSh.Short(header is not null ? 8 : 0);
            short maxBottom = lastBottom;
            bool visibleControlsExists = false;
            ControlLayout<TField>? prevLayout = null;

            foreach (ControlLayout<TField> layout in LayoutsLists[parentPanel])
            {
                bool controlVisible = !Builder[layout.Field].IsEmpty;
                PlacedControl<TField>? placedControl = Layouter.PlacedControl(layout.Field);

                if (placedControl is null)
                    continue;

                placedControl.SetVisible(controlVisible);

                if (controlVisible)
                {
                    visibleControlsExists = true;
                    placedControl.Control.Top =
                        OxSh.Add(
                            lastBottom,
                            prevLayout is not null
                                ? OxSh.Sub(
                                    layout.Top,
                                    prevLayout is not null ? prevLayout!.Bottom : 0
                                )
                                : 8
                        );
                    OxControlHelper.AlignByBaseLine(placedControl.Control, placedControl.Label!);
                    lastBottom = placedControl.Control.Bottom;
                    maxBottom = Math.Max(maxBottom, lastBottom);
                }

                prevLayout = layout;
            }

            parentPanel.Height = OxSh.Add(maxBottom, 36);
            parentPanel.SetVisible(visibleControlsExists);
            SetHeaderVisible(visibleControlsExists);
        }
    }

    protected virtual void AfterControlLayout() { }

    private void ClearLayouts() =>
        Layouter?.Clear();

    protected override DAOSetting VisibleSetting => DAOSetting.ShowItemInfo;

    protected override DAOSetting PinnedSetting => DAOSetting.ItemInfoPanelPinned;

    protected override DAOSetting ExpandedSetting => DAOSetting.ItemInfoPanelExpanded;

    protected override void ApplySettingsInternal()
    {
        OxDock settingsDock = 
            TypeHelper.Helper<ItemInfoPositionHelper>().Dock(Settings.ItemInfoPosition);

        if (!Dock.Equals(settingsDock))
            Dock = settingsDock;

        base.ApplySettingsInternal();
    }

    public override void SaveSettings()
    {
        base.SaveSettings();
        Settings.ItemInfoPanelPinned = IsPinned;
        Settings.ItemInfoPanelExpanded = IsExpanded;
    }

    protected ControlFactory<TField, TDAO> ControlFactory = 
        DataManager.ControlFactory<TField, TDAO>();

    protected ControlBuilder<TField, TDAO> Builder =>
        ControlFactory.Builder(ControlScope.InfoView);

    protected readonly ControlLayouter<TField, TDAO> Layouter;
    protected OxColorHelper FontColors;
    protected readonly Dictionary<OxPanel, OxHeader> Headers = new();
    protected readonly List<OxPanel> panels = new();
    protected readonly Dictionary<OxPanel, ControlLayouts<TField>> LayoutsLists = new();
    public void ScrollToTop() =>
        //ContentBox.AutoScrollPosition = new(0, 0);
        AutoScrollPosition = new(0, 0);

    private ItemsView<TField, TDAO>? itemsView;
    public ItemsView<TField, TDAO>? ItemsView
    {
        get => itemsView;
        set => itemsView = value;
    }

    public override void OnDockChanged(OxDockChangedEventArgs e)
    {
        base.OnDockChanged(e);
        SetSizes();
    }
}