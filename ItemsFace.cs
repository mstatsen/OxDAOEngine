using OxLibrary;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Filter;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Grid;
using OxDAOEngine.Settings;
using OxDAOEngine.Statistic;
using OxDAOEngine.Summary;
using OxDAOEngine.View;
using OxDAOEngine.View.Types;
using OxDAOEngine.Settings.Part;

namespace OxDAOEngine
{
    public class ItemsFace<TField, TDAO> : OxPanel, IDataReceiver, IOxWithIcon
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public IListController<TField, TDAO> ListController =
            DataManager.ListController<TField, TDAO>();

        public ItemsFace()
        {
            VisibleChanged += OnVisibleChanged;
            DataReceivers.Register(this);
            PrepareFace();
            PrepareTabControlParent();
            tabControl = CreateTabControl();
            tableView = CreateTableView();
            cardsView = CreateView(ItemsViewsType.Cards);
            iconsView = CreateView(ItemsViewsType.Icons);
            summaryView = CreateSummaryView();
            PrepareQuickFilter();
            PrepareLoadingPanel();
            PrepareCategoriesTree();
            //sortingPanel.Visible = false;
            statisticPanel = CreateStatisticPanel();
            ActivateTableView();
            SetQuickFilterHandlers();
            statisticPanel.Renew();
            //tabControl.DeactivatePage += DeactivatePageHandler;
        }

        private void OnVisibleChanged(object? sender, EventArgs e) =>
            SetTabButtonsVisible();

        private void PrepareFace()
        {
            Text = ListController.ListName;
            Dock = OxDock.Fill;
            Font = OxStyles.DefaultFont;
            BaseColor = Colors.Darker(2);
        }

        private void SetQuickFilterHandlers()
        {
            if (ListController.AvailableQuickFilter)
                ListController.OnAfterLoad += (s, e) => quickFilter.RenewFilterControls();

            ListController.ListChanged += (s, e) => ApplyQuickFilter(true);
            tabControl.ActivatePage += (s, e) => ApplyQuickFilter(true);
            tabControl.ActivatePage += (s, e) => SaveCurrentView(e);
        }

        private void SaveCurrentView(OxTabControlEventArgs e)
        {
            foreach (KeyValuePair<ItemsViewsType, OxPanel> view in Views)
                if (view.Value.Equals(e.Page))
                {
                    Settings.CurrentView = view.Key;
                    return;
                };
        }

        private void PrepareTabControlParent()
        {
            tabControlPanel.Parent = this;
            tabControlPanel.Dock = OxDock.Fill;
        }

        private RootStatisticPanel<TField, TDAO> CreateStatisticPanel() =>
            new(tableView.Grid, quickFilter)
            {
                Dock = OxDock.Bottom,
                Parent = tabControlPanel
            };

        private void ActivateTableView() => 
            tabControl.ActivePage = Views[ItemsViewsType.Table];

        private void ActivateSavedView()
        {
            OxPanel? firstPage = tabControl.Pages.First;

            if (firstPage is not null)
                tabControl.TabButtons[firstPage].Margin.Left = OxWh.W2;

            tabControl.ActivePage = Views[Settings.CurrentView];
            tabControl.Update();
        }

        private OxTabControl CreateTabControl()
        {
            OxTabControl result = new()
            {
                Parent = tabControlPanel,
                Dock = OxDock.Fill,
                Font = OxStyles.DefaultFont,
                TabHeaderSize = new(OxWh.W84, OxWh.W24),
                TabPosition = OxDock.Bottom,
            };

            result.Margin.Size = OxWh.W0;
            result.Margin.Bottom = OxWh.W1;
            result.Borders.SetVisible(OxDock.Left, false);
            result.Borders.SetVisible(OxDock.Right, false);
            return result;
        }

        private ItemsView<TField, TDAO>? CreateView(ItemsViewsType viewType)
        {
            switch (viewType)
            { 
                case ItemsViewsType.Cards:
                    if (!ListController.AvailableCards)
                        return null;
                    break;
                case ItemsViewsType.Icons:
                    if (!ListController.AvailableIcons)
                        return null;
                    break;
            }    

            ItemsView<TField, TDAO> itemsView =
                new(viewType)
                {
                    Text = itemsViewsTypeHelper.Name(viewType),
                    BaseColor = BaseColor
                };

            itemsView.GetActualItemList += GetActualItemListHandler;
            itemsView.LoadingStarted += LoadingStartedHandler;
            itemsView.LoadingEnded += LoadingEndedHandler;
            tabControl.AddPage(itemsView);
            Views.Add(viewType, itemsView);
            return itemsView;
        }

        private RootListDAO<TField, TDAO>? GetActualItemListHandler() =>
            actualItemList;

