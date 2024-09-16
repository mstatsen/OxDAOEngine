using OxLibrary;
using OxLibrary.Controls;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Extract;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Types;
using OxXMLEngine.Settings;

namespace OxXMLEngine.ControlFactory.Filter
{
    public class CategoriesTree<TField, TDAO> : FunctionsPanel<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public void RefreshCategories(bool systemOnly = false)
        {
            categorySelector.Loading = true;

            try
            {
                LoadCategories(systemOnly);
                FillTree();
            }
            finally
            {
                categorySelector.Loading = false;
            }
        }

        public Category<TField, TDAO>? ActiveCategory
        {
            get =>
                categorySelector.SelectedNode == null
                || categorySelector.SelectedNode.Tag is NullObject
                    ? RootCategory
                    : (Category<TField, TDAO>)categorySelector.SelectedNode.Tag;
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
            Font = EngineStyles.DefaultFont
        };
        private readonly Category<TField, TDAO> RootCategory = new();

        private static Category<TField, TDAO> CreateCategory(string name) => new(name);

        private void ExpandAllClick(object? sender, EventArgs e)
        {
            categorySelector.ExpandAll();
            categorySelector.SelectedNode?.EnsureVisible();
        }

        private readonly OxIconButton CollapseButton = new(OxIcons.up, 23)
        {
            Font = new Font(Styles.FontFamily, Styles.DefaultFontSize - 1, FontStyle.Bold),
            HiddenBorder = false
        };

        private readonly OxIconButton ExpandButton = new(OxIcons.down, 23)
        {
            Font = new Font(Styles.FontFamily, Styles.DefaultFontSize - 1, FontStyle.Bold),
            HiddenBorder = false
        };

        protected override SettingsPart SettingsPart => 
            SettingsPart.Category; 

        private Category<TField, TDAO>? FieldCategory(TField field)
        {
            try
            {
                string? categoryName = TypeHelper.Name(field);

                if (categoryName == null)
                    return null;

                Category<TField, TDAO> fieldCategory = CreateCategory(categoryName);
                List<object> extract = new FieldExtractor<TField, TDAO>()
                {
                    Items = FullList
                }
                .Extract(field, true, true);

                foreach (object value in extract)
                {
                    categoryName = value is Enum ? TypeHelper.Name(value) : value.ToString();

                    if (categoryName != null)
                        fieldCategory.AddChild(
                            CreateCategory(categoryName)
                            .AddFilter(field, value)
                        );
                }

                if (!TypeHelper.FieldIsTypeHelpered(field))
                    fieldCategory.AddChild(
                        CreateCategory($"< no {fieldCategory.Name.ToLower()} >")
                        .AddFilterBlank(field)
                    );

                return fieldCategory;
            }
            catch 
            {
                return null;
            }
        }

        private Category<TField, TDAO>? ByFieldsCategory()
        {
            if (FieldsByFields.Count == 0 && Settings.HideEmptyCategory)
                return null;

            Category<TField, TDAO> byFieldsCategory = CreateCategory("By Fields");

            foreach (TField field in FieldsByFields)
            {
                Category<TField, TDAO>? fieldCategory = FieldCategory(field);

                if (fieldCategory != null)
                    byFieldsCategory.AddChild(fieldCategory);
            }

            return byFieldsCategory;
        }

        private void LoadCategories(bool systemOnly)
        {
            RootCategory.Clear();
            RootCategory.Name = $"All {ListController.Name}";

            Categories<TField, TDAO>? systemCategories = ListController.SystemCategories;

            if (systemCategories?.Count > 0)
            {
                foreach (Category<TField, TDAO> category in systemCategories)
                    RootCategory.AddChild(category);
            }

            if (!systemOnly)
            {
                Category<TField, TDAO>? byFields = ByFieldsCategory();

                if (byFields?.Childs?.Count > 0)
                    RootCategory.AddChild(byFields);
            }
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
            SetContentSize(new Size(280, 1));

        private TreeNode? GetNodeByTag(object tag, TreeNodeCollection? treeNodes = null)
        {
            treeNodes ??= categorySelector.Nodes;

            foreach (TreeNode node in treeNodes)
            {
                if (node.Tag.Equals(tag))
                    return node;

                if (node.Nodes.Count == 0)
                    continue;
                
                TreeNode? findNode = GetNodeByTag(tag, node.Nodes);

                if (findNode != null)
                    return findNode;
            }

            return null;
        }

        private void FillTree()
        {
            StartLoading();
            Update();

            try
            {
                Category<TField, TDAO>? selectedNodeTag = null;

                if (categorySelector.SelectedNode != null)
                    selectedNodeTag = (Category<TField, TDAO>)categorySelector.SelectedNode.Tag;

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

            TreeNode? selectedNode = category == null ? null : GetNodeByTag(category);
            categorySelector.SelectedNode = selectedNode ?? categorySelector.Nodes[0];
        }

        private RootListDAO<TField, TDAO> FullList => ListController.FullItemsList;

        private TreeNode? CreateCategoryTreeNode(Category<TField, TDAO> category)
        {
            int itemsCount = ShowCount ? FullList.FilteredList(category).Count : 0;

            return
                ShowCount && Settings.HideEmptyCategory && itemsCount == 0
                    ? null
                    : !ShowCount || category.FilterIsEmpty
                        ? new TreeNode(category.ToString())
                            {
                                Tag = category
                            }
                        : new CountedTreeNode(category.ToString())
                            {
                                Tag = category,
                                Count = itemsCount
                            };
        }

        protected override void ApplySettingsInternal()
        {
            if (Observer[DAOSetting.ShowCategories])
                Visible = Settings.ShowCategories;

            if (Observer[DAOSetting.CategoryPanelPinned] &&
                (Pinned != Settings.CategoryPanelPinned))
                Pinned = Settings.CategoryPanelPinned;

            if ((Observer[DAOSetting.CategoryPanelPinned] || Observer[DAOSetting.CategoryPanelExpanded])
                && (Expanded != (Pinned && Settings.CategoryPanelExpanded)))
                Expanded = Pinned && Settings.CategoryPanelExpanded;

            if (Observer.CategoryFieldsChanged
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

        private void AddCategoryToSelector(Category<TField, TDAO> category, TreeNode? parentNode)
        {
            TreeNode? node = CreateCategoryTreeNode(category);

            if (node == null)
                return;

            foreach (Category<TField, TDAO> childCategory in category.Childs)
                AddCategoryToSelector(childCategory, node);

            if (Settings.HideEmptyCategory
                && category.FilterIsEmpty
                && node.Nodes.Count == 0)
                return;

            if (parentNode == null)
                categorySelector.Nodes.Add(node);
            else parentNode.Nodes.Add(node);
        }

        private void AfterSelectHandler(object? sender, TreeViewEventArgs e)
        {
            ActiveCategoryChanged?.Invoke(this, new CategoryEventArgs<TField, TDAO>(LastCategory, ActiveCategory));
            LastCategory = ActiveCategory;

            if (categorySelector.SelectedNode != null)
            {
                if (categorySelector.SelectedNode.IsExpanded)
                    categorySelector.SelectedNode.Collapse();
                else categorySelector.SelectedNode.Expand();
            }
        }

        public List<TField> FieldsByFields => 
            Settings.CategoryFields.Fields;
    }
}