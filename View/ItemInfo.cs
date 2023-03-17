using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory;
using OxXMLEngine.Data;
using OxXMLEngine.Settings;

namespace OxXMLEngine.View
{
    public abstract class ItemInfo<TField, TDAO, TFieldGroup>
        : FunctionsPanel<TField, TDAO>, IItemInfo<TField, TDAO>
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
                if (item != null)
                    item.ChangeHandler -= ItemChangeHandler;

                Builder.DetachControlsFromParent();
                item = value;
                
                BaseColor = ControlFactory.ItemColorer.BaseColor(item);
                FontColors.BaseColor = ControlFactory.ItemColorer.ForeColor(item);
                PrepareControls();
                PrepareColors();

                if (Item != null)
                    Item.ChangeHandler += ItemChangeHandler;
            }
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
        }

        private void SetSizes()
        {
            Margins.SetSize(OxSize.None);
            Paddings.SetSize(OxSize.Large);
            Header.SetContentSize(Header.Width, 36);
        }

        //public override Color DefaultColor => EngineStyles.CardColor;

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

            foreach (ControlLayouts<TField> list in LayoutsLists)
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
            Layouter.Template.LabelColor = FontColors.Lighter(1);
            Layouter.Template.LabelStyle = FontStyle.Italic;
            Layouter.Template.AutoSize = true;
        }

        private void PrepareLayoutsInternal()
        {
            ClearLayoutTemplate();
            PrepareLayouts();
        }

        protected abstract void PrepareLayouts();
           

        private void SetTitle() =>
            Text = GetTitle();

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

            ClearLayouts();
            PrepareLayoutsInternal();
            LayoutControls();
            AfterControlLayout();
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
        protected readonly List<ControlLayouts<TField>> LayoutsLists = new();
    }
}