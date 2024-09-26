using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Dialogs;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.ControlFactory.Controls
{
    public abstract partial class ListItemEditor<TItem, TField, TDAO> : OxDialog
        where TItem : DAO, new()
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public IBuilderContext<TField, TDAO> Context { get; private set; } = default!;

        public ListItemEditor() { }

        public TEditor Init<TEditor>(IBuilderContext<TField, TDAO> context)
            where TEditor : ListItemEditor<TItem, TField, TDAO>
        {
            Context = context;
            InitializeComponent();
            SetPaddings();
            CreateControls();
            firstLoad = true;
            return (TEditor)this;
        }

        private bool firstLoad;

        public TDAO? ParentItem { get; set; }

        public List<object>? ExistingItems { get; set; }

        public IMatcher<TField>? Filter { get; set; }

        public virtual void RenewData() { }

        protected virtual void SetPaddings() { }

        public Control? OwnerControl { get; internal set; }

        public DialogResult Edit(TItem item)
        {
            PrepareControlColors();
            FillControls(item);
            DialogResult result = ShowDialog(OwnerControl);

            if (result == DialogResult.OK)
                GrabControls(item);

            return result;
        }

        protected virtual void PrepareControlColors() 
        {
            foreach (Control control in MainPanel.ContentContainer.Controls)
                if (control is not OxLabel)
                    ControlPainter.ColorizeControl(
                        control,
                        MainPanel.BaseColor
                    );

            MainPanel.ContentContainer.BackColor = MainPanel.Colors.Lighter(8);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int height = ContentHeight + (firstLoad ? 0 : 6);
            SetContentSize(ContentWidth, height);

            if (firstLoad)
                firstLoad = false;
        }

        protected virtual void CreateControls() { }

        protected virtual int ContentWidth => 400;
        protected virtual int ContentHeight => 240;

        public TItem? Add()
        {
            TItem item = CreateNewItem();
            return Edit(item) == DialogResult.OK ? item : null;
        }

        protected OxLabel CreateLabel(string caption, IControlAccessor accessor, 
            bool rightLabel = false) =>
            CreateLabel(caption, accessor.Control, rightLabel);

        private OxLabel CreateLabel(string caption, Control control, bool rightLabel = false) => 
            (OxLabel)OxControlHelper.AlignByBaseLine(control,
                new OxLabel()
                {
                    Parent = this,
                    AutoSize = true,
                    Left = (rightLabel ? control.Right : 0) + 8,
                    Text = caption,
                    Font = new Font(EngineStyles.DefaultFont, FontStyle.Regular)
                }
            )!;

        protected virtual TItem CreateNewItem() => new();

        protected abstract void FillControls(TItem item);

        protected abstract void GrabControls(TItem item);
    }
}