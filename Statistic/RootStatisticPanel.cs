using OxLibrary;
using OxDAOEngine.ControlFactory.Filter;
using OxDAOEngine.Data;
using OxDAOEngine.Grid;
using OxLibrary.Controls;

namespace OxDAOEngine.Statistic
{
    public class RootStatisticPanel<TField, TDAO> : StatisticPanel<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public RootStatisticPanel(
            ItemsRootGrid<TField, TDAO> grid,
            QuickFilterPanel<TField, TDAO>? quickFilterPanel) : base(grid)
        {
            QuickFilterPanel = quickFilterPanel;
            AccessHandlers();
            SetStatisticsTexts();
        }

        private readonly IListController<TField, TDAO> ListController = 
            DataManager.ListController<TField, TDAO>();

        private int Statistic(StatisticType type) => 
            type switch
            {
                StatisticType.Visible => 
                    ListController.AvailableQuickFilter 
                    && QuickFilterPanel is not null
                        ? ListController.VisibleItemsList.FilteredList(QuickFilterPanel.ActiveFilter).Count 
                        : ListController.TotalCount,
                StatisticType.Selected => Grid.SelectedCount,
                StatisticType.Modified => ListController.ModifiedCount,
                StatisticType.Added => ListController.AddedCount,
                StatisticType.Deleted => ListController.RemovedCount,
                _ => 0,
            };

        protected override void SetStatisticText(StatisticType type, OxButton label)
        {
            switch (type)
            {
                case StatisticType.Total:
                    label.Text = $"Total {ListController.ListName} : {ListController.TotalCount}";
                    break;
                case StatisticType.Category:
                    RenewCategoryValue();
                    break;
                default:
                    int newStatistic = Statistic(type);
                    label.Text = $"{StatisticTypeHelper.Name(type)}: {newStatistic}";
                    break;
            }

        }

        private void RenewCategoryValue() => 
            Labels[StatisticType.Category].Text =
                ListController.Category is not null 
                && ListController.Category.Name is not null 
                && !ListController.Category.Name.Equals(string.Empty)
                    ? $"{ListController.Category?.Name} : {ListController.FilteredCount}"
                    : string.Empty;

        private readonly OxColorHelper CategoryColorHelper = new(EngineStyles.CategoryColor);
        private readonly OxColorHelper QuickFilterColorHelper = new(EngineStyles.QuickFilterColor);

        protected override void PrepareStatisticColor(StatisticType type, int statistic) => 
            Labels[type].BaseColor = 
                type switch
                {
                    StatisticType.Category => 
                        CategoryColorHelper.Darker(
                            ListController.Category is null 
                            || ListController.Category.FilterIsEmpty
                                ? 0
                                : 2
                        ),
                    StatisticType.Visible =>
                        QuickFilterColorHelper.Darker(
                            !ListController.AvailableQuickFilter 
                            || QuickFilterPanel is null 
                            || QuickFilterPanel.ActiveFilter is null 
                            || QuickFilterPanel.ActiveFilter.FilterIsEmpty
                                ? 0
                                : 2
                        ),
                    StatisticType.Modified or
                    StatisticType.Added or
                    StatisticType.Deleted when statistic > 0 =>
                        DarkerColorHelper.Redder(6),
                    _ => BaseColor
                };

        protected override List<StatisticType> AvailableStatistics =>
            new()
            {
                StatisticType.Total,
                StatisticType.Category,
                StatisticType.Visible,
                StatisticType.Selected,
                StatisticType.Modified,
                StatisticType.Added,
                StatisticType.Deleted
            };

        private void AccessHandlers()
        {
            ListController.CategoryChanged += (s, e) => RenewCategoryValue();
            ListController.ListChanged += (s, e) => SetStatisticsTexts();
            ListController.ModifiedHandler += (d, m) => SetStatisticsTexts();
            ListController.ItemFieldChanged += (d, e) => SetStatisticsTexts();

            if (QuickFilterPanel is not null)
                QuickFilterPanel.Changed += (s, e) => SetStatisticsTexts();

            Grid.CurrentItemChanged += (s, e) => SetStatisticsTexts();
        }

        private readonly QuickFilterPanel<TField, TDAO>? QuickFilterPanel;

        protected override void CreateLabels()
        {
            base.CreateLabels();
            Labels[StatisticType.Category].Visible = ListController.AvailableCategories;
            Labels[StatisticType.Visible].Visible = ListController.AvailableQuickFilter;
            Labels[StatisticType.Modified].ReadOnly = false;
            Labels[StatisticType.Modified].Click += ViewHistoryHandler;
            Labels[StatisticType.Added].ReadOnly = false;
            Labels[StatisticType.Added].Click += ViewHistoryHandler;
            Labels[StatisticType.Deleted].ReadOnly = false;
            Labels[StatisticType.Deleted].Click += ViewHistoryHandler;
        }

        private void ViewHistoryHandler(object? sender, EventArgs e) =>
            ListController.ViewHistory();
    }
}