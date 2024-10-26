using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Dialogs;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.Part;
using OxDAOEngine.ControlFactory.Controls.Fields;

namespace OxDAOEngine.Settings
{
    public partial class SettingsForm : OxDialog
    {
        private class SettingsPartDictionary<T> : Dictionary<SettingsPart, T> { }
        private class SettingsDictionary<T, U, V> : Dictionary<ISettingsController, T>
            where T : Dictionary<U, V>, new()
            where U : notnull
        {
            public void Add(ISettingsController settings) =>
                Add(settings, new T());

            public List<V> List
            {
                get
                {
                    List<V> result = new();

                    foreach (T t in Values)
                        result.AddRange(t.Values);

                    return result;
                }
            }
        };

        private class SettingsDictionray<T, U> : SettingsDictionary<T, SettingsPart, U>
            where T : SettingsPartDictionary<U>, new() { }
        private class SettingsPartPanels : SettingsPartDictionary<OxPanel> { }
        private class SettingsPanels : SettingsDictionray<SettingsPartPanels, OxPanel> { }
        private class SettingsPartControls : SettingsDictionray<SettingsPartDictionary<ControlAccessorList>, ControlAccessorList> { }
        private class SettingsFieldPanels : SettingsDictionray<SettingsPartDictionary<IFieldsPanel>, IFieldsPanel> { }
        private class SettingsControls : SettingsDictionary<Dictionary<string, IControlAccessor>, string, IControlAccessor> { }

        public override Bitmap FormIcon =>
            OxIcons.Settings;

        public SettingsForm()
        {
            BaseColor = EngineStyles.SettingsFormColor;
            InitializeComponent();
            PrepareTabControl();
            CreateSettingsTabs();
            CreatePanels();
            CreateControls();
            MainPanel.DialogButtonStartSpace = 8;
            MainPanel.DialogButtonSpace = 4;
            SetSettingsTabButtonsVisible();
            foreach (OxTabControl tabControl in settingsTabs.Values)
                tabControl.ActivateFirstPage();
        }

        private void ShowSettingsInternal(ISettingsController settings, SettingsPart part)
        {
            if (!AvailablePart(settings, part))
                return;

            startedSettings = settings;
            startedSettingsPart = part;
            DataReceivers.SaveSettings();
            Text = "Settings";
            FillControls();

            if (ShowDialog(null) == DialogResult.OK)
                DataReceivers.ApplySettings();
        }

        public static void ShowSettings(ISettingsController settings, SettingsPart part) =>
            Instance.ShowSettingsInternal(settings, part);

        public static readonly SettingsForm Instance = new();

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ActivatePage(startedSettings, startedSettingsPart);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            if (DialogResult == DialogResult.OK)
                GrabControls();
        }

        private SettingsPartHelper partHelper = TypeHelper.Helper<SettingsPartHelper>();

        protected override string EmptyMandatoryField()
        {
            foreach (var item in settingsFieldPanels)
                foreach (SettingsPart part in partHelper.MandatoryFields)
                    if (AvailablePart(item.Key, part) && 
                        settingsFieldPanels[item.Key][part].Fields.Count == 0)
                        return $"{item.Key.ListName} {TypeHelper.Name(part)} fields";
           
            return base.EmptyMandatoryField();
        }

        private void SetFormSize()
        {
            int maximumTabWidth = tabControl.TabHeaderSize.Width * tabControl.Pages.Count;

            foreach (OxPane tab in tabControl.Pages.Cast<OxPane>())
                if (tab is OxTabControl childTabControl)
                {
                    maximumTabWidth = Math.Max(maximumTabWidth, 
                        childTabControl.TabHeaderSize.Width * childTabControl.Pages.Count);
                }

            maximumTabWidth += tabControl.Margins.Left + tabControl.Margins.Right + 24;
            SetContentSize(
                Math.Max(maximumTabWidth, 480),
                488
            );
            OxControlHelper.CenterForm(this);
        }

        private void FillControls()
        {
            foreach (ISettingsController settings in SettingsManager.Controllers)
            {
                foreach (var item in settingsControls[settings])
                    item.Value.Value = settings[item.Key];

                if (settings is IDAOSettings daoSettings)
                {
                    foreach (var item in settingsFieldPanels[settings])
                        item.Value.Fields = daoSettings.GetFields(item.Key);
                }
            }
        }

