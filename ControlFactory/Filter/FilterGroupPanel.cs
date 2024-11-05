using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxLibrary.Controls;
using OxLibrary;
using OxLibrary.Panels;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;

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
        private readonly int Number;
        private readonly List<SimpleFilterPanel<TField, TDAO>> RulesPanels = new();
        public FilterGroup<TField, TDAO> Group 
        {
            get => GrabGroup();
            set => FillGroup(value);
        }

        private void FillGroup(FilterGroup<TField, TDAO> value)
        {
            if (value.Count == 0)
                value.Add(new SimpleFilter<TField, TDAO>());
            
            foreach (SimpleFilter<TField, TDAO> rule in value)
                AddRule(rule);
        }

        
        private FilterGroup<TField, TDAO> GrabGroup()
        {
            FilterGroup<TField, TDAO> result = new();

            foreach (SimpleFilterPanel<TField, TDAO> simpleFilterPanel in RulesPanels)
                result.Add(simpleFilterPanel.Rule);

            return result;
        }

        public FilterGroupPanel(FilterGroup<TField, TDAO> group, ControlBuilder<TField, TDAO> builder, int number) : base()
        {
            Builder = builder;
            Number = number;
            ConcatControl = CreateConcatControl();
            Margins.TopOx = OxSize.Large;
            Margins.RightOx = OxSize.Extra;
            Margins.BottomOx = OxSize.Medium;
            Margins.Left = 24;
            Group = group;
            PrepareAddRuleButton();
            RecalcSize();
        }

        private IControlAccessor CreateConcatControl()
        {
            IControlAccessor result = Builder.Accessor($"FilterGroup:Concat", FieldType.Enum, Number);
            result.Parent = ConcatControlParent;
            result.Left = 0;
            result.Top = 0;
            result.Width = 58;
            result.Height = 18;
            OxControlHelper.AlignByBaseLine(
                result.Control,
                new OxLabel()
                {
                    Parent = ConcatControlParent,
                    AutoSize = true,
                    Left = 64,
                    Text = "concatenation",
                    Font = new Font(Styles.DefaultFont, FontStyle.Italic)
                }
            );
            ConcatControlParent.Parent = this;
            ConcatControlParent.Paddings.LeftOx = OxSize.Extra;
            ConcatControlParent.Paddings.TopOx = OxSize.Extra;
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
            AddRuleButtonParent.Paddings.SetSize(OxSize.Extra);
            AddRuleButtonParent.SetContentSize(1, AddRuleButton.Height);
        }

        private void AddRuleButtonClickHandler(object? sender, EventArgs e) => 
            AddRule(new SimpleFilter<TField, TDAO>());

        private void AddRule(SimpleFilter<TField, TDAO> rule)
        {
            SuspendLayout();

            try
            {
                SimpleFilterPanel<TField, TDAO> simpleFilterPanel = new(rule, Builder, Number, RulesPanels.Count)
                {
                    Parent = ContentContainer,
                    Dock = DockStyle.Top
                };
                RulesPanels.Insert(0, simpleFilterPanel);
                PrepareColors();
                RecalcSize();

                foreach (SimpleFilterPanel<TField, TDAO> panel in RulesPanels)
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

            if (ConcatControlParent != null)
                ConcatControlParent.BaseColor = BaseColor;

            if (ConcatControl != null)
                ControlPainter.ColorizeControl(ConcatControl, BaseColor);

            if (AddRuleButtonParent != null)
                AddRuleButtonParent.BaseColor = BaseColor;

            if (AddRuleButton != null)
                ControlPainter.ColorizeControl(AddRuleButton, BaseColor);

            foreach (SimpleFilterPanel<TField, TDAO> rulePanel in RulesPanels)
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

            foreach (SimpleFilterPanel<TField, TDAO> panel in RulesPanels)
                result += panel.Height;

            return result;
        }
    }
}
