using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxLibrary.Controls;
using OxLibrary;
using OxLibrary.Panels;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxLibrary.Dialogs;
using OxDAOEngine.Data.Filter.Types;

namespace OxDAOEngine.ControlFactory.Filter
{
    public class FilterGroupPanel<TField, TDAO> : OxFrame
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly ControlBuilder<TField, TDAO> Builder;
        private readonly IControlAccessor ConcatControl;
        private readonly OxPanel ConcatControlParent = new();
        private readonly OxPanel AddRuleButtonParent = new();
        private readonly OxButton AddRuleButton = new("Add rule", OxIcons.Plus);
        private readonly OxIconButton RemoveGroupButton = new(OxIcons.Trash, 16);
        private readonly FilterPanel<TField, TDAO> ParentFilterPanel;
        private readonly int Number;
        private readonly List<RulePanel<TField, TDAO>> RulesPanels = new();

        public FilterGroup<TField, TDAO> Group
        {
            get => GrabGroup();
            set => FillGroup(value);
        }

        private void FillGroup(FilterGroup<TField, TDAO> value)
        {
            ConcatControl.Value = value.FilterConcat;

            if (value.Count is 0)
                value.Add(new FilterRule<TField>());

            foreach (FilterRule<TField> rule in value)
                AddRule(rule);
        }

        private FilterGroup<TField, TDAO> GrabGroup()
        {
            FilterGroup<TField, TDAO> result = new(ConcatControl.EnumValue<FilterConcat>());

            foreach (RulePanel<TField, TDAO> simpleFilterPanel in RulesPanels)
                result.Add(simpleFilterPanel.Rule);

            return result;
        }

        public FilterGroupPanel(
            FilterPanel<TField, TDAO> parentFilterPanel,
            FilterGroup<TField, TDAO> group, 
            ControlBuilder<TField, TDAO> builder, 
            int number) : base()
        {
            ParentFilterPanel = parentFilterPanel;
            Builder = builder;
            Number = number;
            ConcatControl = CreateConcatControl();
            Margins.TopOx = OxSize.S;
            Margins.RightOx = OxSize.M;
            Margins.BottomOx = OxSize.XS;
            Margins.Left = 24;
            Group = group;
            PrepareAddRuleButton();
            PrepareRemoveGroupButton();
            RecalcSize();
        }

        private void PrepareRemoveGroupButton()
        {
            RemoveGroupButton.Parent = ConcatControlParent;
            RemoveGroupButton.ToolTipText = "Remove group";
            RemoveGroupButton.Top = 1;
            RemoveGroupButton.Left = ConcatControlParent.Right - RemoveGroupButton.Width - 1;
            RemoveGroupButton.Anchor = AnchorStyles.Right;
            RemoveGroupButton.Click += RemoveGroupButtonClickHandler;
        }

        public EventHandler? RemoveGroup;

        private void RemoveGroupButtonClickHandler(object? sender, EventArgs e) =>
            RemoveGroup?.Invoke(this, EventArgs.Empty);

        private IControlAccessor CreateConcatControl()
        {
            IControlAccessor result = Builder.Accessor($"FilterGroup:Concat", FieldType.Enum, Number);
            result.Parent = ConcatControlParent;
            result.Left = -1;
            result.Top = -1;
            result.Width = 60;
            result.Height = 18;
            ConcatControlParent.Parent = this;
            ConcatControlParent.SetContentSize(1, result.Height);
            ConcatControlParent.Dock = DockStyle.Top;
            return result;
        }

        private void PrepareAddRuleButton()
        {
            AddRuleButton.Parent = AddRuleButtonParent;
            AddRuleButton.Height = 20;
            AddRuleButton.Click += AddRuleButtonClickHandler;
            AddRuleButtonParent.Parent = this;
            AddRuleButtonParent.Dock = DockStyle.Bottom;
            AddRuleButtonParent.Paddings.SetSize(OxSize.M);
            AddRuleButtonParent.SetContentSize(1, AddRuleButton.Height);
        }

        private void RulePanelRemoveRuleHandler(object? sender, EventArgs e)
        {
            if (RulesPanels.Count is 1)
            {
                if (ParentFilterPanel.GroupsCount is 1)
                {
                    OxMessage.ShowError("You can't delete last rule in last group", this);
                    return;
                }
                
                if (OxMessage.ShowConfirm(
                        "Are you shore you want to delete last rule in this group?", 
                        this
                    ) is not DialogResult.Yes)
                    return;
            }
                
                    
            if (sender is not RulePanel<TField, TDAO> rulePanel)
                return;

            rulePanel.Parent = null;
            RulesPanels.Remove(rulePanel);
            //TODO: dispose

            if (RulesPanels.Count is 0)
                RemoveGroup?.Invoke(this, EventArgs.Empty);

            RecalcSize();
        }

        private void AddRuleButtonClickHandler(object? sender, EventArgs e) => 
            AddRule(new FilterRule<TField>());

        private void AddRule(FilterRule<TField> rule)
        {
            SuspendLayout();

            try
            {
                RulePanel<TField, TDAO> simpleFilterPanel = new(rule, Builder, Number, RulesPanels.Count)
                {
                    Parent = ContentContainer,
                    Dock = DockStyle.Top
                };
                RulesPanels.Insert(0, simpleFilterPanel);
                PrepareColors();
                RecalcSize();
                simpleFilterPanel.RemoveRule += RulePanelRemoveRuleHandler;

                foreach (RulePanel<TField, TDAO> panel in RulesPanels)
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
            }

            RuleAdded?.Invoke(this, EventArgs.Empty);
        }

        public EventHandler? RuleAdded;

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (ConcatControlParent is not null)
                ConcatControlParent.BaseColor = BaseColor;

            if (ConcatControl is not null)
                ControlPainter.ColorizeControl(ConcatControl, BaseColor);

            if (AddRuleButtonParent is not null)
                AddRuleButtonParent.BaseColor = BaseColor;

            if (AddRuleButton is not null)
                AddRuleButton.BaseColor = BaseColor;

            if (RemoveGroupButton is not null)
                RemoveGroupButton.BaseColor = BaseColor;

            foreach (RulePanel<TField, TDAO> rulePanel in RulesPanels)
                rulePanel.BaseColor = BaseColor;
        }

        protected override int GetCalcedHeight() =>
            + Margins.Top
            + Margins.Bottom
            + Paddings.Top
            + Paddings.Bottom
            + ConcatControlParent.CalcedHeight
            + RulePanelsHeight()
            + AddRuleButtonParent.CalcedHeight;

        private int RulePanelsHeight()
        {
            int result = 0;

            foreach (RulePanel<TField, TDAO> panel in RulesPanels)
                result += panel.Height;

            return result;
        }
    }
}
