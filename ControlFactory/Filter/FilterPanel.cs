using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Filter.Types;
using OxLibrary.Dialogs;


namespace OxDAOEngine.ControlFactory.Filter
{
    public class FilterPanel<TField, TDAO> : OxFrameWithHeader
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly ControlBuilder<TField, TDAO> Builder;
        private readonly OxPanel ConcatControlParent = new();
        private readonly IControlAccessor ConcatControl;
        private readonly List<FilterGroupPanel<TField, TDAO>> groupsPanels = new();
        private readonly OxPanel AddGroupButtonParent = new();
        private readonly OxButton AddGroupButton = new("Add group", OxIcons.Plus);

        private int GroupPanelsHeight()
        {
            int result = 0;

            foreach (FilterGroupPanel<TField, TDAO> panel in groupsPanels)
                result += panel.Height;

            return result;
        }

        public Filter<TField, TDAO> Filter
        {
            get => GrabFilter();
            set => FillFilter(value);
        }

        private void FillFilter(Filter<TField, TDAO> value)
        {
            Clear();

            if (value.Root.Count == 0)
                value.AddGroup(ConcatControl.EnumValue<FilterConcat>());

            foreach(FilterGroup<TField, TDAO> group in value.Root)
                AddGroup(group);
        }

        private Filter<TField, TDAO> GrabFilter()
        {
            Filter<TField, TDAO> result = new();

            foreach (FilterGroupPanel<TField, TDAO> groupPanel in groupsPanels)
                result.AddGroup(groupPanel.Group);

            return result;
        }

        private void AddGroup(FilterGroup<TField, TDAO> group)
        {
            SuspendLayout();

            try
            {
                FilterGroupPanel<TField, TDAO> groupPanel = new(this, group, Builder, groupsPanels.Count)
                {
                    Parent = ContentContainer,
                    Dock = DockStyle.Top
                };
                groupPanel.RuleAdded += GroupPanelRuleAddedHandler;
                groupPanel.RemoveGroup += GroupPanelRemoveGroupHandler;
                groupsPanels.Insert(0, groupPanel);
                PrepareColors();

                foreach (FilterGroupPanel<TField, TDAO> panel in groupsPanels)
                {
                    panel.Dock = DockStyle.None;
                    panel.Dock = DockStyle.Top;
                }

                ConcatControlParent.Dock = DockStyle.None;
                ConcatControlParent.Dock = DockStyle.Top;
            }
            finally
            {
                ResumeLayout();
                RecalcSize();
            }
        }

        public int GroupsCount => groupsPanels.Count;

        private void GroupPanelRemoveGroupHandler(object? sender, EventArgs e)
        {
            if (groupsPanels.Count == 1)
            {
                OxMessage.ShowError("You can't delete last group", this);
                return;
            }

            if (sender is not FilterGroupPanel<TField, TDAO> groupPanel)
                return;

            groupPanel.Parent = null;
            groupsPanels.Remove(groupPanel);
            RecalcSize();
            //TODO: dispose group panel
        }

        private void GroupPanelRuleAddedHandler(object? sender, EventArgs e)
        {
            RecalcSize();
            OnSizeChanged(EventArgs.Empty);
        }

        public FilterPanel(ControlBuilder<TField, TDAO> builder) : base()
        { 
            Builder = builder;
            ConcatControl = CreateConcatControl();
            PrepareAddGroupButton();
            Text = "Filter";
            Margins.SetSize(OxSize.Extra);
            RecalcSize();
        }

        protected override int GetCalcedHeight() =>
            Header.Height
            + Margins.Top
            + Margins.Bottom
            + Paddings.Top
            + Paddings.Bottom
            + ConcatControlParent.CalcedHeight
            + GroupPanelsHeight()
            + AddGroupButtonParent.CalcedHeight;


        private void PrepareAddGroupButton()
        {
            AddGroupButton.Parent = AddGroupButtonParent;
            AddGroupButton.Click += AddGroupButtonClickHandler;
            AddGroupButton.Height = 20;
            AddGroupButtonParent.Parent = this;
            AddGroupButtonParent.Dock = DockStyle.Bottom;
            AddGroupButtonParent.Paddings.SetSize(OxSize.Extra);
            AddGroupButtonParent.SetContentSize(1, AddGroupButton.Height);
        }

        private void AddGroupButtonClickHandler(object? sender, EventArgs e) => 
            AddGroup(new FilterGroup<TField, TDAO>());

        protected override void PrepareColors()
        {
            base.PrepareColors();

            foreach (FilterGroupPanel<TField, TDAO> groupPanel in groupsPanels)
                groupPanel.BaseColor = BaseColor;

            if (ConcatControlParent != null)
                ConcatControlParent.BaseColor = BaseColor;

            if (ConcatControl != null)
                ControlPainter.ColorizeControl(ConcatControl, BaseColor);

            if (AddGroupButtonParent != null)
                AddGroupButtonParent.BaseColor = BaseColor;

            AddGroupButton.BaseColor = BaseColor;
        }

        private IControlAccessor CreateConcatControl()
        {
            IControlAccessor result = Builder.Accessor("Filter:Concat", FieldType.Enum);
            result.Parent = ConcatControlParent;
            result.Left = -1;
            result.Top = -1;
            result.Width = 108;
            result.Height = 18;
            ConcatControlParent.Parent = this;
            ConcatControlParent.SetContentSize(1, result.Height);
            ConcatControlParent.Dock = DockStyle.Top;
            return result;
        }

        public void Clear()
        {
            foreach (FilterGroupPanel<TField, TDAO> groupPanel in groupsPanels)
            {
                groupPanel.Parent = null;
                //TODO: groupPanel.Dispose();
            }

            groupsPanels.Clear();
        }
    }
}