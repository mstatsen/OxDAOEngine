using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data.Fields;

namespace OxDAOEngine.Editor
{
    public partial class UniqueKeyViewer<TField, TDAO> : OxDialog
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public UniqueKeyViewer() 
        {
            InitializeComponent();
            CreateControls(DataManager.Builder<TField, TDAO>(ControlScope.Editor));
            DialogButtons = OxDialogButton.OK;
        }

        public void View(TDAO? item, Control ownerControl)
        {
            PrepareControlColors();
            FillControls(item);
            ShowDialog(ownerControl);
        }

        private void FillControls(TDAO? item)
        {
            FieldHelper<TField> fieldHelper = DataManager.FieldHelper<TField>();
            Text = $"{(item is not null ? $"{item[fieldHelper.TitleField]} " : string.Empty)}{fieldHelper.Name(fieldHelper.UniqueField)}";
            uniqueKeyAccessor.Value = 
                item is null 
                    ? "Empty" 
                    : item[fieldHelper.UniqueField];
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (uniqueKeyAccessor.ReadOnlyControl is OxTextBox textBox)
                textBox.SelectionLength = 0;
        }

        private void PrepareControlColors() 
        {
            foreach (Control control in MainPanel.Controls)
                if (control is not OxLabel)
                    ControlPainter.ColorizeControl(
                        control,
                        MainPanel.BaseColor
                    );

            MainPanel.BackColor = MainPanel.Colors.Lighter(8);
        }

        private bool firstLoad = true;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int height = ContentHeight + (firstLoad ? 0 : 6);
            Size = new(ContentWidth, height);

            if (firstLoad)
                firstLoad = false;
        }

        private void CreateControls(ControlBuilder<TField, TDAO> builder) 
        {
            uniqueKeyAccessor = builder.Accessor("UniqueKey", FieldType.Guid);
            uniqueKeyAccessor.Parent = MainPanel;
            uniqueKeyAccessor.Left = 12;
            uniqueKeyAccessor.Top = 12;
            uniqueKeyAccessor.Width = 340;
            uniqueKeyAccessor.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            uniqueKeyAccessor.ReadOnly = true;
            ((OxTextBox)uniqueKeyAccessor.ReadOnlyControl!).Multiline = false;

            copyButton.Parent = MainPanel;
            copyButton.Top = OxWh.W8;
            copyButton.Size = new(OxWh.W64, OxWh.W24);
            copyButton.Left = OxWh.W336;
            copyButton.Click += CopyHandler;
        }

        private void CopyHandler(object? sender, EventArgs e)
        {
            if (uniqueKeyAccessor.Value is not null)
                Clipboard.SetText(uniqueKeyAccessor.Value.ToString());
        }

        private readonly int ContentWidth = 410;
        private readonly int ContentHeight = 44;

        private IControlAccessor uniqueKeyAccessor = default!;
        private readonly OxButton copyButton = new("Copy", OxIcons.Copy);
    }
}