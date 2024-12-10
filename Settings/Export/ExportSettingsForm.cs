using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;
using OxLibrary.Interfaces;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Controls.Fields;
using OxDAOEngine.ControlFactory.Controls.Sorting;
using OxDAOEngine.ControlFactory.Filter;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Fields.Types;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Sorting.Types;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Export;
using OxDAOEngine.Grid;
using OxDAOEngine.Settings.Export;
using OxLibrary.Geometry;

namespace OxDAOEngine.Settings;

public partial class ExportSettingsForm<TField, TDAO> : OxDialog
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
{
    public override Bitmap FormIcon =>
        OxIcons.Export;

    private static readonly ControlBuilder<TField, TDAO> Builder = 
        DataManager.Builder<TField, TDAO>(ControlScope.Export);

    public ExportSettingsForm(ExportSettings<TField, TDAO> exportSettings) 
    {
        CreateQuickFilter();
        CreateCategoriesTree();
        settings = exportSettings;
        InitializeComponent();
        Text = "Export";
        BaseColor = EngineStyles.SettingsFormColor;
        CreateExtraSettingsFrames();

        if (ListController.AvailableQuickFilter)
        { 
            quickFilter!.Parent = FormPanel;
            quickFilter!.Margin.Horizontal = 8;
        }

        PrepareHTMLControls();

        htmlIncludeParamsAccessor = CreateIncludeParamsControl(ExportFormat.Html);
        htmlSummaryAccessor = CreateSummaryControl(ExportFormat.Html);
        zeroSummaryAccessor = CreateZeroSummaryAccessor();

        indentAccessor = Builder.Accessor("XmlIndent", FieldType.Boolean);
        indentAccessor.Text = "Indent XML elements";
        indentAccessor.Value = settings.XML.Indent;
        SetupControl((IOxControl)indentAccessor.Control, ExportFormat.Xml, null, Colors.Lighter());

        PrepareTextControls();

        textIncludeParamsAccessor = CreateIncludeParamsControl(ExportFormat.Text);
        textSummaryAccessor = CreateSummaryControl(ExportFormat.Text);

        selectedItemsPanel = CreateFrame($"Selected {DataManager.ListController<TField, TDAO>().ListName}", false);
        PrepareSelectedItemsPanel();
        PrepareSelectedItemsGrid();

        GeneralPanel = CreateFrame("General");
        GeneralPanel.Margin.Top = 8;

        formatAccessor = Builder.Accessor<ExportFormat>();
        formatAccessor.Value = settings.Format;
        formatAccessor.ValueChangeHandler += FormatChangeHandler;
        SetupGeneralControl(formatAccessor.Control, BaseColor, "Export as");


        if (ListController.Settings.AvailableCategories)
        {
            categoryControl = CreateButtonEdit(settings.CategoryName, SelectCategory);
            SetupGeneralControl(categoryControl, Colors.Lighter(), "Category");
        }

        fileControl = CreateButtonEdit(settings.FileName, ShowFileDialog);
        SetupGeneralControl(fileControl, Colors.Lighter(), "File name");
        CalcFramesSizes();
        ActualizeFormatSettings();
        SetFooterButtonText(OxDialogButton.OK, "Export");
        DialogButtonStartSpace = 8;
        DialogButtonSpace = 4;
    }

    private void PrepareSelectedItemsPanel()
    {
        selectedItemsPanel.Parent = FormPanel;
        selectedItemsPanel.Size = new(100, 200);
        selectedItemsPanel.Visible = false;
        selectedItemsPanel.Padding.Size = 0;
    }

    private void PrepareSelectedItemsGrid()
    {
        selectedGrid.Parent = selectedItemsPanel;
        selectedGrid.BorderStyle = BorderStyle.None;
        selectedGrid.GridView.RowTemplate.Height = 20;
        selectedGrid.GridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
        selectedGrid.GridView.ColumnHeadersHeight = 20;
        selectedGrid.GridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
    }

    private void FillSelectedItemsPanel()
    {
        if (selectedItems is not null)
        {
            selectedGrid.ItemsList = selectedItems.Reverse();
            selectedGrid.Fill();
        }
        else
            selectedGrid.ClearGrid();
    }

    private RootListDAO<TField, TDAO>? selectedItems;
    public RootListDAO<TField, TDAO>? SelectedItems 
    { 
        get => selectedItems;
        set => SetSelectedItems(value);
    }

    private void SetSelectedItems(RootListDAO<TField, TDAO>? value)
    {
        selectedItems = value;

        if (quickFilter is not null)
            quickFilter.Visible = 
                value is null 
                    ? FunctionalPanelVisible.Fixed 
                    : FunctionalPanelVisible.Hidden;

        if (categoryControl is not null)
        {
            if (value is not null)
            {
                categoryControl.Visible = false;
                ((OxLabel)categoryControl.Tag).Visible = false;
                fileControl.Top = categoryControl.Top;
            }
            else
            {
                categoryControl.Visible = true;
                ((OxLabel)categoryControl.Tag).Visible = true;
                fileControl.Top = OxSH.Add(categoryControl.Bottom, 8);
                
            }
            OxControlHelper.AlignByBaseLine(fileControl, (OxLabel)fileControl.Tag);
            CalcFrameSize(GeneralPanel);
        }

        selectedItemsPanel.Visible = value is not null;

        if (value is not null)
            FillSelectedItemsPanel();
    }

    private void CreateCategoriesTree()
    {
        if (!ListController.AvailableCategories)
            return;

        categoriesTree = new()
        {
            ShowCount = false,
            Visible = FunctionalPanelVisible.Fixed
        };
    }

    private void CreateQuickFilter()
    {
        if (!ListController.AvailableQuickFilter)
            return;

        quickFilter = new(QuickFilterVariant.Export)
        {
            Dock = OxDock.Top,
            Visible = FunctionalPanelVisible.Fixed
        };
        quickFilter.Margin.Bottom = 8;
    }

    private OxButtonEdit CreateButtonEdit(string? value, EventHandler? onClick)
    {
        OxButtonEdit buttonEdit = new()
        {
            Value = value,
            Font = OxStyles.DefaultFont,
            BaseColor = Colors.Lighter()
        };
        buttonEdit.OnButtonClick += onClick;
        return buttonEdit;
    }

    private static OxPanel CreateExtraPanel(short height) => new()
    {
        Dock = OxDock.Top,
        Height = height
    };

    private void PrepareExtraPanel(OxPanel panel, ExportFormat format)
    {
        OxFrame parentFrame = extraSettingsFrames[format];
        panel.Parent = parentFrame;
        panel.BaseColor = BaseColor;
        FramesControls[parentFrame].Add(panel);
    }

    private void PrepareTextControls()
    {
        PrepareExtraPanel(textsPanel, ExportFormat.Text);
        PrepareExtraPanel(textGeneralPanel, ExportFormat.Text);
        PrepareFieldsPanel(inlineFieldsPanel, textsPanel, OxDock.Left);
        PrepareGroupByPanel();
    }

    private IControlAccessor CreateIncludeParamsControl(ExportFormat format)
    {
        IControlAccessor accessor = Builder.Accessor("IncludeParams", FieldType.Boolean, format);
        accessor.Value = format is ExportFormat.Html
            ? settings.HTML.IncludeExportParams
            : settings.Text.IncludeExportParams;
        accessor.Text = $"Add export parameters info to {TypeHelper.Name(format).ToLower()}";
        OxPanel parentPane = format is ExportFormat.Html 
            ? htmlGeneralPanel 
            : textGeneralPanel;
        SetupControl((IOxControl)accessor.Control, format, parentPane, Colors.Lighter());
        return accessor;
    }

    private void PrepareHTMLControls()
    {
        PrepareExtraPanel(htmlsPanel, ExportFormat.Html);
        PrepareExtraPanel(htmlGeneralPanel, ExportFormat.Html);
        PrepareFieldsPanel(htmlFieldsPanel, htmlsPanel, OxDock.Right);
        htmlSortingPanel.Sortings = settings.HTML.Sorting;
        htmlSortingPanel.Parent = htmlsPanel;
        htmlSortingPanel.Dock = OxDock.Right;
        htmlSortingPanel.Padding.Horizontal = 2;
        htmlSortingPanel.Size = new(168, htmlSortingPanel.Height);
        htmlSortingPanel.Visible = FunctionalPanelVisible.Fixed;
    }

    private IControlAccessor CreateZeroSummaryAccessor()
    {
        IControlAccessor accessor = Builder.Accessor("ZeroSummary", FieldType.Boolean);
        accessor.Value = settings.HTML.ZeroSummary;
        accessor.Text = "Show summary with zero count";
        SetupControl((IOxControl)accessor.Control, ExportFormat.Html, htmlGeneralPanel, Colors.Lighter());
        return accessor;
    }

    private OxFrame CreateFrame(string Caption, bool caledSize = true)
    {
        OxFrame frame = new OxFrameWithHeader()
        {
            Text = Caption,
            Parent = FormPanel,
            Dock = OxDock.Top,
            BaseColor = BaseColor
        };
        frame.Margin.Size = 8;
        frame.Margin.Top = 0;
        frame.Padding.Size = 8;

        if (caledSize)
            FramesControls.Add(frame, new List<IOxControl>());

        return frame;
    }

    private void CreateExtraSettingsFrames()
    {
        foreach (ExportFormat format in TypeHelper.All<ExportFormat>())
            extraSettingsFrames.Add(format, 
                CreateFrame($"{TypeHelper.Name(format)} settings")
            );
    }

    protected override string EmptyMandatoryField() => 
        fileControl.IsEmpty 
            ? "File name" 
            : base.EmptyMandatoryField();

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (e.Cancel)
            return;

        if (DialogResult is not DialogResult.OK)
            return;

        settings.Filter.CopyFrom(quickFilter?.ActiveFilter);
        settings.CategoryName = categoryControl?.Value ?? string.Empty;
        settings.Format = formatAccessor.EnumValue;
        settings.FileName = fileControl.Value ?? string.Empty;
        settings.HTML.Fields = htmlFieldsPanel.Fields;
        settings.HTML.Summary = htmlSummaryAccessor.EnumValue;
        settings.HTML.IncludeExportParams = htmlIncludeParamsAccessor.BoolValue;
        settings.HTML.ZeroSummary = zeroSummaryAccessor.BoolValue;
        settings.Text.Fields = inlineFieldsPanel.Fields;
        settings.Text.Summary = textSummaryAccessor.EnumValue;
        settings.Text.IncludeExportParams = textIncludeParamsAccessor.BoolValue;
        settings.XML.Indent = indentAccessor.BoolValue;
    }

    private void ExportSettingsForm_Shown(object? sender, EventArgs e)
    {
        if (ListController.AvailableQuickFilter)
        {
            quickFilter!.RenewFilterControls();
            quickFilter!.ActiveFilter = settings.Filter;
        }

        htmlFieldsPanel.Fields = 
            settings.HTML.Fields.Count is 0
                ? TypeHelper.FieldHelper<TField>()
                    .Columns(FieldsVariant.Html, FieldsFilling.Default)
                : settings.HTML.Fields;
        inlineFieldsPanel.Fields = settings.Text.Fields;
        ActualizeFormatSettings();
        MoveToScreenCenter();
    }

    private void ShowFileDialog(object? sender, EventArgs e)
    {
        ExportFormat currentFormat = formatAccessor.EnumValue;
        ExportFormatHelper helper = TypeHelper.Helper<ExportFormatHelper>();
        SaveFileDialog saveDialog = new()
        {
            FileName = string.Empty.Equals(fileControl.Value)
                ? DataManager.ListController<TField, TDAO>().ListName + helper.FileExt(currentFormat)
                : fileControl.Value,
            Filter = helper.FileFilter(currentFormat),
            OverwritePrompt = false
        };

        if (saveDialog.ShowDialog(this) is DialogResult.OK)
            fileControl.Value = saveDialog.FileName;
    }

    private static void PrepareFieldsPanel(FieldsPanel<TField, TDAO> panel, OxPanel parent, OxDock ExtraMarginsDock)
    {
        panel.Parent = parent;
        panel.Dock = OxDock.Fill;
        panel.Margin[ExtraMarginsDock].Size = 8;
        panel.Padding.Horizontal = 2;
    }

    private void PrepareGroupByPanel()
    {
        groupByPanel.Sortings = settings.Text.Grouping;
        groupByPanel.Parent = textsPanel;
        groupByPanel.Dock = OxDock.Left;
        groupByPanel.Padding.Horizontal = 2;
        groupByPanel.Size = new(200, groupByPanel.Height);
        groupByPanel.Visible = FunctionalPanelVisible.Fixed;
    }

    private void SetupControl(IOxControl control, ExportFormat format, OxPanel? parent, Color baseColor, string caption = "") =>
        SetupControl(control, extraSettingsFrames[format], parent, baseColor, caption);

    private void SetupControl(IOxControl control, OxFrame frame, OxPanel? parent, Color baseColor, string caption)
    {
        List<IOxControl> framesControls = FramesControls[frame];
        parent ??= frame;
        control.Top = GetLastControlBottom(frame);
        control.Parent = parent;
        control.Height = 24;
        control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

        if (control is OxCheckBox)
            control.Left = 4;
        else
        {
            OxLabel label = new()
            {
                Parent = parent,
                Left = 0,
                AutoSize = true,
                Text = caption,
                Font = OxStyles.DefaultFont
            };

            control.Left = OxSH.Add(label.Right, 12);
            control.Tag = label;
            OxControlHelper.AlignByBaseLine(control, label);
        }

        ControlPainter.ColorizeControl(control, baseColor);
        framesControls.Add(control);
    }

    private short GetLastControlBottom(OxFrame frame)
    {
        List<IOxControl> framesControls = FramesControls[frame];

        for (int i = framesControls.Count - 1; i > -1; i--)
            if (framesControls[i] is not OxPanel)
                return OxSH.Add(framesControls[i].Bottom, 8);

        return 0;
    }

    private void SetupGeneralControl(IOxControl control, Color baseColor, string caption = "") =>
        SetupControl((IOxControl)control, GeneralPanel, null, baseColor, caption);

    private void CalcExtraPanelSize(OxFrame frame)
    {
        OxPanel? panel =
            frame.Equals(extraSettingsFrames[ExportFormat.Html])
                ? htmlGeneralPanel
                : frame.Equals(extraSettingsFrames[ExportFormat.Text])
                    ? textGeneralPanel
                    : null;

        if (panel is null)
            return;

        panel.Size = new(
            panel.Width,
            GetLastControlBottom(frame)
        );
    }

    private void CalcFrameSize(OxFrame frame)
    {
        CalcExtraPanelSize(frame);
        List<IOxControl> frameControls = FramesControls[frame];

        frame.Size = new(
            frame.Width,
            OxSH.IfElse(
                frame.Equals(extraSettingsFrames[ExportFormat.Html]),
                htmlsPanel.Bottom,
                OxSH.IfElse(
                    frame.Equals(extraSettingsFrames[ExportFormat.Text]),
                    textsPanel.Bottom,
                    OxSH.Add(
                        OxSH.IfElse(
                            frameControls.Count > 0,
                            frameControls[^1].Bottom,
                            24
                        ),
                        frame.Margin.Top,
                        frame.Padding.Vertical,
                        12
                    )
                )
            )
        );

        short maxLeft = 0;

        foreach (IOxControl control in frameControls)
            maxLeft = Math.Max(maxLeft, control.Left);

        foreach (IOxControl control in frameControls)
        {
            if (control is OxCheckBox)
                continue;

            control.Left = maxLeft;
            control.Width =
                OxSH.Sub(
                    frame.Width,
                    maxLeft,
                    frame.Margin.Horizontal,
                    frame.Margin.Left,
                    frame.Padding.Right,
                    12
                );
        }
    }

    private void CalcFramesSizes()
    {
        foreach (OxFrame frame in FramesControls.Keys)
            CalcFrameSize(frame);
    }
       
    private void SelectCategory(object? sender, EventArgs e)
    {
        if (!ListController.AvailableCategories)
            return;

        categoriesTree!.Size = new(360, 480);
        categoriesTree!.RefreshCategories();

        if (categoriesTree.ShowAsDialog(this, OxDialogButton.OK | OxDialogButton.Cancel) is DialogResult.OK)
            categoryControl!.Value = categoriesTree.ActiveCategory?.Name;
    }

    private EnumAccessor<TField, TDAO, ExportSummaryType> CreateSummaryControl(ExportFormat format)
    {
        EnumAccessor<TField, TDAO, ExportSummaryType> accessor = Builder.Accessor<ExportSummaryType>(format);
        
        accessor.Value = format is ExportFormat.Html 
            ? settings.HTML.Summary 
            : settings.Text.Summary;

        OxPanel parentPane = format is ExportFormat.Html
            ? htmlGeneralPanel 
            : textGeneralPanel;

        SetupControl((IOxControl)accessor.Control, format, parentPane, BaseColor, "Summary");
        return accessor;
    }

    private void FormatChangeHandler(object? sender, EventArgs e) => 
        ActualizeFormatSettings();

    private void ActualizeFormatSettings()
    {
        fileControl.Value = settings.GetFileName(formatAccessor.EnumValue);

        foreach (var item in extraSettingsFrames)
            item.Value.Visible = item.Key.Equals(formatAccessor.EnumValue);

        foreach (OxFrame frame in extraSettingsFrames.Values)
            if (frame.Visible)
            {
                FormPanel.Size = new(720, frame.Bottom);
                break;
            }
    }

    private readonly ExportSettings<TField, TDAO> settings;
    private QuickFilterPanel<TField, TDAO>? quickFilter;
    private readonly OxFrame selectedItemsPanel;
    private readonly ItemsRootGrid<TField, TDAO> selectedGrid = new(GridUsage.ViewItems)
    { 
        Dock = OxDock.Fill,
    };
    private readonly OxPanel htmlGeneralPanel = CreateExtraPanel(216);
    private readonly OxPanel htmlsPanel = CreateExtraPanel(144);
    private readonly FieldsPanel<TField, TDAO> htmlFieldsPanel = new(FieldsVariant.Html);
    public readonly SortingPanel<TField, TDAO> htmlSortingPanel = new(SortingVariant.Export, ControlScope.Html);
    private readonly EnumAccessor<TField, TDAO, ExportSummaryType> htmlSummaryAccessor;
    private readonly IControlAccessor htmlIncludeParamsAccessor;
    private readonly IControlAccessor zeroSummaryAccessor;

    private readonly OxPanel textsPanel = CreateExtraPanel(144);
    private readonly OxPanel textGeneralPanel = CreateExtraPanel(84);
    private readonly FieldsPanel<TField, TDAO> inlineFieldsPanel = new(FieldsVariant.Inline);
    public readonly SortingPanel<TField, TDAO> groupByPanel = new(SortingVariant.GroupBy, ControlScope.Grouping);
    private readonly EnumAccessor<TField, TDAO, ExportSummaryType> textSummaryAccessor;
    private readonly IControlAccessor textIncludeParamsAccessor;

    private readonly OxButtonEdit fileControl;
    private readonly OxButtonEdit? categoryControl;
    private readonly EnumAccessor<TField, TDAO, ExportFormat> formatAccessor;

    private readonly IControlAccessor indentAccessor;

    private readonly OxFrame GeneralPanel;
    private readonly Dictionary<ExportFormat, OxFrame> extraSettingsFrames = new();
    private CategoriesTree<TField, TDAO>? categoriesTree;
    private readonly Dictionary<OxFrame, List<IOxControl>> FramesControls = new();
    private readonly IListController<TField, TDAO> ListController = DataManager.ListController<TField, TDAO>();
}