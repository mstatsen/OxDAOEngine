using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Dialogs;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory;
using OxXMLEngine.ControlFactory.Accessors;
using OxXMLEngine.ControlFactory.Controls;
using OxXMLEngine.ControlFactory.Filter;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Data.Types;
using OxXMLEngine.Export;

namespace OxXMLEngine.Settings
{
    public partial class ExportSettingsForm<TField, TDAO> : OxDialog
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public override Bitmap FormIcon =>
            OxIcons.export;

        private static readonly ControlBuilder<TField, TDAO> Builder = DataManager.Builder<TField, TDAO>(ControlScope.Export);

        public ExportSettingsForm(ExportSettings<TField, TDAO> exportSettings) 
        {
            settings = exportSettings;
            InitializeComponent();
            Text = "Export";
            BaseColor = EngineStyles.SettingsFormColor;
            CreateExtraSettingsFrames();

            quickFilter.Parent = this;
            quickFilter.Margins.HorizontalOx = OxSize.Extra;

            PrepareHTMLControls();

            htmlIncludeParamsAccessor = CreateIncludeParamsControl(ExportFormat.Html);
            htmlSummaryAccessor = CreateSummaryControl(ExportFormat.Html);
            zeroSummaryAccessor = CreateZeroSummaryAccessor();

            indentAccessor = Builder.Accessor("XmlIndent", FieldType.Boolean);
            indentAccessor.Text = "Indent XML elements";
            indentAccessor.Value = settings.XML.Indent;
            SetupControl(indentAccessor.Control, ExportFormat.Xml, null, MainPanel.Colors.Lighter(1));

            PrepareTextControls();

            textIncludeParamsAccessor = CreateIncludeParamsControl(ExportFormat.Text);
            textSummaryAccessor = CreateSummaryControl(ExportFormat.Text);

            GeneralPanel = CreateFrame("General");

            formatAccessor = Builder.EnumAccessor<ExportFormat>();
            formatAccessor.Value = settings.Format;
            formatAccessor.ValueChangeHandler += FormatChangeHandler;
            SetupGeneralControl(formatAccessor.Control, MainPanel.BaseColor, "Export as");

            categoryControl = CreateButtonEdit(settings.CategoryName, SelectCategory);
            SetupGeneralControl(categoryControl, MainPanel.Colors.Lighter(1), "Category");

            fileControl = CreateButtonEdit(settings.FileName, ShowFileDialog);
            SetupGeneralControl(fileControl, MainPanel.Colors.Lighter(1), "File name");

            CalcFramesSizes();
            ActualizeFormatSettings();
            MainPanel.SetButtonText(OxDialogButton.OK, "Export");
            MainPanel.DialogButtonStartSpace = 8;
            MainPanel.DialogButtonSpace = 4;
        }

        private OxButtonEdit CreateButtonEdit(string? value, EventHandler? onClick)
        {
            OxButtonEdit buttonEdit = new()
            {
                Value = value,
                Font = EngineStyles.DefaultFont,
                BaseColor = MainPanel.Colors.Lighter()
            };
            buttonEdit.OnButtonClick += onClick;
            return buttonEdit;
        }

        private static OxPanel CreateExtraPanel(int height) =>
            new()
            {
                Dock = DockStyle.Top,
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
            accessor.Value = format == ExportFormat.Html
                    ? settings.HTML.IncludeExportParams
                    : settings.Text.IncludeExportParams;
            accessor.Text = $"Add export parameters info to {TypeHelper.Name(format).ToLower()}";

            OxPane parentPane = format == ExportFormat.Html 
                ? htmlGeneralPanel 
                : textGeneralPanel;

            SetupControl(accessor.Control, format, parentPane, MainPanel.Colors.Lighter(1));
            return accessor;
        }

        private void PrepareHTMLControls()
        {
            PrepareExtraPanel(htmlsPanel, ExportFormat.Html);
            PrepareExtraPanel(htmlGeneralPanel, ExportFormat.Html);
            PrepareFieldsPanel(htmlFieldsPanel, htmlsPanel, OxDock.Right);

            htmlSortingPanel.Sortings = settings.HTML.Sorting;
            htmlSortingPanel.Parent = htmlsPanel;
            htmlSortingPanel.Dock = DockStyle.Right;
            htmlSortingPanel.Paddings.HorizontalOx = OxSize.Medium;
            htmlSortingPanel.SetContentSize(168, htmlSortingPanel.SavedHeight);
        }