        private void LoadingEndedHandler(object? sender, EventArgs e) =>
            EndLoading();

        private void LoadingStartedHandler(object? sender, EventArgs e) =>
            StartLoading(
                sender is null 
                    ? this 
                    : (ItemsView<TField, TDAO>)sender
                );

        private readonly Dictionary<ItemsViewsType, OxPanel> Views = new();

        private TableView<TField, TDAO> CreateTableView()
        {
            TableView<TField, TDAO> result = new()
            {
                Parent = tabControl,
                Dock = OxDock.Fill,
                Text = itemsViewsTypeHelper.Name(ItemsViewsType.Table),
                BaseColor = BaseColor,
                BatchUpdateCompleted = BatchUpdateCompletedHandler
            };
            result.Padding.Left = OxWh.W2;

            if (ListController.AvailableQuickFilter)
                result.GridFillCompleted += (s, e) => ApplyQuickFilter();

            tabControl.AddPage(result);
            Views.Add(ItemsViewsType.Table, result);
            return result;
        }

        private SummaryView<TField, TDAO>? CreateSummaryView()
        {
            if (!ListController.AvailableSummary)
                return null;

            SummaryView<TField, TDAO> result = new()
            {
                Dock = OxDock.Fill,
                Text = itemsViewsTypeHelper.Name(ItemsViewsType.Summary),
                BaseColor = new OxColorHelper(EngineStyles.SummaryColor).BaseColor
            };
            result.Padding.Left = OxWh.W2;
            tabControl.AddPage(result);
            Views.Add(ItemsViewsType.Summary, result);
            return result;
        }

        private readonly ItemsViewsTypeHelper itemsViewsTypeHelper = 
            TypeHelper.Helper<ItemsViewsTypeHelper>();

