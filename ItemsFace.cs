using OxLibrary;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory.Filter;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Types;
using OxXMLEngine.Grid;
using OxXMLEngine.Settings;
using OxXMLEngine.Statistic;
using OxXMLEngine.Summary;
using OxXMLEngine.View;

namespace OxXMLEngine
{
    public class ItemsFace<TField, TDAO> : OxPane, IDataReceiver
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public IListController<TField, TDAO> ListController =
            DataManager.ListController<TField, TDAO>();

        public ItemsFace()
        {
            DataReceivers.Register(this);
            Text = ListController.Name;
            Dock = DockStyle.Fill;
            Font = EngineStyles.DefaultFont;

            tabControl = CreateTabControl();
            tableView = CreateTableView();
            cardsView = CreateView(ItemsViewsType.Cards);
            iconsView = CreateView(ItemsViewsType.Icons);
            summaryView = CreateSummaryView();
            ActivateFirstPage();

            PrepareQuickFilter();
            PrepareLoadingPanel();
            PrepareCategoriesTree();
            //sortingPanel.Visible = false;

            statisticPanel = CreateStatisticPanel();
            ListController.ListChanged += ListChangedHandler;
            ListController.OnAfterLoad += RenewFilterControls;
            tabControl.ActivatePage += ActivatePageHandler;
            tabControl.DeactivatePage += DeactivatePageHandler;
        }

        private StatisticPanel<TField, TDAO> CreateStatisticPanel() =>
            new(tableView.Grid, quickFilter)
            {
                Dock = DockStyle.Bottom,
                Parent = this
            };

        private void ActivateFirstPage()
        {
            IOxPane? firstPage = tabControl.Pages.First;

            if (firstPage != null)
                tabControl.TabButtons[firstPage].Margins.LeftOx = OxSize.Medium;

            tabControl.ActivePage = firstPage;
        }

        private OxTabControl CreateTabControl()
        {
            OxTabControl result = new()
            {
                Parent = this,
                Dock = DockStyle.Fill,
                Font = EngineStyles.DefaultFont,
                TabHeaderSize = new Size(84, 24),
                TabPosition = OxDock.Bottom,
            };

            result.Margins.SetSize(OxSize.None);
            result.Margins.BottomOx = OxSize.Small;
            result.Borders[OxDock.Left].Visible = false;
            result.Borders[OxDock.Right].Visible = false;
            return result;
        }

        private ItemsView<TField, TDAO> CreateView(ItemsViewsType viewType)
        {
            ItemsView<TField, TDAO> itemsView =
                new(viewType)
                {
                    Text = itemsViewsTypeHelper.Name(viewType),
                    BaseColor = BaseColor
                };

            itemsView.LoadingStarted += LoadingStartedHandler;
            itemsView.LoadingEnded += LoadingEndedHandler;
            tabControl.AddPage(itemsView);
            return itemsView;
        }

        private TableView<TField, TDAO> CreateTableView()
        {
            TableView<TField, TDAO> result = new()
            {
                Parent = tabControl,
                Dock = DockStyle.Fill,
                Text = itemsViewsTypeHelper.Name(ItemsViewsType.Table),
                BaseColor = BaseColor,
                BatchUpdateCompleted = BatchUpdateCompletedHandler
            };
            result.Paddings.LeftOx = OxSize.Medium;
            result.GridFillCompleted += TableFillCompleteHandler;
            tabControl.AddPage(result);
            return result;
        }

        private SummaryView<TField, TDAO> CreateSummaryView()
        {
            SummaryView<TField, TDAO> result = new()
            {
                Dock = DockStyle.Fill,
                Text = itemsViewsTypeHelper.Name(ItemsViewsType.Summary),
                BaseColor = BaseColor
            };
            result.Paddings.LeftOx = OxSize.Medium;
            tabControl.AddPage(result);
            return result;
        }

        private readonly ItemsViewsTypeHelper itemsViewsTypeHelper = 
            TypeHelper.Helper<ItemsViewsTypeHelper>();

        private void BatchUpdateCompletedHandler(object? sender, EventArgs e)
        {
            SortList();
            categoriesTree.RefreshCategories();
            quickFilter.RenewFilterControls();
            statisticPanel.Renew();
            tableView.Renew();
            tableView.SelectFirstItem();
        }

        private void SortList()
        {
            StartLoading(tabControl);

            try
            {
                ListController.Sort();
                ApplyQuickFilter(true);
            }
            finally
            {
                EndLoading();
            }
        }

        private bool QuickFilterChanged()
        {
            RootListDAO<TField, TDAO> newActualItemList = ListController.VisibleItemsList
                .FilteredList(quickFilter?.ActiveFilter, Settings.Sortings.SortingsList);

            if (actualItemList != null
                && actualItemList.Equals(newActualItemList))
                return false;

            actualItemList = newActualItemList;
            return true;
        }

