using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Grid;
using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;

namespace OxDAOEngine.Statistic
{
    public class StatisticPanel<TField, TDAO> : OxFrame
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public StatisticPanel(ItemsRootGrid<TField, TDAO> grid) : base(new(100, 24))
        {
            Grid = grid;
            Borders.Size = OxSize.None;
            Borders.Top = OxSize.XXS;
            DarkerColorHelper = new OxColorHelper(BaseColor);
            SetStatisticsTexts();
        }

        protected readonly ItemsRootGrid<TField, TDAO> Grid;

        protected readonly OxColorHelper DarkerColorHelper;

        protected readonly Dictionary<StatisticType, OxButton> Labels = new();

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (DarkerColorHelper is not null)
                DarkerColorHelper.BaseColor = Colors.Darker();

            foreach (StatisticType statisticType in Labels.Keys)
                PrepareStatisticColor(statisticType, 0);
        }

        protected virtual void SetStatisticText(StatisticType type, OxButton label)
        {
            switch (type)
            {
                case StatisticType.Total:
                    label.Text = $"Count : {Grid.RowCount}";
                    break;
            }
        }

        protected void SetStatisticsTexts()
        {
            foreach (var item in Labels)
            {
                int newStatistic = 0;
                SetStatisticText(item.Key, item.Value);
                PrepareStatisticColor(item.Key, newStatistic);
            }
        }

        protected virtual void PrepareStatisticColor(StatisticType type, int statistic) =>
            Labels[type].BaseColor = BaseColor;

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            CreateLabels();
        }

        private void CreateLabel(StatisticType type)
        {
            OxButton label = new(string.Empty, null)
            {
                Parent = this,
                Dock = StatisticTypeHelper.Dock(type),
                ReadOnly = true
            };
            Labels.Add(type, label);
            label.Size = new(StatisticTypeHelper.Width(type), 1);
        }

        protected readonly StatisticTypeHelper StatisticTypeHelper = TypeHelper.Helper<StatisticTypeHelper>();

        protected virtual List<StatisticType> AvailableStatistics =>
            new()
            { 
                StatisticType.Total
            };

        protected virtual void CreateLabels()
        {
            foreach (StatisticType type in StatisticTypeHelper.All())
                if (AvailableStatistics.Contains(type))
                CreateLabel(type);

            foreach (OxButton label in Labels.Values)
            {
                label.BringToFront();
                label.Margin.Size = OxSize.XS;
            }
        }

        public void Renew() =>
            SetStatisticsTexts();
    }
}