        private void GrabControls()
        {
            foreach (ISettingsController settings in SettingsManager.Controllers)
            {
                foreach (var item in settingsControls[settings])
                    settings[item.Key] = item.Value.Value;

                if (settings is IDAOSettings daoSettings)
                    foreach (var item in settingsFieldPanels[settings])
                        daoSettings.SetFields(item.Key, item.Value.Fields);
            }
        }

        private static void MagnetLabelWithControl(Control control)
        {
            if ((control.Tag is not Control))
                return;

            Control label = (Control)control.Tag;
            label.Parent = control.Parent;
            OxControlHelper.AlignByBaseLine(control, label);
        }

        private void ControlLocationChangeHandler(object? sender, EventArgs e)
        {
            if (sender is not Control)
                return;

            MagnetLabelWithControl((Control)sender);
        }

        private OxPanel CreateParamsPanel(ISettingsController settings, SettingsPart part)
        {
            OxPanel panel = new()
            {
                BaseColor = MainPanel.BaseColor,
                Parent = MainPanel,
                Dock = DockStyle.Fill,
                Text = TypeHelper.Name(part)
            };

            panel.Paddings.SetSize(OxSize.Large);
            settingsPanels[settings].Add(part, panel);
            settingsPartControls[settings].Add(part, new ControlAccessorList());
            settingsTabs[settings].AddPage(panel);
            return panel;
        }

        private void ActivatePage(ISettingsController? settings, SettingsPart part)
        {
            if (part == SettingsPart.Full || settings == null)
            {
                tabControl.ActivateFirstPage();
                return;
            }

            tabControl.ActivePage = settingsTabs[settings];
            settingsTabs[settings].ActivePage = settingsPanels[settings][part];
        }

        private List<SettingsPart> PartList(ISettingsController settings) =>
            settings is IDAOSettings
                ? partHelper.VisibleDAOSettings
                : partHelper.VisibleGeneralSettings;

        private static bool AvailablePart(ISettingsController settings, SettingsPart part) => 
            settings is not IDAOSettings daoSettings 
            || part switch
                {
                    SettingsPart.Category =>
                        daoSettings.AvailableCategories,
                    SettingsPart.QuickFilter or
                    SettingsPart.QuickFilterText =>
                        daoSettings.AvailableQuickFilter,
                    SettingsPart.View =>
                        daoSettings.AvailableCards || daoSettings.AvailableIcons,
                    _ => true
                };

        private void CreatePanels()
        {
            foreach (ISettingsController settings in SettingsManager.Controllers)
                foreach (SettingsPart part in PartList(settings))
                {
                    if (!AvailablePart(settings, part))
                        continue;
                    
                    CreateParamsPanel(settings, part);
                }
        }

        private void CreateControl(ISettingsController settings, string setting, int columnNum = 0)
        {
            IControlAccessor accessor = settings.Accessor(setting);
            SettingsPart settingsPart = settings.Helper.Part(setting);

            if (!AvailablePart(settings, settingsPart))
                return;

            accessor.Parent = settingsPanels[settings][settingsPart];
            accessor.Left = 180 * (columnNum + 1);
            accessor.Top = CalcAcessorTop(
                settingsPartControls[settings][settingsPart].Last
            );

            if (accessor.Control is not OxCheckBox)
                accessor.Width = settings.Helper.ControlWidth(setting);

            if (!settings.Helper.WithoutLabel(setting))
            {
                OxLabel label = new()
                {
                    Parent = accessor.Parent,
                    Left = 12 + 150 * columnNum,
                    Font = Styles.DefaultFont,
                    Text = $"{settings.Helper.Name(setting)}",
                    Tag = accessor.Control
                };
                label.Click += ControlLabelClick;
                accessor.Control.Tag = label;
                MagnetLabelWithControl(accessor.Control);
                accessor.Control.LocationChanged += ControlLocationChangeHandler;
                accessor.Control.ParentChanged += ControlLocationChangeHandler;
            }

            ControlPainter.ColorizeControl(accessor, MainPanel.BaseColor);
            settingsPartControls[settings][settingsPart].Add(accessor);
            settingsControls[settings].Add(setting, accessor);
        }

        private void ControlLabelClick(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            if (((OxLabel)sender).Tag is OxCheckBox checkbox)
                checkbox.Checked = !checkbox.Checked;
        }