        private void ApplyQuickFilter(bool Force = false)
        {
            if (!QuickFilterChanged() && !Force)
                return;

            StartLoading(tabControl);

            try
            {
                tableView.ApplyQuickFilter(quickFilter.ActiveFilter);

                if (tabControl.ActivePage == cardsView)
                    cardsView.Fill(actualItemList);

                if (tabControl.ActivePage == iconsView)
                    iconsView.Fill(actualItemList);
            }
            finally
            {
                EndLoading();
            }
        }

        private static DAOSettings<TField, TDAO> Settings =>
            SettingsManager.DAOSettings<TField, TDAO>();

        public virtual void ApplySettings()
        {
            /*
            if (ItemsFace<TField, TDAO>.Settings.Observer.SortingFieldsChanged)
            {
                sortingPanel.Sortings = ItemsFace<TField, TDAO>.Settings.Sortings;
                SortList();
            }
            */

            if (ItemsFace<TField, TDAO>.Settings.Observer[DAOSetting.ShowCategories])
                categoriesPlace.Visible = ItemsFace<TField, TDAO>.Settings.ShowCategories;

            if (categoriesPlace.Expanded != ItemsFace<TField, TDAO>.Settings.CategoryPanelExpanded)
                categoriesPlace.Expanded = ItemsFace<TField, TDAO>.Settings.CategoryPanelExpanded;

            tableView.ApplySettings();
            //sortingPanel.ApplySettings();
            quickFilter.ApplySettings();
            categoriesTree.ApplySettings();

            if (Settings.Observer.QuickFilterFieldsChanged)
                RecalcQuickFilterSize();

            if (Settings.Observer[DAOSetting.ShowIcons])
            {
                tabControl.TabButtons[iconsView].Visible = ItemsFace<TField, TDAO>.Settings.ShowIcons;
                iconsView.Visible = ItemsFace<TField, TDAO>.Settings.ShowIcons;
            }

            iconsView.ApplySettings();

            if (Settings.Observer[DAOSetting.ShowCards])
            {
                tabControl.TabButtons[cardsView].Visible = ItemsFace<TField, TDAO>.Settings.ShowCards;
                cardsView.Visible = ItemsFace<TField, TDAO>.Settings.ShowCards;
            }

            cardsView.ApplySettings();
        }

        public virtual void SaveSettings()
        {
            tableView.SaveSettings();
            //sortingPanel.SaveSettings();
            quickFilter.SaveSettings();
            categoriesTree.SaveSettings();
            ItemsFace<TField, TDAO>.Settings.CategoryPanelExpanded = categoriesPlace.Expanded;
            //ItemsFace<TField, TDAO>.Settings.Sortings = sortingPanel.Sortings;
        }

        private void PrepareLoadingPanel()
        {
            loadingPanel.Parent = tabControl;
            loadingPanel.Visible = false;
            loadingPanel.Margins.TopOx = OxSize.Large;
            loadingPanel.Borders.SetSize(OxSize.Small);
        }

        private void StartLoading(IOxPane? parentPanel = null)
        {
            loadingPanel.Parent = parentPanel == null ? this : (Control)parentPanel;
            loadingPanel.StartLoading();
        }

        private void LoadingStartedHandler(object? sender, EventArgs e) =>
            StartLoading(sender == null ? this : ((ItemsView<TField, TDAO>)sender).ContentContainer);

        private void LoadingEndedHandler(object? sender, EventArgs e) =>
            EndLoading();

        private void EndLoading() => loadingPanel.EndLoading();

        private void PrepareQuickFilter()
        {
            quickFilterPlace.Parent = tabControl;
            quickFilterPlace.Dock = DockStyle.Top;
            quickFilterPlace.Expanded = true;
            quickFilterPlace.Margins.SetSize(OxSize.Large);
 
            quickFilter.Dock = DockStyle.Fill;
            quickFilter.Changed += QuickFilterChangedHandler;
            quickFilter.RenewFilterControls();
            quickFilter.Parent = quickFilterPlace;
            quickFilter.VisibleChanged += QuickFilterVisibleChangedHandler;
            quickFilter.Borders[OxDock.Bottom].Visible = false;
            quickFilter.Header.Colors.BaseColorChanged += QuickFilterBaseColorChangerHandler;

            RecalcQuickFilterSize();
        }

        private void QuickFilterBaseColorChangerHandler(object? sender, EventArgs e) =>
            quickFilterPlace.BaseColor = quickFilter.Header.BaseColor;

        private void QuickFilterVisibleChangedHandler(object? sender, EventArgs e)
        {
            RecalcQuickFilterSize();
        }