        private IControlAccessor CreateZeroSummaryAccessor()
        {

            IControlAccessor accessor = Builder.Accessor("ZeroSummary", FieldType.Boolean);
            accessor.Value = settings.HTML.ZeroSummary;
            accessor.Text = "Show summary with zero count";
            SetupControl(accessor.Control, ExportFormat.Html, htmlGeneralPanel, MainPanel.Colors.Lighter(1));
            return accessor;
        }

        private OxFrame CreateFrame(string Caption)
        {
            OxFrame frame = new OxFrameWithHeader()
            {
                Text = Caption,
                Parent = MainPanel,
                Dock = DockStyle.Top,
                BaseColor = MainPanel.BaseColor
            };
            frame.Margins.SetSize(OxSize.Extra);
            frame.Paddings.SetSize(OxSize.Extra);
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

        protected override string EmptyMandatoryField()
        {
            if (fileControl.IsEmpty)
                return "File name";

            return base.EmptyMandatoryField();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.Cancel)
                return;

            if (DialogResult == DialogResult.OK)
            {
                settings.Filter.CopyFrom(quickFilter.ActiveFilter);
                settings.CategoryName = categoryControl.Value ?? string.Empty;
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
        }

        private void ExportSettingsForm_Shown(object? sender, EventArgs e)
        {
            quickFilter.RenewFilterControls();
            quickFilter.ActiveFilter = settings.Filter;
            htmlFieldsPanel.Fields = settings.HTML.Fields.Count == 0
                ? TypeHelper.FieldHelper<TField>()
                    .Columns(FieldsVariant.Html, FieldsFilling.Default)
                : settings.HTML.Fields;
            inlineFieldsPanel.Fields = settings.Text.Fields;
            ActualizeFormatSettings();
            OxControlHelper.CenterForm(this);
        }

        private void ShowFileDialog(object? sender, EventArgs e)
        {
            ExportFormat currentFormat = formatAccessor.EnumValue;
            ExportFormatHelper helper = TypeHelper.Helper<ExportFormatHelper>();
            SaveFileDialog saveDialog = new()
            {
                FileName = fileControl.Value == string.Empty
                    ? DataManager.ListController<TField, TDAO>().Name + helper.FileExt(currentFormat)
                    : fileControl.Value,
                Filter = helper.FileFilter(currentFormat),
                OverwritePrompt = false
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
                fileControl.Value = saveDialog.FileName;
        }

        private static void PrepareFieldsPanel(FieldsPanel<TField, TDAO> panel, OxPane parent, OxDock ExtraMarginsDock)
        {
            panel.Parent = parent;
            panel.Dock = DockStyle.Fill;
            panel.Margins[ExtraMarginsDock].SetSize(OxSize.Extra);
            panel.Paddings.HorizontalOx = OxSize.Medium;
        }

        private void PrepareGroupByPanel()
        {
            groupByPanel.Sortings = settings.Text.Grouping;
            groupByPanel.Parent = textsPanel;
            groupByPanel.Dock = DockStyle.Left;
            groupByPanel.Paddings.HorizontalOx = OxSize.Medium;
            groupByPanel.SetContentSize(200, groupByPanel.SavedHeight);
        }

        private void SetupControl(Control control, ExportFormat format, OxPane? parent, Color baseColor, string caption = "") =>
            SetupControl(control, extraSettingsFrames[format], parent, baseColor, caption);

        private void SetupControl(Control control, OxFrame frame, OxPane? parent, Color baseColor, string caption)
        {
            List<Control> framesControls = FramesControls[frame];

            if (parent == null)
                parent = frame;

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
                    Font = EngineStyles.DefaultFont
                };

                control.Left = label.Right + 12;
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
            OxPanel? panel = null;

            if (frame == extraSettingsFrames[ExportFormat.Html])
                panel = htmlGeneralPanel;
            else
            if (frame == extraSettingsFrames[ExportFormat.Text])
                panel = textGeneralPanel;

            if (panel == null)
                return;

            panel.SetContentSize(
                panel.SavedWidth,
                GetLastControlBottom(frame)
            );
        }

        private void CalcFrameSize(OxFrame frame)
        {
            CalcExtraPanelSize(frame);
            List<Control> frameControls = FramesControls[frame];

            int lastControlBottom = 24;

            if (frame == extraSettingsFrames[ExportFormat.Html])
                lastControlBottom = htmlsPanel.Bottom;
            else
            if (frame == extraSettingsFrames[ExportFormat.Text])
                lastControlBottom = textsPanel.Bottom;
            else
            if (frameControls.Count > 0)
                lastControlBottom = frameControls[^1].Bottom;

            frame.SetContentSize(
                frame.SavedWidth,
                lastControlBottom
                + frame.Margins.Top
                + frame.Paddings.Top
                + frame.Paddings.Bottom
                + 12
            );

            int maxLeft = 0;

            foreach (Control control in frameControls)
                maxLeft = Math.Max(maxLeft, control.Left);

            foreach (Control control in frameControls)
            {
                if (control is OxCheckBox)
                    continue;

                control.Left = maxLeft;
                control.Width = frame.Width
                    - maxLeft
                    - frame.Margins.Right
                    - frame.Margins.Left
                    - frame.Paddings.Right
                    - 12;
            }
        }

        private void CalcFramesSizes()
        {
            foreach (OxFrame frame in FramesControls.Keys)
                CalcFrameSize(frame);
        }
           
        private void InitCategoriesTree()
        {
            if (categoriesTree != null)
                return;

            categoriesTree = new CategoriesTree<TField, TDAO>()
            {
                ShowCount = false
            };
            categoriesTree.SetContentSize(360, 480);
            categoriesTree.RefreshCategories(true);
        }

        private void SelectCategory(object? sender, EventArgs e)
        {
            InitCategoriesTree();
            categoriesTree!.ActiveCategory = 
                DataManager.ListController<TField, TDAO>().SystemCategories?
                    .Find(c => c.Name == categoryControl.Value);

            if (categoriesTree.ShowAsDialog(OxDialogButton.OK | OxDialogButton.Cancel) == DialogResult.OK)
                categoryControl.Value = categoriesTree.ActiveCategory?.Name;
        }

        private EnumAccessor<TField, TDAO, ExportSummaryType> CreateSummaryControl(ExportFormat format)
        {
            EnumAccessor<TField, TDAO, ExportSummaryType> accessor = Builder.EnumAccessor<ExportSummaryType>(format);
            
            accessor.Value = format == ExportFormat.Html 
                ? settings.HTML.Summary 
                : settings.Text.Summary;

            OxPane parentPane = format == ExportFormat.Html
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
                item.Value.Visible = item.Key == formatAccessor.EnumValue;

            foreach (OxFrame frame in extraSettingsFrames.Values)
                if (frame.Visible)
                {
                    MainPanel.SetContentSize(720, frame.Bottom);
                    break;
                }
        }

        private readonly ExportSettings<TField, TDAO> settings;
        private readonly QuickFilterPanel<TField, TDAO> quickFilter = new(QuickFilterVariant.Export)
        {
            Dock = DockStyle.Top
        };

        private readonly OxPanel htmlGeneralPanel = CreateExtraPanel(172);
        private readonly OxPanel htmlsPanel = CreateExtraPanel(60);
        private readonly FieldsPanel<TField, TDAO> htmlFieldsPanel = new(FieldsVariant.Html);
        private readonly SortingPanel<TField, TDAO> htmlSortingPanel = new(SortingVariant.Export, ControlScope.Html);
        private readonly EnumAccessor<TField, TDAO, ExportSummaryType> htmlSummaryAccessor;
        private readonly IControlAccessor htmlIncludeParamsAccessor;
        private readonly IControlAccessor zeroSummaryAccessor;

        private readonly OxPanel textsPanel = CreateExtraPanel(120);
        private readonly OxPanel textGeneralPanel = CreateExtraPanel(60);
        private readonly FieldsPanel<TField, TDAO> inlineFieldsPanel = new(FieldsVariant.Inline);
        private readonly SortingPanel<TField, TDAO> groupByPanel = new(SortingVariant.GroupBy, ControlScope.Grouping);
        private readonly EnumAccessor<TField, TDAO, ExportSummaryType> textSummaryAccessor;
        private readonly IControlAccessor textIncludeParamsAccessor;

        private readonly OxButtonEdit fileControl;
        private readonly OxButtonEdit categoryControl;
        private readonly EnumAccessor<TField, TDAO, ExportFormat> formatAccessor;

        private readonly IControlAccessor indentAccessor;

        private readonly OxFrame GeneralPanel;
        private readonly Dictionary<ExportFormat, OxFrame> extraSettingsFrames = new();
        private CategoriesTree<TField, TDAO>? categoriesTree;
        private readonly Dictionary<OxFrame, List<Control>> FramesControls = new();
    }
}