        private static int CalcAcessorTop(IControlAccessor? prevAccessor) =>
            (prevAccessor != null ? prevAccessor.Bottom : 4) + 4;

        private void CreateFieldsPanel(IDAOSettings settings, SettingsPart part)
        {
            if (!AvailablePart(settings, part))
                return;

            settingsFieldPanels[settings].Add(
                part,
                settings.CreateFieldsPanel(
                    part,
                    settingsPanels[settings][part == SettingsPart.QuickFilterText ? SettingsPart.QuickFilter : part]
                )
            );
        }

        private void CreateFieldsPanels()
        {
            foreach (ISettingsController settings in SettingsManager.Controllers)
                if (settings is IDAOSettings daoSettings)
                    foreach (SettingsPart part in partHelper.FieldsSettings)
                    {
                        if (!AvailablePart(settings, part))
                            continue;

                        CreateFieldsPanel(daoSettings, part);
                    }
        }

        private void CreateControls()
        {
            foreach (ISettingsController settings in SettingsManager.Controllers)
                foreach (string setting in settings.Helper.VisibleItems)
                    try
                    {
                        CreateControl(settings, setting);
                    }
                    catch
                    {
                        OxMessage.ShowError($"Can not create control for {setting} setting.", this);
                    }

            CreateFieldsPanels();
            CreateFramesForControls();
            CreateDefaultButtons();
        }

        private OxFrameWithHeader CreateFrame(ISettingsController settings, SettingsPart part, string text = "")
        { 
            OxFrameWithHeader frame = new()
            {
                Parent = settingsPanels[settings][part],
                Dock = DockStyle.Top,
                Text = text,
                BaseColor = MainPanel.BaseColor
            };
            frame.Margins.SetSize(OxSize.Large);

            if (frame.Header != null)
                frame.Header.Visible = text != string.Empty;

            return frame;
        }

        private void RelocateControls(ISettingsController settings, SettingsPart part,
            List<string>? settingList = null, string caption = "")
        {
            if (!AvailablePart(settings, part))
                return;

            settingList ??= settings.Helper.ItemsByPart(part);

            if (settingList == null || settingList.Count == 0)
                return;

            OxFrameWithHeader frame = CreateFrame(settings, part, caption);
            IControlAccessor? lastAccessor = null;
            int maxLabelWidth = 0;

            foreach (string setting in settingList)
            {
                settingsControls[settings][setting].Parent = frame;
                settingsControls[settings][setting].Top = CalcAcessorTop(lastAccessor);

                if (!settings.Helper.WithoutLabel(setting))
                    maxLabelWidth = Math.Max(
                        maxLabelWidth,
                        ((OxLabel)settingsControls[settings][setting].Control.Tag).Width
                    );

                lastAccessor = settingsControls[settings][setting];
            }

            foreach (string setting in settingList)
                settingsControls[settings][setting].Control.Left = 
                    settings.Helper.WithoutLabel(setting) 
                        ? 8 
                        : maxLabelWidth + 24;

            frame.SetContentSize(
                frame.Width,
                (lastAccessor != null ? lastAccessor.Bottom : 0)
                + (caption != string.Empty ? frame.Header.Height : 0)
                + 16
            );
        }

        private void CreateFramesForControls()
        {
            foreach (ISettingsController settings in SettingsManager.Controllers)
            {
                if (settings is IDAOSettings)
                {
                    RelocateControls(settings, SettingsPart.View, settings.Helper.CardSettingsItems, "Cards");
                    RelocateControls(settings, SettingsPart.View, settings.Helper.IconSettingsItems, "Icons");
                }

                foreach (SettingsPart part in PartList(settings))
                    if (part != SettingsPart.View)
                        RelocateControls(settings, part);
            }
        }

        private void CreateDefaulter(DefaulterScope scope)
        {
            int left = 4;

            foreach (OxButton existButton in defaulters.Keys)
                left = Math.Max(left, existButton.Right);

            DefaulterScopeHelper helper = TypeHelper.Helper<DefaulterScopeHelper>();
            left += DefaulterScopeHelper.DefaultButtonsSpace;
            OxButton button = new(helper.Name(scope), OxIcons.Eraser)
            {
                Parent = MainPanel.Footer,
                BaseColor = MainPanel.BaseColor,
                Top = (MainPanel.Footer.Height - DefaulterScopeHelper.DefaultButtonHeight) / 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                Font = Styles.Font(-1, FontStyle.Regular),
                Left = left
            };

            button.SetContentSize(
                helper.Width(scope),
                DefaulterScopeHelper.DefaultButtonHeight);
            button.Click += DefaultButtonClickHandler;
            defaulters.Add(button, scope);
        }