        private void RecalcQuickFilterSize()
        {
            quickFilter.Paddings.SetSize(quickFilter.OnlyText ? OxSize.None : OxSize.Large);
            quickFilterPlace.SetContentSize(1, quickFilter.CalcedHeight);
        }

        private void PrepareCategoriesTree()
        {
            categoriesPlace.Dock = DockStyle.Left;
            categoriesPlace.Expanded = true;
            categoriesPlace.Parent = this;
            categoriesPlace.Margins.TopOx = OxSize.Large;
            categoriesPlace.Margins.LeftOx = OxSize.Medium;
            categoriesPlace.Margins.BottomOx = OxSize.Small;
            categoriesPlace.Margins.RightOx = OxSize.None;
            categoriesPlace.OnExpandedChanged += CategoriesPlaceExpandedHandler;
            categoriesPlace.OnAfterCollapse += CategoriesPlaceAfterCollapseHandler;

            categoriesTree.Parent = categoriesPlace;
            categoriesTree.Dock = DockStyle.Fill;
            categoriesTree.Paddings.SetSize(OxSize.Medium);
            categoriesTree.Borders[OxDock.Right].Visible = false;
            categoriesTree.ActiveCategoryChanged += ActiveCategoryChangedHandler;
            categoriesTree.ActiveCategoryChanged += RenewFilterControls;
            categoriesTree.Header.Colors.BaseColorChanged += CategoriesBaseColorChangeHandler;
        }

        private void CategoriesBaseColorChangeHandler(object? sender, EventArgs e) => 
            categoriesPlace.BaseColor = categoriesTree.Header.BaseColor;

        private void QuickFilterChangedHandler(object? sender, EventArgs e) =>
            ApplyQuickFilter();

        private void TableFillCompleteHandler(object? sender, EventArgs e) =>
            ApplyQuickFilter();

        /*
        private void SortChangedHandler(DAO dao, DAOEntityEventArgs e)
        {
            ItemsFace<TField, TDAO>.Settings.Sortings = sortingPanel.Sortings;
            SortList();
        }
        */

        private void ActivatePageHandler(object sender, OxTabControlEventArgs e) =>
            ApplyQuickFilter(true);

        private void DeactivatePageHandler(object sender, OxTabControlEventArgs e)
        {
            //sortingPanel.Visible = e.Page != tableView;
            quickFilterPlace.Visible = e.Page != summaryView;
        }

        private void ListChangedHandler(object? sender, EventArgs e) =>
            ApplyQuickFilter(true);

        public void RenewFilterControls(object? sender, CategoryEventArgs<TField, TDAO> e)
        {
            if (e.IsFilterChanged)
                quickFilter.RenewFilterControls();
        }

        public void RenewFilterControls(object? sender, EventArgs e) =>
            quickFilter.RenewFilterControls();

        private void ActiveCategoryChangedHandler(object? sender, CategoryEventArgs<TField, TDAO> e)
        {
            StartLoading(tabControl);

            try
            {
                ListController.Category = categoriesTree?.ActiveCategory;

                if (e.IsFilterChanged)
                    tableView.FillGrid();
            }
            finally
            {
                EndLoading();
            }
        }

        private void CategoriesPlaceAfterCollapseHandler(object? sender, EventArgs e) =>
            categoriesTree.Enabled = false;

        private void CategoriesPlaceExpandedHandler(object? sender, EventArgs e) =>
            categoriesTree.Enabled = true;

        public void FillData()
        {
            tableView.FillGrid();
            summaryView.RefreshData(true);
            ApplyQuickFilter(true);
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (statisticPanel != null)
                statisticPanel.BaseColor = BaseColor;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            RecalcQuickFilterSize();
        }

        public SettingsPart ActiveSettingsPart => tabControl.ActivePage != tableView 
            ? SettingsPart.View
            : SettingsPart.Table;

        private readonly TableView<TField, TDAO> tableView;
        private readonly ItemsView<TField, TDAO> cardsView;
        private readonly ItemsView<TField, TDAO> iconsView;
        private readonly SummaryView<TField, TDAO> summaryView;
        private readonly CategoriesTree<TField, TDAO> categoriesTree = new();
        private readonly OxLoadingPanel loadingPanel = new();
        private readonly QuickFilterPanel<TField, TDAO> quickFilter = new(QuickFilterVariant.Base);
        //private readonly SortingPanel<TField, TDAO> sortingPanel = new(SortingVariant.Global, ControlScope.Table);
        private RootListDAO<TField, TDAO>? actualItemList;
        private readonly OxSidePanel categoriesPlace = new(new Size(280, 1));
        private readonly OxSidePanel quickFilterPlace = new(new Size(1, 150));
        private readonly OxTabControl tabControl;
        private readonly StatisticPanel<TField, TDAO> statisticPanel;
    }
}