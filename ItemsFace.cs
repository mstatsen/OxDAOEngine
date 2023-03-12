using OxLibrary;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory;
using OxXMLEngine.ControlFactory.Controls;
using OxXMLEngine.ControlFactory.Filter;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Data.Types;
using OxXMLEngine.Grid;
using OxXMLEngine.Settings;
using OxXMLEngine.Statistic;
using OxXMLEngine.Summary;
using OxXMLEngine.View;

namespace OxXMLEngine
{
    public class ItemsFace<TField, TDAO> : OxFrame, IDataReceiver
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

            functionsPlace = CreateFunctionsPlace();
            PrepareFunctionsPanels();
            PrepareLoadingPanel();
            PrepareCategoriesTree();
            sortingPanel.Visible = false;

            statisticPanel = CreateStatisticPanel();
            ListController.ListChanged += ListChangedHandler;
            ListController.OnAfterLoad += RenewFilterControls;
            tabControl.ActivatePage += ActivatePageHandler;
            tabControl.DeactivatePage += DeactivatePageHandler;
        }

        private StatisticPanel<TField, TDAO> CreateStatisticPanel() =>
            new(tableView.Grid, quickFilterPanel)
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
            quickFilterPanel.RenewFilterControls();
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
                .FilteredList(quickFilterPanel?.ActiveFilter, Settings.Sortings.SortingsList);

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
                tableView.ApplyQuickFilter(quickFilterPanel.ActiveFilter);

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
            if (ItemsFace<TField, TDAO>.Settings.Observer.SortingFieldsChanged)
            {
                sortingPanel.Sortings = ItemsFace<TField, TDAO>.Settings.Sortings;
                SortList();
            }

            if (ItemsFace<TField, TDAO>.Settings.Observer[DAOSetting.ShowCategories])
                categoriesPlace.Visible = ItemsFace<TField, TDAO>.Settings.ShowCategories;

            if (categoriesPlace.Expanded != ItemsFace<TField, TDAO>.Settings.CategoryPanelExpanded)
                categoriesPlace.Expanded = ItemsFace<TField, TDAO>.Settings.CategoryPanelExpanded;

            tableView.ApplySettings();
            sortingPanel.ApplySettings();
            quickFilterPanel.ApplySettings();
            categoriesTree.ApplySettings();

            if (ItemsFace<TField, TDAO>.Settings.Observer[DAOSetting.ShowIcons])
            {
                tabControl.TabButtons[iconsView].Visible = ItemsFace<TField, TDAO>.Settings.ShowIcons;
                iconsView.Visible = ItemsFace<TField, TDAO>.Settings.ShowIcons;
            }

            iconsView.ApplySettings();

            if (ItemsFace<TField, TDAO>.Settings.Observer[DAOSetting.ShowCards])
            {
                tabControl.TabButtons[cardsView].Visible = ItemsFace<TField, TDAO>.Settings.ShowCards;
                cardsView.Visible = ItemsFace<TField, TDAO>.Settings.ShowCards;
            }

            cardsView.ApplySettings();
        }

        public virtual void SaveSettings()
        {
            tableView.SaveSettings();
            sortingPanel.SaveSettings();
            quickFilterPanel.SaveSettings();
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

        private void PrepareFunctionsPanels()
        {
            quickFilterPanel.Margins.SetSize(OxSize.Large);
            quickFilterPanel.Margins.RightOx = OxSize.None;
            quickFilterPanel.Paddings.SetSize(OxSize.Large);
            quickFilterPanel.Changed += QuickFilterChangedHandler;
            quickFilterPanel.RenewFilterControls();
            quickFilterPanel.Parent = functionsPanel;
            quickFilterPanel.SizeChanged += FunctionsPanelResizeHandler;
            quickFilterPanel.VisibleChanged += FunctionsPanelResizeHandler;

            sortingPanel.Sortings = ItemsFace<TField, TDAO>.Settings.Sortings;
            sortingPanel.ExternalChangeHandler = SortChangedHandler;
            sortingPanel.Margins.SetSize(OxSize.Large);
            sortingPanel.Paddings.HorizontalOx = OxSize.Medium;
            sortingPanel.MaximumSize = sortingPanel.Size;
            sortingPanel.Parent = functionsPanel;
            sortingPanel.SizeChanged += FunctionsPanelResizeHandler;
            sortingPanel.VisibleChanged += FunctionsPanelResizeHandler;
            sortingPanel.Left = quickFilterPanel.Right;

            functionsPanel.Parent = functionsPlace;
            functionsPanel.BaseColor = new OxColorHelper(BaseColor).HBluer(2).HGreener(1).Lighter(1);
            functionsPanel.Paddings.VerticalOx = OxSize.None;

            functionsPlace.BaseColor = new OxColorHelper(functionsPanel.BaseColor)
                .Darker(Consts.SiderButtonDarkerMultiple);
        }

        private OxSidePanel CreateFunctionsPlace()
        {
            OxSidePanel result = new(new Size(1, 200))
            {
                Parent = tabControl,
                Dock = DockStyle.Top,
                Expanded = true,
                BorderVisible = true
            };
            result.Borders.SetSize(OxSize.Small);
            result.Borders.BottomOx = OxSize.None;
            result.Margins.LeftOx = OxSize.Large;
            result.Margins.RightOx = OxSize.Large;
            result.Margins.TopOx = OxSize.Large;
            result.SiderButtonBorders.HorizontalOx = OxSize.None;
            result.SiderButtonBorders.BottomOx = OxSize.None;
            result.OnExpandedChanged += FunctionsPlaceExpandedHandler;
            return result;
        }

        private void FunctionsPanelResizeHandler(object? sender, EventArgs e) =>
            RelayoutFunctionsPanels();

        private void RelayoutFunctionsPanels()
        {
            OxPaneList functionPanels = new()
            {
                quickFilterPanel,
                sortingPanel
            };
            sortingPanel.Left = quickFilterPanel.Visible ? quickFilterPanel.Right : 0;

            int calcedWidth = 0;

            foreach (OxPane pane in functionPanels)
                calcedWidth += pane.Width;

            functionsPanel.SetContentSize(
                calcedWidth,
                Math.Max(quickFilterPanel.Height, sortingPanel.Height)
            );
            functionsPlace.SetContentSize(functionsPanel.Width, functionsPanel.Height);

            int maxHeight = 0;

            foreach (OxPane pane in functionPanels)
                maxHeight = Math.Max(maxHeight, pane.Height);

            foreach (OxPane pane in functionPanels)
                pane.Height = maxHeight;

            Borders.TopOx = functionsPlace.Visible ? OxSize.Small : OxSize.None;
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

        private void SortChangedHandler(DAO dao, DAOEntityEventArgs e)
        {
            ItemsFace<TField, TDAO>.Settings.Sortings = sortingPanel.Sortings;
            SortList();
        }

        private void ActivatePageHandler(object sender, OxTabControlEventArgs e) =>
            ApplyQuickFilter(true);

        private void DeactivatePageHandler(object sender, OxTabControlEventArgs e)
        {
            sortingPanel.Visible = e.Page != tableView;
            functionsPlace.Visible = e.Page != summaryView;
        }

        private void ListChangedHandler(object? sender, EventArgs e) =>
            ApplyQuickFilter(true);

        public void RenewFilterControls(object? sender, CategoryEventArgs<TField, TDAO> e)
        {
            if (e.IsFilterChanged)
                quickFilterPanel.RenewFilterControls();
        }

        public void RenewFilterControls(object? sender, EventArgs e) =>
            quickFilterPanel.RenewFilterControls();

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

        private void FunctionsPlaceExpandedHandler(object? sender, EventArgs e) => 
            functionsPlace.SiderButtonBorders.TopOx = functionsPlace.Expanded ? OxSize.Small : OxSize.None;

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
            RelayoutFunctionsPanels();
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
        private readonly QuickFilterPanel<TField, TDAO> quickFilterPanel = new(QuickFilterVariant.Base);
        private readonly SortingPanel<TField, TDAO> sortingPanel = new(SortingVariant.Global, ControlScope.Table);
        private readonly OxPanel functionsPanel = new();
        private RootListDAO<TField, TDAO>? actualItemList;
        private readonly OxSidePanel categoriesPlace = new(new Size(280, 1));
        private readonly OxSidePanel functionsPlace;
        private readonly OxTabControl tabControl;
        private readonly StatisticPanel<TField, TDAO> statisticPanel;
    }
}