using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.ControlFactory.Controls
{
    public abstract partial class CustomItemEditor<TItem, TField, TDAO> : OxDialog
        where TItem : DAO, new()
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public IBuilderContext<TField, TDAO> Context { get; private set; } = default!;
        public ControlBuilder<TField, TDAO> Builder => Context.Builder;

        public CustomItemEditor() { }

        public TEditor Init<TEditor>(IBuilderContext<TField, TDAO> context)
            where TEditor : CustomItemEditor<TItem, TField, TDAO>
        {
            Context = context;
            InitializeComponent();
            SetPaddings();
            CreateControls();
            FirstLoad = true;
            return (TEditor)this;
        }

        protected bool FirstLoad { get; private set; }

        public TDAO? OwnerDAO { get; set; }

        public TItem? ParentItem { get; set; }

        public List<object>? ExistingItems { get; set; }

        public IMatcher<TField>? Filter { get; set; }

        public virtual void RenewData() { }

        protected virtual void SetPaddings() { }

        public IItemsContainerControl<TField,TDAO>? OwnerControl { get; internal set; }

        protected bool ReadOnly { get; set; }
        public DialogResult Edit(TItem item, bool readOnly = false)
        {
            ReadOnly = readOnly;
            PrepareReadonly();
            PrepareControlColors();
            FillControls(item);
            DialogResult result = ShowDialog(OwnerControl);

            if (result is DialogResult.OK)
                GrabControls(item);

            return result;
        }

        protected virtual void PrepareReadonly() { }

        protected virtual void PrepareControlColors() 
        {
            foreach (Control control in MainPanel.Controls)
                if (control is not OxLabel)
                    ControlPainter.ColorizeControl(
                        control,
                        MainPanel.BaseColor
                    );

            MainPanel.BackColor = MainPanel.Colors.Lighter(8);
        }

        protected override void OnShown(EventArgs e) => 
            RecalcSize();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RecalcSize();

            if (FirstLoad)
                FirstLoad = false;
        }

        protected virtual void RecalcSize() => 
            Size = new(
                OxWh.Int(ContentWidth),
                OxWh.Int(ContentHeight | (FirstLoad ? OxWh.W0 : OxWh.W6))
            );

        protected virtual void CreateControls() { }

        protected virtual OxWidth ContentWidth => OxWh.W400;
        protected virtual OxWidth ContentHeight => OxWh.W240;

        public TItem? Add()
        {
            TItem item = CreateNewItem();
            return 
                Edit(item) is DialogResult.OK 
                    ? item 
                    : null;
        }

        protected OxLabel CreateLabel(string caption, IControlAccessor accessor, 
            bool rightLabel = false) =>
            CreateLabel(caption, accessor.Control, rightLabel);

        private OxLabel CreateLabel(string caption, Control control, bool rightLabel = false) => 
            OxControlHelper.AlignByBaseLine(control,
                new OxLabel()
                {
                    Parent = this,
                    AutoSize = true,
                    Left = OxWh.Add(OxWh.W(rightLabel ? control.Right : 0), OxWh.W8),
                    Text = caption,
                    Font = OxStyles.DefaultFont
                }
            )!;

        protected virtual TItem CreateNewItem() => new();

        protected abstract void FillControls(TItem item);

        protected abstract void GrabControls(TItem item);
    }
}