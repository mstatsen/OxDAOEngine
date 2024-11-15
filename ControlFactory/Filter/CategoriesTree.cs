using OxLibrary;
using OxLibrary.Controls;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Part;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Fields;

namespace OxDAOEngine.ControlFactory.Filter
{
    public class CategoriesTree<TField, TDAO> : FunctionsPanel<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public void RefreshCategories()
        {
            categorySelector.Loading = true;
            CategoriesReady = false;

            try
            {
                LoadCategories();
                FillTree();
            }
            finally
            {
                CategoriesReady = true;
                categorySelector.Loading = false;
            }
        }

        private bool CategoriesReady = false;

        public Category<TField, TDAO>? ActiveCategory
        {
            get =>
                categorySelector.SelectedNode == null
                || (categorySelector.SelectedItem is IEmptyChecked ec && ec.IsEmpty)
                    ? RootCategory
                    : (Category<TField, TDAO>?)categorySelector.SelectedItem;
            set =>
                SelectCategory(value);
        }

        public bool ShowCount { get; set; } = true;

        public ChangeCategoryHandler<TField, TDAO>? ActiveCategoryChanged;

        protected override Color FunctionColor => EngineStyles.CategoryColor;

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Text = "Categories";
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();
            categorySelector.BackColor = BackColor;
        }

        protected override void SetHandlers()
        {
            base.SetHandlers();
            ListController.ItemFieldChanged += FieldChangedHandler;
        }

        protected IListController<TField, TDAO> ListController =>
            DataManager.ListController<TField, TDAO>();

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            PrepareCategorySelector();
            ExpandButton.SetContentSize(28, 23);
            ExpandButton.Click += ExpandAllClick;
            CollapseButton.SetContentSize(28, 23);
            CollapseButton.Click += (s, e) => categorySelector.CollapseAll();
            Header.AddToolButton(CollapseButton);
            Header.AddToolButton(ExpandButton);
        }

        private void FieldChangedHandler(DAO dao, DAOEntityEventArgs? e) =>
            FillTree();

        private readonly OxTreeView categorySelector = new()
        {
            Left = 20,
            Top = 20,
            Width = 200,
            Height = 128,
            Dock = DockStyle.Fill,
            Font = Styles.DefaultFont
        };
        private readonly Category<TField, TDAO> RootCategory = new();

        private void ExpandAllClick(object? sender, EventArgs e)
        {
            categorySelector.ExpandAll();
            categorySelector.SelectedNode?.EnsureVisible();
        }

        private readonly OxIconButton CollapseButton = new(OxIcons.Up, 23)
        {
            Font = Styles.Font(-1, FontStyle.Bold),
            HiddenBorder = false
        };

        private readonly OxIconButton ExpandButton = new(OxIcons.Down, 23)
        {
            Font = Styles.Font(-1, FontStyle.Bold),
            HiddenBorder = false
        };

        protected override SettingsPart SettingsPart => 
            SettingsPart.Category; 

        private void LoadCategories()
        {
            RootCategory.Clear();
            RootCategory.Name = $"All {ListController.ListName}";

            foreach (Category<TField, TDAO> category in Settings.Categories)
                RootCategory.AddChild(category.GetCopy<Category<TField, TDAO>>());
        }

        private void PrepareCategorySelector()
        {
            categorySelector.Parent = ContentContainer;
            categorySelector.AfterSelect += AfterSelectHandler;
            categorySelector.BeforeCollapse += (s, e) => e.Cancel = e.Node == null || e.Node.Level == 0;
            categorySelector.DoubleClick += CategorySelectorDoubleClickHandler;
            categorySelector.Click += CategorySelectorClickHandler;
        }

        private void CategorySelectorClickHandler(object? sender, EventArgs e)
        {
            if (categorySelector.SelectedNode != null
                && categorySelector.SelectedNode.IsExpanded)
                categorySelector.SelectedNode.Collapse();
        }

        private void CategorySelectorDoubleClickHandler(object? sender, EventArgs e)
        {
            if (PanelViewer != null)
                PanelViewer.DialogResult = DialogResult.OK;
        }

        private Category<TField, TDAO>? LastCategory;

        public CategoriesTree() : base() =>
            SetContentSize(new(280, 1));

        private void FillTree()
        {
            StartLoading();
            Update();

            try
            {
                Category<TField, TDAO>? selectedNodeTag = (Category<TField, TDAO>?)categorySelector.SelectedItem;
                categorySelector.Nodes.Clear();
                AddCategoryToSelector(RootCategory, null);

                if (categorySelector.GetNodeCount(true) == 0)
                    return;

                if (Settings.AutoExpandCategories)
                    categorySelector.ExpandAll();
                else
                    categorySelector.Nodes[0].Expand();

                SelectCategory(selectedNodeTag);
            }
            finally 
            {
                //TODO:
                ExpandButton.Enabled = categorySelector.GetNodeCount(true) > 0;
                CollapseButton.Enabled = ExpandButton.Enabled;
                ResumeLayout();
                EndLoading();
            }
        }

        private void SelectCategory(Category<TField, TDAO>? category)
        {
            if (categorySelector.VisibleCount == 0)
                return;

            categorySelector.SelectedItem = category;
        }

        private RootListDAO<TField, TDAO> FullList => ListController.FullItemsList;

        private TreeNode? CreateCategoryTreeNode(Category<TField, TDAO> category)
        {
            int itemsCount = 
                ShowCount 
                    ? FullList.FilteredList(category).Count 
                    : 0;

            if (!category.IsRootCategory
                && ShowCount
                && Settings.HideEmptyCategory
                && itemsCount == 0)
                return null;

            return
                !ShowCount 
                || category.Type.Equals(CategoryType.FieldExtraction)
                || category.FilterIsEmpty
                ? new TreeNode(category.Name)
                    {
                        Tag = category
                    }
                : new CountedTreeNode(category.Name)
                    {
                        Tag = category,
                        Count = itemsCount
                    };
        }

        protected override DAOSetting VisibleSetting => DAOSetting.ShowCategories;

        protected override DAOSetting PinnedSetting => DAOSetting.CategoryPanelPinned;

        protected override DAOSetting ExpandedSetting => DAOSetting.CategoryPanelExpanded;

        protected override void ApplySettingsInternal()
        {
            base.ApplySettingsInternal();

            if (Observer.CategoriesChanged
                || Observer[DAOSetting.AutoExpandCategories]
                || Observer[DAOSetting.HideEmptyCategory]
                || Observer[DAOSetting.ShowCategories])
                RefreshCategories();
        }

        public override void SaveSettings()
        {
            base.SaveSettings();
            Settings.CategoryPanelPinned = Pinned;
            Settings.CategoryPanelExpanded = Expanded;
        }

        private readonly FieldHelper<TField> FieldHelper = TypeHelper.FieldHelper<TField>();

        private void AddCategoryToSelector(Category<TField, TDAO> category, TreeNode? parentNode)
        {
            TreeNode? node = CreateCategoryTreeNode(category);

            if (node == null)
                return;

            foreach (Category<TField, TDAO> childCategory in category.Childs)
            {
                if (!CategoriesReady 
                    && childCategory.Type.Equals(CategoryType.FieldExtraction))
                    CreateFieldExtractionCategories(childCategory);

                AddCategoryToSelector(childCategory, node);
            }

            if (!category.IsRootCategory
                && Settings.HideEmptyCategory
                && category.FilterIsEmpty
                && node.Nodes.Count == 0)
                return;

            if (parentNode == null)
                categorySelector.Nodes.Add(node);
            else parentNode.Nodes.Add(node);
        }

        private void CreateFieldExtractionCategories(Category<TField, TDAO> category)
        {
            TreeDAO<Category<TField, TDAO>> fieldChildsCategories =
                category.Childs.GetCopy<TreeDAO<Category<TField, TDAO>>>();

            category.Childs.Clear();

            List<object> extract = new FieldExtractor<TField, TDAO>()
            {
                Items = FullList
            }
            .Extract(category.Field, true, true);

            foreach (object value in extract)
            {
                string categoryName =
                    value is null
                        ? "<blank>"
                        : value is Enum
                            ? TypeHelper.Name(value)
                            : value.ToString()!;

                Filter<TField, TDAO> valueFilter = new(FilterConcat.AND);

                FilterOperation filterOperation = FieldHelper.DefaultFilterOperation(category.Field);

                if (value == null)
                    filterOperation = FilterOperation.Blank;

                valueFilter.AddFilter(category.Field, filterOperation, value);

                Category<TField, TDAO> valueCategory = new()
                {
                    Name = categoryName,
                    Type = CategoryType.Filter,
                    BaseOnChilds = false,
                    Filter = valueFilter
                };

                foreach (Category<TField, TDAO> fieldChildsCategory in fieldChildsCategories)
                    valueCategory.AddChild(fieldChildsCategory.GetCopy<Category<TField, TDAO>>());

                category.AddChild(
                    valueCategory
                );
            }
        }

        private void AfterSelectHandler(object? sender, TreeViewEventArgs e)
        {
            ActiveCategoryChanged?.Invoke(this, new CategoryEventArgs<TField, TDAO>(LastCategory, ActiveCategory));
            LastCategory = ActiveCategory;

            if (categorySelector.SelectedNode == null)
                return;

            if (categorySelector.SelectedNode.IsExpanded)
                categorySelector.SelectedNode.Collapse();
            else categorySelector.SelectedNode.Expand();
        }
    }
}