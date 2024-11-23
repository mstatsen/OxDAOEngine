using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings;

namespace OxDAOEngine.View
{
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

            if (Expanded)
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
            SettingsAvailable = false;
            Icon = DataManager.ListController<TField, TDAO>().Icon;
        }

        protected override void OnExpandedChanged(ExpandedChangedEventArgs e)
        {
            base.OnExpandedChanged(e);
            RenewControls();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            RenewControls();
        }

        private void SetSizes()
        {
            Margin.Size = OxWh.W2;
            Margin[OxDockHelper.Opposite(Dock)].Size = OxWh.W0;

            if (Dock is OxDock.Right)
                Margin.Bottom = OxWh.W0;

            Margin.Top = 
                Dock is OxDock.Bottom 
                    ? OxWh.W1
                    : Pinned 
                        ? OxWh.W9
                        : OxWh.W4;

            Margin.Right = OxWh.W0;
            Padding.Size = OxWh.W4;
            HeaderHeight = OxWh.W36;
        }

        protected override void OnPinnedChanged(PinnedChangedEventArgs e) =>
            SetSizes();

        public OxPane AsPane => this;

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (FontColors is not null)
                Header.Label.ForeColor = FontColors.BaseColor;

            foreach (OxPane panel in panels)
                panel.BaseColor = BaseColor;

            foreach (OxHeader header in Headers.Values)
            {
                header.BaseColor = BaseColor;
                header.Label.ForeColor = Header.Label.ForeColor;
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
            Layouter.Template.Left = OxWh.W0;
            Layouter.Template.Top = OxWh.W8;
            Layouter.Template.CaptionVariant = ControlCaptionVariant.Left;
            Layouter.Template.WrapLabel = true;
            Layouter.Template.MaximumLabelWidth = OxWh.W80;
            Layouter.Template.BackColor = Color.Transparent;
            Layouter.Template.FontColor = FontColors.BaseColor;
            Layouter.Template.FontStyle = FontStyle.Bold;
            Layouter.Template.LabelColor = FontColors.Lighter();
            Layouter.Template.LabelStyle = FontStyle.Italic;
            Layouter.Template.AutoSize = true;
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

        protected void PreparePanel(OxPane panel, string text)
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
                        placedControl.Control.Visible = false;
                }

            ClearLayouts();
            PrepareLayoutsInternal();
            LayoutControls();
            RecalcControlsVisible();
            AfterControlLayout();
        }

        private void RecalcControlsVisible()
        {
            foreach (OxPane parentPanel in LayoutsLists.Keys)
            {
                Headers.TryGetValue(parentPanel, out var header);

                OxWidth lastBottom = header is null ? OxWh.W0 : OxWh.W8;
                OxWidth maxBottom = lastBottom;
                bool visibleControlsExists = false;
                ControlLayout<TField>? prevLayout = null;

                foreach (ControlLayout<TField> layout in LayoutsLists[parentPanel])
                {
                    bool controlVisible = !Builder[layout.Field].IsEmpty;
                    PlacedControl<TField>? placedControl = Layouter.PlacedControl(layout.Field);

                    if (placedControl is null)
                        continue;

                    placedControl.Visible = controlVisible;

                    if (controlVisible)
                    {
                        visibleControlsExists = true;
                        placedControl.Control.Top =
                            OxWh.Int(
                                lastBottom 
                                | (prevLayout is not null 
                                    ? OxWh.Sub(layout.Top, prevLayout.Bottom) 
                                    : OxWh.W8
                                )
                            );
                        OxControlHelper.AlignByBaseLine(placedControl.Control, placedControl.Label!);
                        lastBottom = OxWh.W(placedControl.Control.Bottom);
                        maxBottom = OxWh.Max(maxBottom, lastBottom);
                    }

                    prevLayout = layout;
                }

                parentPanel.Height = maxBottom | OxWh.W36;
                parentPanel.Visible = visibleControlsExists;
                HeaderVisible = visibleControlsExists;
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
            Settings.ItemInfoPanelPinned = Pinned;
            Settings.ItemInfoPanelExpanded = Expanded;
        }

        protected ControlFactory<TField, TDAO> ControlFactory = 
            DataManager.ControlFactory<TField, TDAO>();

        protected ControlBuilder<TField, TDAO> Builder =>
            ControlFactory.Builder(ControlScope.InfoView);

        protected readonly ControlLayouter<TField, TDAO> Layouter;
        protected OxColorHelper FontColors;
        protected readonly Dictionary<OxPane, OxHeader> Headers = new();
        protected readonly List<OxPane> panels = new();
        protected readonly Dictionary<OxPane, ControlLayouts<TField>> LayoutsLists = new();
        public void ScrollToTop() =>
            //ContentBox.AutoScrollPosition = new(0, 0);
            AutoScrollPosition = new(0, 0);

        private ItemsView<TField, TDAO>? itemsView;
        public ItemsView<TField, TDAO>? ItemsView
        {
            get => itemsView;
            set => itemsView = value;
        }

        protected override void OnDockChanged(EventArgs e)
        {
            base.OnDockChanged(e);
            SetSizes();
        }
    }
}