        private void CreateDefaultButtons()
        {
            foreach (DefaulterScope scope in TypeHelper.All<DefaulterScope>())
                CreateDefaulter(scope);
        }

        private void SetDefaultForPart(ISettingsController settings, SettingsPart part)
        {
            foreach (var item in settingsControls[settings])
                if (settings.Helper.Part(item.Key) == part)
                    item.Value.Value = settings.GetDefault(item.Key);

            if (partHelper.IsFieldsSettings(part))
                settingsFieldPanels[settings][part].ResetFields();

            if (part == SettingsPart.QuickFilter)
                SetDefaultForPart(settings, SettingsPart.QuickFilterText);
        }

        private void DefaultButtonClickHandler(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            DefaulterScope scope = defaulters[(OxButton)sender];

            if (scope == DefaulterScope.All
                && !OxMessage.Confirmation("Are you sure you want to reset all settings to the default values?", this))
                return;

            foreach (var item in settingsPanels)
                foreach (var partPanel in item.Value)
                    if (scope == DefaulterScope.All ||
                        partPanel.Value == settingsTabs[item.Key].ActivePage)
                        SetDefaultForPart(item.Key, partPanel.Key);
        }

        private readonly OxTabControl tabControl = new();
        private SettingsPart startedSettingsPart = SettingsPart.Table;
        private ISettingsController? startedSettings = null;
        private readonly SettingsPanels settingsPanels = new();
        private readonly Dictionary<ISettingsController, OxTabControl> settingsTabs = new();
        private readonly SettingsPartControls settingsPartControls = new();
        private readonly SettingsControls settingsControls = new();
        private readonly SettingsFieldPanels settingsFieldPanels = new();
        private readonly Dictionary<OxButton, DefaulterScope> defaulters = new();

        private void SettingsForm_Shown(object? sender, EventArgs e) =>
            SetFormSize();

        private void SetSettingsTabButtonsVisible()
        {
            foreach (KeyValuePair<ISettingsController, OxTabControl> item in settingsTabs)
            {
                if (item.Key is not IDAOSettings daoSettings)
                    continue;

                if (daoSettings.AvailableCards ||
                    daoSettings.AvailableIcons ||
                    daoSettings.AvailableCategories ||
                    daoSettings.AvailableQuickFilter ||
                    daoSettings.AvailableSummary)
                    item.Value.Header.Visible = true;
                else
                {
                    item.Value.Header.Visible = false;
                    settingsFieldPanels[item.Key][SettingsPart.Table].Text = "Fields";
                }
            }
        }

        private void PrepareTabControl()
        {
            tabControl.Parent = this;
            tabControl.Dock = DockStyle.Fill;
            tabControl.BaseColor = MainPanel.BaseColor;
            tabControl.Font = Styles.DefaultFont;
            tabControl.TabHeaderSize = new Size(124, 32);
            tabControl.BorderVisible = false;
            tabControl.Margins.SetSize(OxSize.None);
            tabControl.Margins.TopOx = OxSize.Extra;
        }

        private void CreateSettingsTabs()
        {
            foreach (ISettingsController settings in SettingsManager.Controllers)
            {
                OxTabControl tab = new()
                {
                    Parent = this,
                    Dock = DockStyle.Fill,
                    BaseColor = MainPanel.BaseColor,
                    Font = Styles.DefaultFont,
                    TabHeaderSize = new Size(84, 30),
                    BorderVisible = false,
                    Text = settings.ListName,
                };
                tab.Margins.SetSize(OxSize.None);
                tab.Margins.TopOx = OxSize.Extra;
                tabControl.AddPage(tab, settings.Icon);
                settingsTabs.Add(settings, tab);
                settingsPanels.Add(settings);
                settingsPartControls.Add(settings);
                settingsControls.Add(settings);

                if (settings is IDAOSettings)
                    settingsFieldPanels.Add(settings);
            }
        }
    }
}