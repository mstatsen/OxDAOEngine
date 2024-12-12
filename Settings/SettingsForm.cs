﻿using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;
using OxLibrary.Interfaces;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.Part;
using OxDAOEngine.ControlFactory.Controls.Fields;
using OxDAOEngine.ControlFactory.Filter;
using OxLibrary.Geometry;

namespace OxDAOEngine.Settings;

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

        if (ShowDialog(null) is DialogResult.OK)
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

        if (DialogResult is DialogResult.OK)
            GrabControls();
    }

    private readonly SettingsPartHelper partHelper = TypeHelper.Helper<SettingsPartHelper>();

    protected override string EmptyMandatoryField()
    {
        foreach (var item in settingsFieldPanels)
            foreach (SettingsPart part in partHelper.MandatoryFields)
                if (AvailablePart(item.Key, part) 
                    && settingsFieldPanels[item.Key][part].Fields.Count is 0)
                    return $"{item.Key.ListName} {TypeHelper.Name(part)} fields";
       
        return base.EmptyMandatoryField();
    }

    private void SetFormSize()
    {
        int maximumTabWidth = tabControl.TabHeaderSize.Width * tabControl.Pages.Count;

        foreach (OxPanel tab in tabControl.Pages.Cast<OxPanel>())
            if (tab is OxTabControl childTabControl)
                maximumTabWidth = OxSH.Max(
                    maximumTabWidth,
                    childTabControl.TabHeaderSize.Width *
                        childTabControl.Pages.Count
                );

        maximumTabWidth += tabControl.Margin.Horizontal + 24;
        Size = new(
            OxSH.Max(maximumTabWidth, 480),
            488
        );
        MoveToScreenCenter();
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

                if (daoSettings.AvailableCategories)
                    settingsCategoriesPanels[settings].Categories = daoSettings.Categories;
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
            {
                foreach (var item in settingsFieldPanels[settings])
                    daoSettings.SetFields(item.Key, item.Value.Fields);

                if (daoSettings.AvailableCategories)
                    daoSettings.Categories = settingsCategoriesPanels[settings].Categories;
            }
        }
    }

    private static void MagnetLabelWithControl(IOxControl control)
    {
        if (control.Tag is not Control)
            return;

        IOxControl label = (IOxControl)control.Tag;
        label.Parent = control.Parent;
        OxControlHelper.AlignByBaseLine(control, label);
    }

    private void ControlLocationChangeHandler(object? sender, EventArgs e)
    {
        if (sender is not Control)
            return;

        MagnetLabelWithControl((IOxControl)sender);
    }

    private OxPanel CreateParamsPanel(ISettingsController settings, SettingsPart part)
    {
        OxPanel panel = new()
        {
            BaseColor = BaseColor,
            Parent = FormPanel,
            Dock = OxDock.Fill,
            Text = TypeHelper.Name(part)
        };

        panel.Padding.Size = 4;
        settingsPanels[settings].Add(part, panel);
        settingsPartControls[settings].Add(part, new ControlAccessorList());
        settingsTabs[settings].AddPage(panel);
        return panel;
    }

    private void ActivatePage(ISettingsController? settings, SettingsPart part)
    {
        if (part.Equals(SettingsPart.Full) 
            || settings is null)
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
                SettingsPart.Cards =>
                    daoSettings.AvailableCards,
                SettingsPart.Icons =>
                    daoSettings.AvailableIcons,
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
        accessor.Left = OxSH.Mul(180, columnNum + 1);
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
                Left = OxSH.Mul(150, OxSH.Add(columnNum, 12)),
                Font = OxStyles.DefaultFont,
                Text = $"{settings.Helper.Name(setting)}",
                Tag = accessor.Control
            };
            label.Click += ControlLabelClick;
            accessor.Control.Tag = label;
            MagnetLabelWithControl(accessor.Control);
            accessor.Control.LocationChanged += ControlLocationChangeHandler;
            accessor.Control.ParentChanged += ControlLocationChangeHandler;
        }

        ControlPainter.ColorizeControl(accessor, BaseColor);
        settingsPartControls[settings][settingsPart].Add(accessor);
        settingsControls[settings].Add(setting, accessor);
    }

    private void ControlLabelClick(object? sender, EventArgs e)
    {
        if (sender is null)
            return;

        if (((OxLabel)sender).Tag is OxCheckBox checkbox)
            checkbox.Checked = !checkbox.Checked;
    }

    private static short CalcAcessorTop(IControlAccessor? prevAccessor) =>
        OxSH.Add(
            prevAccessor is not null
                ? prevAccessor!.Bottom
                : 4,
            4
        );

    private void CreateCategoriesPanel(IDAOSettings settings)
    {
        if (!AvailablePart(settings, SettingsPart.Category))
            return;

        settingsCategoriesPanels.Add(
            settings, 
            settings.CreateCategoriesPanel(
                settingsPanels[settings][SettingsPart.Category]
            )
        );
    }

    private void CreateCategoriesPanels()
    {
        foreach (ISettingsController settings in SettingsManager.Controllers)
            if (settings is IDAOSettings daoSettings)
                CreateCategoriesPanel(daoSettings);
    }

    private void CreateFieldsPanel(IDAOSettings settings, SettingsPart part)
    {
        if (!AvailablePart(settings, part))
            return;

        settingsFieldPanels[settings].Add(
            part,
            settings.CreateFieldsPanel(
                part,
                settingsPanels[settings][part is SettingsPart.QuickFilterText ? SettingsPart.QuickFilter : part]
            )
        );
    }

    private void CreateFieldsPanels()
    {
        foreach (ISettingsController settings in SettingsManager.Controllers)
            if (settings is IDAOSettings daoSettings)
                foreach (SettingsPart part in partHelper.FieldsSettings)
                    CreateFieldsPanel(daoSettings, part);
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
        CreateCategoriesPanels();
        CreateFramesForControls();
        CreateDefaultButtons();
    }

    private OxFrameWithHeader CreateFrame(ISettingsController settings, SettingsPart part, string text = "")
    { 
        OxFrameWithHeader frame = new()
        {
            Parent = settingsPanels[settings][part],
            Dock = OxDock.Top,
            Text = text,
            BaseColor = BaseColor
        };
        frame.Margin.Size = 4;
        frame.HeaderVisible = !text.Equals(string.Empty);
        return frame;
    }

    private OxFrameWithHeader? RelocateControls(ISettingsController settings, SettingsPart part,
        List<string>? settingList = null, string caption = "")
    {
        if (!AvailablePart(settings, part))
            return null;

        settingList ??= settings.Helper.ItemsByPart(part);

        if (settingList is null 
            || settingList.Count is 0)
            return null;

        OxFrameWithHeader frame = CreateFrame(settings, part, caption);
        IControlAccessor? lastAccessor = null;
        short maxLabelWidth = 0;

        foreach (string setting in settingList)
        {
            settingsControls[settings][setting].Parent = frame;
            settingsControls[settings][setting].Top = CalcAcessorTop(lastAccessor);

            if (!settings.Helper.WithoutLabel(setting))
                maxLabelWidth = Math.Max(
                    maxLabelWidth,
                    ((OxLabel)settingsControls[settings][setting].Control.Tag!).Width
                );

            lastAccessor = settingsControls[settings][setting];
        }

        foreach (string setting in settingList)
            settingsControls[settings][setting].Control.Left =
                OxSH.Short(
                    settings.Helper.WithoutLabel(setting)
                        ? 8
                        : maxLabelWidth + 24
                );

        frame.Size = new(
            frame.Width,
            OxSH.Add(
                lastAccessor is not null ? lastAccessor!.Bottom : 0,
                caption.Equals(string.Empty) ? 0 : frame.Header.Height,
                16
            )
        );

        return frame;
    }

    private void CreateFramesForControls()
    {
        foreach (ISettingsController settings in SettingsManager.Controllers)
            foreach (SettingsPart part in PartList(settings))
                RelocateControls(settings, part);
    }

    private void CreateDefaulter(DefaulterScope scope)
    {
        short left = 4;

        foreach (OxButton existButton in defaulters.Keys)
            left = Math.Max(left, existButton.Right);

        DefaulterScopeHelper helper = TypeHelper.Helper<DefaulterScopeHelper>();
        left += helper.DefaultButtonsSpace;
        OxButton button = new(helper.Name(scope), OxIcons.Eraser)
        {
            Parent = Footer,
            BaseColor = BaseColor,
            Top = OxSH.CenterOffset(Footer.Height, helper.DefaultButtonHeight),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
            Font = OxStyles.Font(-1, FontStyle.Regular),
            Left = left,
            Size = new(
                helper.Width(scope),
                helper.DefaultButtonHeight
            )
        };
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
            if (settings.Helper.Part(item.Key).Equals(part))
                item.Value.Value = settings.GetDefault(item.Key);

        if (partHelper.IsFieldsSettings(part))
            settingsFieldPanels[settings][part].ResetFields();

        if (part.Equals(SettingsPart.Category))
            settingsCategoriesPanels[settings].ResetToDefault();

        if (part is SettingsPart.QuickFilter)
            SetDefaultForPart(settings, SettingsPart.QuickFilterText);
    }

    private void DefaultButtonClickHandler(object? sender, EventArgs e)
    {
        if (sender is null)
            return;

        DefaulterScope scope = defaulters[(OxButton)sender];

        if (scope is DefaulterScope.All
            && !OxMessage.Confirmation(
                    "Are you sure you want to reset all settings to the default values?", 
                    this
                )
            )
            return;

        foreach (var item in settingsPanels)
            foreach (var partPanel in item.Value)
                if (scope is DefaulterScope.All
                    || partPanel.Value.Equals(settingsTabs[item.Key].ActivePage))
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
    private readonly Dictionary<ISettingsController, ICategoriesPanel> settingsCategoriesPanels = new();
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
                item.Value.HeaderVisible = true;
            else
            {
                item.Value.HeaderVisible = false;
                settingsFieldPanels[item.Key][SettingsPart.Table].Text = "Fields";
            }
        }
    }

    private void PrepareTabControl()
    {
        tabControl.Parent = FormPanel;
        tabControl.Dock = OxDock.Fill;
        tabControl.BaseColor = BaseColor;
        tabControl.Font = OxStyles.DefaultFont;
        tabControl.TabHeaderSize = new(124, 32);
        tabControl.BorderVisible = false;
        tabControl.Margin.Size = 0;
        tabControl.Margin.Top = 8;
    }

    private void CreateSettingsTabs()
    {
        foreach (ISettingsController settings in SettingsManager.Controllers)
        {
            OxTabControl tab = new()
            {
                Parent = FormPanel,
                Dock = OxDock.Fill,
                BaseColor = BaseColor,
                Font = OxStyles.DefaultFont,
                TabHeaderSize = new(84, 30),
                BorderVisible = false,
                Text = settings.ListName,
            };
            tab.Margin.Size = 0;
            tab.Margin.Top = 8;
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