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
            BaseColor = Colors.Darker(2);

            tabControlPanel.Parent = this;
            tabControlPanel.Dock = DockStyle.Fill;
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
            ListController.ListChanged += (s, e) => ApplyQuickFilter(true);
            ListController.OnAfterLoad += (s, e) => quickFilter.RenewFilterControls();
            tabControl.ActivatePage += (s, e) => ApplyQuickFilter(true);
            statisticPanel.Renew();
            //tabControl.DeactivatePage += DeactivatePageHandler;
        }

        private StatisticPanel<TField, TDAO> CreateStatisticPanel() =>
            new(tableView.Grid, quickFilter)
            {
                Dock = DockStyle.Bottom,
                Parent = tabControlPanel
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
                Parent = tabControlPanel,
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

            itemsView.LoadingStarted += (s, e) => StartLoading(s == null ? this : ((ItemsView<TField, TDAO>)s).ContentContainer);
            itemsView.LoadingEnded += (s, e) => EndLoading();
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
            result.GridFillCompleted += (s, e) => ApplyQuickFilter();
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

        public virtual void ApplySettings(bool firstLoad)
        {
            /*
            if (ItemsFace<TField, TDAO>.Settings.Observer.SortingFieldsChanged)
            {
                sortingPanel.Sortings = ItemsFace<TField, TDAO>.Settings.Sortings;
                SortList();
            }
            */
            if (firstLoad)
            {
                categoriesTree.ActiveCategoryChanged -= ActiveCategoryChangedHandler;
                Settings.Observer.QuickFilterFieldsChanged = false;
                Settings.Observer.QuickFilterTextFieldsChanged = false;
                Settings.Observer.SortingFieldsChanged = false;
                Settings.Observer.TableFieldsChanged = false;
            }

            tableView.ApplySettings();
            //sortingPanel.ApplySettings();
            quickFilter.ApplySettings();
            categoriesTree.ApplySettings();

            if (firstLoad)
                categoriesTree.ActiveCategoryChanged += ActiveCategoryChangedHandler;

            if (Settings.Observer.QuickFilterFieldsChanged)
                quickFilter.RecalcPaddings();

            if (Settings.Observer[DAOSetting.ShowIcons])
            {
                tabControl.TabButtons[iconsView].Visible = Settings.ShowIcons;
                iconsView.Visible = Settings.ShowIcons;
            }

            iconsView.ApplySettings();

            if (Settings.Observer[DAOSetting.ShowCards])
            {
                tabControl.TabButtons[cardsView].Visible = Settings.ShowCards;
                cardsView.Visible = Settings.ShowCards;
            }

            cardsView.ApplySettings();

            if (!firstLoad && Settings.Observer.SummaryFieldsChanged)
                summaryView.RefreshData();
        }

        public virtual void SaveSettings()
        {
            tableView.SaveSettings();
            //sortingPanel.SaveSettings();
            quickFilter.SaveSettings();
            categoriesTree.SaveSettings();
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

        private void EndLoading() => loadingPanel.EndLoading();

        private void PrepareQuickFilter()
        {
            quickFilter.Parent = tabControlPanel;
            quickFilter.Dock = DockStyle.Top;
            quickFilter.Changed += (s, e) => ApplyQuickFilter();
            quickFilter.Margins.SetSize(OxSize.Large);
            quickFilter.Margins.BottomOx = OxSize.None;
            quickFilter.RenewFilterControls();
            quickFilter.PinnedChanged += (s, e) => categoriesTree.RecalcPinned();
            quickFilter.VisibleChanged += (s, e) => quickFilter.RecalcPaddings();
            quickFilter.RecalcPaddings();
            quickFilter.RecalcPinned();
        }

        private void PrepareCategoriesTree()
        {
            categoriesTree.Parent = this;
            categoriesTree.Dock = DockStyle.Left;
            categoriesTree.Margins.TopOx = OxSize.Large;
            categoriesTree.Margins.LeftOx = OxSize.Medium;
            categoriesTree.Margins.BottomOx = OxSize.Medium;
            categoriesTree.Margins.RightOx = OxSize.None;
            categoriesTree.Paddings.SetSize(OxSize.Medium);
            categoriesTree.Borders[OxDock.Right].Visible = false;
            categoriesTree.ActiveCategoryChanged += ActiveCategoryChangedHandler;
            categoriesTree.ActiveCategoryChanged += RenewFilterControls;
            categoriesTree.RecalcPinned();
        }


        /*
        private void SortChangedHandler(DAO dao, DAOEntityEventArgs e)
        {
            ItemsFace<TField, TDAO>.Settings.Sortings = sortingPanel.Sortings;
            SortList();
        }
        */
        /*

        private void DeactivatePageHandler(object sender, OxTabControlEventArgs e)
        {
            sortingPanel.Visible = e.Page != tableView;
        }
        */

        public void RenewFilterControls(object? sender, CategoryEventArgs<TField, TDAO> e)
        {
            if (e.IsFilterChanged)
                quickFilter.RenewFilterControls();
        }

        private void ChangeActiveCategory(bool needFillTableView)
        {
            StartLoading(tabControl);

            try
            {
                ListController.Category = categoriesTree?.ActiveCategory;

                if (needFillTableView)
                    tableView.FillGrid();
            }
            finally
            {
                EndLoading();
            }
        }

        private void ActiveCategoryChangedHandler(object? sender, CategoryEventArgs<TField, TDAO> e) => 
            ChangeActiveCategory(e.IsFilterChanged);

        public void FillData()
        {
            ChangeActiveCategory(true);
            summaryView.RefreshData();
            ApplyQuickFilter(true);
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (tableView != null)
                tableView.BaseColor = BaseColor;

            if (cardsView != null)
                cardsView.BaseColor = BaseColor;

            if (iconsView != null)
                iconsView.BaseColor = BaseColor;

            if (statisticPanel != null)
                statisticPanel.BaseColor = BaseColor;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
                quickFilter.RecalcPaddings();

            quickFilter.SiderEnabled = Visible;

            if (!quickFilter.Pinned)
                quickFilter.Expanded = false;

            categoriesTree.SiderEnabled = Visible;

            if (!categoriesTree.Pinned)
                categoriesTree.Expanded = false;

            if (tableView != null && tableView.CurrentInfoCard != null)
            {
                tableView.CurrentInfoCard.SiderEnabled = Visible;

                if (!tableView.CurrentInfoCard.Pinned)
                    tableView.CurrentInfoCard.Expanded = false;
            }
        }

        public SettingsPart ActiveSettingsPart => tabControl.ActivePage != tableView 
            ? SettingsPart.View
            : SettingsPart.Table;

        private readonly TableView<TField, TDAO> tableView;
        private readonly ItemsView<TField, TDAO> cardsView;
        private readonly ItemsView<TField, TDAO> iconsView;
        private readonly SummaryView<TField, TDAO> summaryView;
        private readonly QuickFilterPanel<TField, TDAO> quickFilter = new(QuickFilterVariant.Base);
        private readonly CategoriesTree<TField, TDAO> categoriesTree = new();
        private readonly OxLoadingPanel loadingPanel = new();
        //private readonly SortingPanel<TField, TDAO> sortingPanel = new(SortingVariant.Global, ControlScope.Table);
        private RootListDAO<TField, TDAO>? actualItemList;
        private readonly OxTabControl tabControl;
        private readonly OxPane tabControlPanel = new();
        private readonly StatisticPanel<TField, TDAO> statisticPanel;
    }
}