        private void BatchUpdateCompletedHandler(object? sender, EventArgs e)
        {
            SortList();
            categoriesTree.RefreshCategories();

            if (ListController.AvailableQuickFilter)
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

                if (ListController.AvailableQuickFilter)
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
                .FilteredList(
                    ListController.AvailableQuickFilter 
                        ? quickFilter?.ActiveFilter 
                        : null, 
                    Settings.Sortings.SortingsList
                );

            if (actualItemList is not null
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
                if (ListController.AvailableQuickFilter)
                    tableView.ApplyQuickFilter(quickFilter.ActiveFilter);

                if (ListController.AvailableCards &&
                    cardsView is not null
                    && cardsView.Equals(tabControl.ActivePage))
                    cardsView?.Fill();

                if (ListController.AvailableIcons
                    && iconsView is not null
                    && iconsView.Equals(tabControl.ActivePage))
                    iconsView?.Fill();
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

            if (ListController.AvailableQuickFilter)
            {
                quickFilter.ApplySettings();
                quickFilter.Visible = Settings.ShowQuickFilter;
            }

            if (ListController.AvailableCategories)
            {
                categoriesTree.ApplySettings();
                categoriesTree.Visible = Settings.ShowCategories;
            }

            if (firstLoad)
                categoriesTree.ActiveCategoryChanged += ActiveCategoryChangedHandler;

            if (ListController.AvailableQuickFilter &&
                Settings.Observer.QuickFilterFieldsChanged)
                quickFilter.RecalcPaddings();

            if (ListController.AvailableIcons)
            { 
                if (Settings.Observer[DAOSetting.ShowIcons])
                {
                    tabControl.TabButtons[iconsView!].Visible = Settings.ShowIcons;
                    iconsView!.Visible = Settings.ShowIcons;
                }

                iconsView?.ApplySettings();
            }

            if (ListController.AvailableCards)
            {
                if (Settings.Observer[DAOSetting.ShowCards])
                {
                    tabControl.TabButtons[cardsView!].Visible = Settings.ShowCards;
                    cardsView!.Visible = Settings.ShowCards;
                }

                cardsView!.ApplySettings();
            }

            SetTabButtonsVisible();
        }

        private void SetTabButtonsVisible() =>
            tabControl.HeaderVisible =
                (ListController.AvailableCards && Settings.ShowCards) ||
                (ListController.AvailableIcons && Settings.ShowIcons) ||
                ListController.AvailableSummary;

        public virtual void SaveSettings()
        {
            tableView.SaveSettings();
            //sortingPanel.SaveSettings();

            if (ListController.AvailableQuickFilter)
                quickFilter.SaveSettings();

            if (ListController.AvailableCategories)
                categoriesTree.SaveSettings();
            //ItemsFace<TField, TDAO>.Settings.Sortings = sortingPanel.Sortings;
        }

        private void PrepareLoadingPanel()
        {
            loadingPanel.Parent = tabControl;
            loadingPanel.Visible = false;
            loadingPanel.Margin.Top = OxWh.W4;
            loadingPanel.Borders.Size = OxWh.W1;
        }

        private void StartLoading(OxPanel? parentPanel = null)
        {
            loadingPanel.Parent = 
                parentPanel is null 
                    ? this 
                    : parentPanel;
            loadingPanel.StartLoading();
        }

        private void EndLoading() => loadingPanel.EndLoading();

        private void PrepareQuickFilter()
        {
            if (!ListController.AvailableQuickFilter)
                return;

            quickFilter.Parent = tabControlPanel;
            quickFilter.Dock = OxDock.Top;
            quickFilter.Changed += (s, e) => ApplyQuickFilter();
            quickFilter.Margin.Size = OxWh.W4;
            quickFilter.Margin.Bottom = OxWh.W0;
            quickFilter.RenewFilterControls();
            quickFilter.PinnedChanged += (s, e) => categoriesTree.RecalcPinned();
            quickFilter.VisibleChanged += (s, e) => quickFilter.RecalcPaddings();
            quickFilter.RecalcPaddings();
            quickFilter.RecalcPinned();
        }

        private void PrepareCategoriesTree()
        {
            if (!ListController.AvailableCategories)
                return;

            categoriesTree.Parent = this;
            categoriesTree.Dock = OxDock.Left;
            categoriesTree.Margin.Top = OxWh.W4;
            categoriesTree.Margin.Left = OxWh.W2;
            categoriesTree.Margin.Bottom = OxWh.W2;
            categoriesTree.Margin.Right = OxWh.W0;
            categoriesTree.Padding.Size = OxWh.W2;
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
            sortingPanel.Visible = !e.Page.Equals(tableView);
        }
        */

        public void RenewFilterControls(object? sender, CategoryEventArgs<TField, TDAO> e)
        {
            if (ListController.AvailableQuickFilter &&
                e.IsFilterChanged)
                quickFilter.RenewFilterControls();
        }

        private void ChangeActiveCategory(bool needFillTableView)
        {
            StartLoading(tabControl);

            try
            {
                ListController.Category = categoriesTree.ActiveCategory;

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

            if (ListController.AvailableSummary)
                summaryView!.RefreshData();

            ApplyQuickFilter(true);
            ActivateSavedView();
        }

        public override void PrepareColors()
        {
            base.PrepareColors();

            if (tableView is not null)
                tableView.BaseColor = BaseColor;

            if (cardsView is not null)
                cardsView.BaseColor = BaseColor;

            if (iconsView is not null)
                iconsView.BaseColor = BaseColor;

            if (statisticPanel is not null)
                statisticPanel.BaseColor = BaseColor;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (ListController.AvailableQuickFilter)
            {
                if (Visible)
                    quickFilter.RecalcPaddings();

                quickFilter.SiderEnabled = Visible;

                if (!quickFilter.Pinned)
                    quickFilter.Expanded = false;
            }

            if (ListController.AvailableCategories)
            {
                categoriesTree.SiderEnabled = Visible;

                if (!categoriesTree.Pinned)
                    categoriesTree.Expanded = false;
            }

            if (tableView is not null
                && tableView.InfoPanel is not null)
            {
                tableView.InfoPanel.SiderEnabled = Visible;

                if (Visible)
                    if (!tableView.InfoPanel.Pinned)
                        tableView.InfoPanel.Expanded = false;
            }
        }

        public SettingsPart ActiveSettingsPart => 
            tableView.Equals(tabControl.ActivePage)
                ? SettingsPart.Table
                : ListController.AvailableCards
                    && cardsView is not null
                    && cardsView.Equals(tabControl.ActivePage)
                        ? SettingsPart.Cards
                        : ListController.AvailableIcons
                            && iconsView is not null
                            && iconsView.Equals(tabControl.ActivePage)
                            ? SettingsPart.Icons
                            : SettingsPart.Table;

        protected override Bitmap? GetIcon()
        {
            return ListController.Icon;
        }

        private readonly TableView<TField, TDAO> tableView;
        private readonly ItemsView<TField, TDAO>? cardsView;
        private readonly ItemsView<TField, TDAO>? iconsView;
        private readonly SummaryView<TField, TDAO>? summaryView;
        private readonly QuickFilterPanel<TField, TDAO> quickFilter = new(QuickFilterVariant.Base);
        private readonly CategoriesTree<TField, TDAO> categoriesTree = new();
        private readonly OxLoadingPanel loadingPanel = new();
        //private readonly SortingPanel<TField, TDAO> sortingPanel = new(SortingVariant.Global, ControlScope.Table);
        private RootListDAO<TField, TDAO>? actualItemList;
        private readonly OxTabControl tabControl;
        private readonly OxPanel tabControlPanel = new();
        private readonly RootStatisticPanel<TField, TDAO> statisticPanel;
    }
}