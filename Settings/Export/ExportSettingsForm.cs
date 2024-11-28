using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Filter;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Export;
using OxDAOEngine.Settings.Export;
using OxDAOEngine.Grid;
using OxDAOEngine.Data.Sorting.Types;
using OxDAOEngine.ControlFactory.Controls.Fields;
using OxDAOEngine.ControlFactory.Controls.Sorting;
using OxDAOEngine.Data.Fields.Types;

namespace OxDAOEngine.Settings
{
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
                quickFilter!.Parent = MainPanel;
                quickFilter!.Margin.Horizontal = OxWh.W8;
            }

            PrepareHTMLControls();

            htmlIncludeParamsAccessor = CreateIncludeParamsControl(ExportFormat.Html);
            htmlSummaryAccessor = CreateSummaryControl(ExportFormat.Html);
            zeroSummaryAccessor = CreateZeroSummaryAccessor();

            indentAccessor = Builder.Accessor("XmlIndent", FieldType.Boolean);
            indentAccessor.Text = "Indent XML elements";
            indentAccessor.Value = settings.XML.Indent;
            SetupControl(indentAccessor.Control, ExportFormat.Xml, null, MainPanel.Colors.Lighter());

            PrepareTextControls();

            textIncludeParamsAccessor = CreateIncludeParamsControl(ExportFormat.Text);
            textSummaryAccessor = CreateSummaryControl(ExportFormat.Text);

            selectedItemsPanel = CreateFrame($"Selected {DataManager.ListController<TField, TDAO>().ListName}", false);
            PrepareSelectedItemsPanel();
            PrepareSelectedItemsGrid();

            GeneralPanel = CreateFrame("General");
            GeneralPanel.Margin.Top = OxWh.W8;

            formatAccessor = Builder.Accessor<ExportFormat>();
            formatAccessor.Value = settings.Format;
            formatAccessor.ValueChangeHandler += FormatChangeHandler;
            SetupGeneralControl(formatAccessor.Control, MainPanel.BaseColor, "Export as");


            if (ListController.Settings.AvailableCategories)
            {
                categoryControl = CreateButtonEdit(settings.CategoryName, SelectCategory);
                SetupGeneralControl(categoryControl, MainPanel.Colors.Lighter(), "Category");
            }

            fileControl = CreateButtonEdit(settings.FileName, ShowFileDialog);
            SetupGeneralControl(fileControl, MainPanel.Colors.Lighter(), "File name");
            CalcFramesSizes();
            ActualizeFormatSettings();
            MainPanel.SetFooterButtonText(OxDialogButton.OK, "Export");
            MainPanel.DialogButtonStartSpace = OxWh.W8;
            MainPanel.DialogButtonSpace = OxWh.W4;
        }

        private void PrepareSelectedItemsPanel()
        {
            selectedItemsPanel.Parent = MainPanel;
            selectedItemsPanel.Size = new(OxWh.W100, OxWh.W200);
            selectedItemsPanel.Visible = false;
            selectedItemsPanel.Padding.Size = OxWh.W0;
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
                    fileControl.Top = categoryControl.Bottom + 8;
                    
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
            quickFilter.Margin.Bottom = OxWh.W8;
        }

        private OxButtonEdit CreateButtonEdit(string? value, EventHandler? onClick)
        {
            OxButtonEdit buttonEdit = new()
            {
                Value = value,
                Font = Styles.DefaultFont,
                BaseColor = MainPanel.Colors.Lighter()
            };
            buttonEdit.OnButtonClick += onClick;
            return buttonEdit;
        }

        private static OxPanel CreateExtraPanel(OxWidth height) => new()
        {
            Dock = OxDock.Top,
            Height = height
        };

        private void PrepareExtraPanel(OxPanel panel, ExportFormat format)
        {
            OxFrame parentFrame = extraSettingsFrames[format];
            panel.Parent = parentFrame;
            panel.BaseColor = MainPanel.BaseColor;
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
            SetupControl(accessor.Control, format, parentPane, MainPanel.Colors.Lighter());
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
            htmlSortingPanel.Padding.Horizontal = OxWh.W2;
            htmlSortingPanel.Size = new(OxWh.W168, htmlSortingPanel.Height);
            htmlSortingPanel.Visible = FunctionalPanelVisible.Fixed;
        }

        private IControlAccessor CreateZeroSummaryAccessor()
        {
            IControlAccessor accessor = Builder.Accessor("ZeroSummary", FieldType.Boolean);
            accessor.Value = settings.HTML.ZeroSummary;
            accessor.Text = "Show summary with zero count";
            SetupControl(accessor.Control, ExportFormat.Html, htmlGeneralPanel, MainPanel.Colors.Lighter());
            return accessor;
        }

        private OxFrame CreateFrame(string Caption, bool caledSize = true)
        {
            OxFrame frame = new OxFrameWithHeader()
            {
                Text = Caption,
                Parent = MainPanel,
                Dock = OxDock.Top,
                BaseColor = MainPanel.BaseColor
            };
            frame.Margin.Size = OxWh.W8;
            frame.Margin.Top = OxWh.W0;
            frame.Padding.Size = OxWh.W8;

            if (caledSize)
                FramesControls.Add(frame, new List<Control>());

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
            panel.Margin[ExtraMarginsDock].Size = OxWh.W8;
            panel.Padding.Horizontal = OxWh.W2;
        }

        private void PrepareGroupByPanel()
        {
            groupByPanel.Sortings = settings.Text.Grouping;
            groupByPanel.Parent = textsPanel;
            groupByPanel.Dock = OxDock.Left;
            groupByPanel.Padding.Horizontal = OxWh.W2;
            groupByPanel.Size = new(OxWh.W200, groupByPanel.Height);
            groupByPanel.Visible = FunctionalPanelVisible.Fixed;
        }

        private void SetupControl(Control control, ExportFormat format, OxPanel? parent, Color baseColor, string caption = "") =>
            SetupControl(control, extraSettingsFrames[format], parent, baseColor, caption);

        private void SetupControl(Control control, OxFrame frame, OxPanel? parent, Color baseColor, string caption)
        {
            List<Control> framesControls = FramesControls[frame];
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
                    Font = Styles.DefaultFont
                };

                control.Left = OxWh.Int(label.Right) + 12;
                control.Tag = label;
                OxControlHelper.AlignByBaseLine(control, label);
            }

            ControlPainter.ColorizeControl(control, baseColor);
            framesControls.Add(control);
        }

        private int GetLastControlBottom(OxFrame frame)
        {
            List<Control> framesControls = FramesControls[frame];

            for (int i = framesControls.Count - 1; i > -1; i--)
                if (framesControls[i] is not OxPanel)
                    return framesControls[i].Bottom + 8;

            return 0;
        }

        private void SetupGeneralControl(Control control, Color baseColor, string caption = "") =>
            SetupControl(control, GeneralPanel, null, baseColor, caption);

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
            List<Control> frameControls = FramesControls[frame];

            frame.Size = new(
                frame.Width,
                frame.Equals(extraSettingsFrames[ExportFormat.Html])
                    ? htmlsPanel.Bottom
                    : frame.Equals(extraSettingsFrames[ExportFormat.Text])
                        ? textsPanel.Bottom
                        : frameControls.Count > 0
                            ? OxWh.W(frameControls[^1].Bottom)
                            : OxWh.W24
                | frame.Margin.Top
                | frame.Padding.Top
                | frame.Padding.Bottom
                | OxWh.W12
            );

            OxWidth maxLeft = OxWh.W0;

            foreach (Control control in frameControls)
                maxLeft = OxWh.Max(maxLeft, control.Left);

            foreach (Control control in frameControls)
            {
                if (control is OxCheckBox)
                    continue;

                control.Left = OxWh.Int(maxLeft);
                control.Width =
                    OxWh.Int
                    (
                        OxWh.Sub(
                            frame.Width,
                            maxLeft
                                | frame.Margin.Right
                                | frame.Margin.Left
                                | frame.Padding.Right
                                | OxWh.W12
                        )
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

            SetupControl(accessor.Control, format, parentPane, MainPanel.BaseColor, "Summary");
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
                    MainPanel.Size = new(OxWh.W720, frame.Bottom);
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
        private readonly OxPanel htmlGeneralPanel = CreateExtraPanel(OxWh.W216);
        private readonly OxPanel htmlsPanel = CreateExtraPanel(OxWh.W144);
        private readonly FieldsPanel<TField, TDAO> htmlFieldsPanel = new(FieldsVariant.Html);
        public readonly SortingPanel<TField, TDAO> htmlSortingPanel = new(SortingVariant.Export, ControlScope.Html);
        private readonly EnumAccessor<TField, TDAO, ExportSummaryType> htmlSummaryAccessor;
        private readonly IControlAccessor htmlIncludeParamsAccessor;
        private readonly IControlAccessor zeroSummaryAccessor;

        private readonly OxPanel textsPanel = CreateExtraPanel(OxWh.W144);
        private readonly OxPanel textGeneralPanel = CreateExtraPanel(OxWh.W84);
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
        private readonly Dictionary<OxFrame, List<Control>> FramesControls = new();
        private readonly IListController<TField, TDAO> ListController = DataManager.ListController<TField, TDAO>();
    }
}