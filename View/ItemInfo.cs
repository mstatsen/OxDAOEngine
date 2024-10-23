using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data;
using OxDAOEngine.Settings;
using OxDAOEngine.ControlFactory.Accessors;

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
                if (item != null && item.Equals(value))
                    return;

                if (item != null)
                    item.ChangeHandler -= ItemChangeHandler;

                item = value;

                if (Item != null)
                    Item.ChangeHandler += ItemChangeHandler;

                RenewControls();
            }
        }

        private void RenewControls()
        {
            BaseColor = ControlFactory.ItemColorer.BaseColor(item);
            FontColors.BaseColor = ControlFactory.ItemColorer.ForeColor(item);

            if (Expanded)
                PrepareControls();

            PrepareColors();
        }

        protected override Color FunctionColor => DefaultColor;

        public ItemInfo() : base()
        {
            ContentContainer.AutoScroll = true;
            PreparePanels();
            Layouter = Builder.Layouter;
            SetSizes();
            FontColors = new OxColorHelper(DefaultForeColor);
            SettingsAvailable = false;
            Header.Icon = Icon;
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
            Margins.SetSize(OxSize.None);
            Paddings.SetSize(OxSize.Large);
            Header.SetContentSize(Header.Width, 36);
        }

        public OxPane AsPane => this;

        protected override void PrepareColors()
        {
            base.PrepareColors();
            Header.BaseColor = Colors.Darker(
                SettingsManager.Settings<GeneralSettings>().DarkerHeaders ? 1 : 0
            );

            if (FontColors != null)
                Header.Label.ForeColor = FontColors.BaseColor;

            foreach (OxPanel panel in panels)
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
            WrapControl(field, new Size(360, 60));
        
        private void WrapControl(TField field, Size size)
        {
            OxLabel? control = 
                (OxLabel?)Layouter.PlacedControl(field)?.Control;

            if (control != null)
            {
                control.MaximumSize = size;
                control.TextAlign = ContentAlignment.TopLeft;
            }
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
            Text = Item == null ? string.Empty : GetTitle();

        protected virtual string? GetTitle() => Item?.ToString();

        protected void PreparePanel(OxPanel panel, string text)
        {
            panel.Parent = this;
            panel.Dock = DockStyle.Top;

            if (text != string.Empty)
                Headers.Add(panel, new OxHeader(text)
                {
                    Parent = this,
                    Dock = DockStyle.Top
                });

            panels.Add(panel);
        }


        protected virtual void PreparePanels() { }

        private void ItemChangeHandler(object sender, DAOEntityEventArgs e) =>
            PrepareControls();

        private void PrepareControls()
        {
            SetTitle();

            if (Item != null)
                Builder.FillControls(Item);
            else
                foreach (ControlLayout<TField> layout in Layouter.Layouts)
                {
                    IControlAccessor placedControl = Builder[layout.Field];

                    if (placedControl != null)
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
            foreach (OxPanel parentPanel in LayoutsLists.Keys)
            {
                Headers.TryGetValue(parentPanel, out var header);

                int lastBottom = header == null ? 0 : 8;
                bool visibleControlsExists = false;
                ControlLayout<TField>? prevLayout = null;

                foreach (ControlLayout<TField> layout in LayoutsLists[parentPanel])
                {
                    bool controlVisible = !Builder[layout.Field].IsEmpty;
                    PlacedControl<TField>? placedControl = Layouter.PlacedControl(layout.Field);

                    if (placedControl == null)
                        continue;

                    placedControl.Visible = controlVisible;

                    if (controlVisible)
                    {
                        visibleControlsExists = true;
                        placedControl.Control.Top = lastBottom + (prevLayout != null ? (layout.Top - prevLayout.Bottom) : 8);
                        OxControlHelper.AlignByBaseLine(placedControl.Control, placedControl.Label!);
                        lastBottom = placedControl.Control.Bottom;
                    }
                    prevLayout = layout;
                }

                parentPanel.Height = lastBottom + 36;
                parentPanel.Visible = visibleControlsExists;

                if (header != null)
                    header.Visible = visibleControlsExists;
            }
        }

        protected virtual void AfterControlLayout() { }

        private void ClearLayouts() =>
            Layouter?.Clear();

        protected override void ApplySettingsInternal()
        {
            if (Observer[DAOSetting.ShowItemInfo])
            {
                if (Visible != Settings.ShowItemInfo)
                    Visible = Settings.ShowItemInfo;
            }

            if (Observer[DAOSetting.ItemInfoPanelPinned] &&
                (Pinned != Settings.ItemInfoPanelPinned))
                Pinned = Settings.ItemInfoPanelPinned;

            if ((Observer[DAOSetting.ItemInfoPanelPinned] || Observer[DAOSetting.ItemInfoPanelExpanded])
                && (Expanded != (Pinned && Settings.ItemInfoPanelExpanded)))
                Expanded = Pinned && Settings.ItemInfoPanelExpanded;

            if (SettingsManager.Settings<GeneralSettings>().Observer[GeneralSetting.DarkerHeaders])
                PrepareColors();
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
            ControlFactory.Builder(ControlScope.FullInfoView);

        protected readonly ControlLayouter<TField, TDAO> Layouter;
        protected OxColorHelper FontColors;
        protected readonly Dictionary<OxPane, OxHeader> Headers = new();
        protected readonly List<OxPanel> panels = new();
        protected readonly Dictionary<OxPanel, ControlLayouts<TField>> LayoutsLists = new();

        protected override Bitmap? GetIcon() => DataManager.ListController<TField, TDAO>().Icon;
    }
}