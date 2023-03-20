using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory.BatchUpdate;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Decorator;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Grid
{
    public class CustomGridColumns<TField, TDAO> : Dictionary<CustomGridColumn<TField, TDAO>, DataGridViewColumn>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public CustomGridColumn<TField, TDAO>? GetCustomColumn(DataGridViewColumn column)
        { 
            foreach (var item in this)
                if (item.Value == column)
                    return item.Key;

            return null;
        }
    }

    public class ItemsGrid<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly GridFieldColumns<TField> gridFieldColumns = new();
        private readonly CustomGridColumns<TField, TDAO> customGridColumns = new();
        public EventHandler? GridFillCompleted;
        public EventHandler? CurrentItemChanged;

        private List<TField>? fields;
        public List<TField>? Fields 
        { 
            get => fields;
            set
            {
                fields = value;
                PrepareColumns();
            }
        }

        private List<CustomGridColumn<TField, TDAO>>? additionalColumns;
        public List<CustomGridColumn<TField, TDAO>>? AdditionalColumns 
        {
            get => additionalColumns;
            set
            {
                additionalColumns = value;
                PrepareColumns();
            }
        }

        private readonly GridPainter<TField, TDAO>? Painter;

        public ItemsGrid(GridUsage usage = GridUsage.Edit) : this(null, usage) { }

        public ItemsGrid(RootListDAO<TField, TDAO>? itemsList, GridUsage usage = GridUsage.Edit)
        {
            customItemsList = itemsList;
            ListController.ItemsSortChangeHandler += (s, e) => SortGrid();
            ListController.AddHandler += (d, e) => d.ChangeHandler += ItemChanged;
            selector = new GridSelector<TField, TDAO>(GridView);
            GridView.DoubleClick += (s, e) => ExecuteAction(OxToolbarAction.Edit);
            GridView.SelectionChanged += GridSelectionChangedHandler;
            GridView.SortingChanged += GridSortingChangeHandler;
            GridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            GridView.ColumnHeadersHeight = 40;
            GridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            Usage = usage;

            ReadOnly = GridUsageHelper.IsReadOnly(usage);
            Painter = ListController.ControlFactory.CreateGridPainter(gridFieldColumns, usage);

            if (Painter != null)
                GridView.CellPainting += Painter.CellPainting_Handler;

            GridView.MultiSelect = Usage != GridUsage.SelectItem;
            PrepareColumns();
        }

        private void GridSortingChangeHandler(object? sender, DataGridViewCellMouseEventArgs e)
        {
            List<ISorting<TField, TDAO>> newSortings = new();

            foreach (var sorting in GridView.ColumnSorting)
            {
                if (gridFieldColumns.ContainsValue(sorting.Key))
                    newSortings.Add(
                        new FieldSorting<TField, TDAO>(
                            gridFieldColumns.GetField(sorting.Key.Index),
                            sorting.Value)
                    );
                else
                if (customGridColumns.ContainsValue(sorting.Key))
                {
                    CustomGridColumn<TField, TDAO>? customGridColumn = customGridColumns.GetCustomColumn(sorting.Key);

                    if (customGridColumn != null)
                        newSortings.Add(new GridSorting<TField, TDAO>(customGridColumn, sorting.Value));
                }
            }

            if (Usage == GridUsage.Edit && customItemsList == null)
            {
                ListController.Settings.Sortings.SortingsList = newSortings;
                ListController.Sort();
            }
            else
            {
                ItemsList.Sort(newSortings, true);
                SortGrid();
            }
        }

        public void Renew() =>
            Fill(null, PrepareColumns());

        private void SetGridSorting()
        {
            GridView.ColumnSorting.Clear();

            foreach (FieldSorting<TField, TDAO> sorting in ListController.Settings.Sortings)
            {
                if (!gridFieldColumns.TryGetValue(sorting.Field, out var column))
                    continue;

                GridView.ColumnSorting.Add(column, sorting.SortOrder);
                column.HeaderCell.SortGlyphDirection = sorting.SortOrder;
            }
        }
        
        private RootListDAO<TField, TDAO>? customItemsList;

        public RootListDAO<TField, TDAO>? CustomItemsList
        {
            get => customItemsList;
            set
            {
                customItemsList = value;
                ClearGrid();
            }
        }

        public RootListDAO<TField, TDAO> ItemsList =>
            customItemsList ?? (Usage == GridUsage.Edit
                    ? DataManager.VisibleItemsList<TField, TDAO>()
                    : DataManager.FullItemsList<TField, TDAO>());

        private void IterateItems(Func<TDAO, int> iterator, IMatcher<TField>? filter = null) => 
            ItemsList.Iterate(iterator, filter);

        public void ClearGrid()
        {
            SuspendLayout();

            try
            {
                GridView.Rows.Clear();
                IterateItems(UnSetChangeHandler);
            }
            finally
            {
                ResumeLayout();
            }
        }

        public void Fill(IMatcher<TField>? filter = null, bool force = false)
        {
            SetGridSorting();

            if (!ListOfItemsChanged() && !force)
            {
                SortGrid();
                return;
            }
                
            SaveState();
            ClearGrid();
            SuspendLayout();

            try
            {
                IterateItems(AppendItem, filter);
            }
            finally
            {
                RestoreState();
                ResumeLayout();
                NotifyAboutFill();
            }
        }

        public void ExecuteAction(OxToolbarAction action)
        {
            if (readOnly)
            {
                if (action == OxToolbarAction.Edit && CurrentItem != null)
                    ListController.ViewItem(CurrentItem);

                return;
            }

            switch (action)
            {
                case OxToolbarAction.New:
                    ListController.AddItem();
                    break;
                case OxToolbarAction.Edit:
                    if (CurrentItem != null)
                        ListController.EditItem(CurrentItem); 
                    break;
                case OxToolbarAction.Copy:
                    if (CurrentItem != null)
                        ListController.CopyItem(CurrentItem);
                    break;
                case OxToolbarAction.Delete:
                    ListController.Delete(GetSelectedItems());
                    break;
                case OxToolbarAction.Update:
                    BatchUpdate();
                    break;
                case OxToolbarAction.Export:
                    ListController.ExportController.Export();
                    break;
            }
        }

        private BatchUpdateForm<TField, TDAO>? batchUpdateForm;

        private void BatchUpdate()
        {
            if (batchUpdateForm == null)
                batchUpdateForm = new BatchUpdateForm<TField, TDAO>()
                {
                    ItemsGetter = GetSelectedItems,
                    BatchUpdateCompleted = (s, e) => BatchUpdateCompleted?.Invoke(s, e)
                };

            batchUpdateForm.ShowDialog();
        }

        public EventHandler? BatchUpdateCompleted;

        public TDAO? CurrentItem =>
            GridView.SelectedRows.Count > 0
                ? selector.GetDaoFromRow(GridView.SelectedRows[0]) 
                : null;

        public Color CurrentItemBackColor =>
            Painter != null && CurrentItem != null 
                ? Painter.GetCellStyle(CurrentItem, gridFieldColumns.First().Key).BackColor 
                : EngineStyles.DefaultGridRowColor;

        public RootListDAO<TField, TDAO> GetSelectedItems() =>
            selector.GetSelectedItems();

        public int SelectedCount =>
            GetSelectedItems().Count;

        public void SelectFirstItem() =>
            selector.FocusOnFirstRow();

        protected void GridSelectionChangedHandler(object? sender, EventArgs e) =>
            CurrentItemChanged?.Invoke(sender, e);

        private bool ListOfItemsChanged()
        {
            List<int> oldItemsHashes = new();

            foreach (DataGridViewRow row in GridView.Rows)
            {
                TDAO? item = selector.GetDaoFromRow<TDAO>(row);

                if (item != null)
                    oldItemsHashes.Add(item.GetHashCode());
            }

            List<int> currentItemsHashes = new();

            foreach (TDAO item in ListController.VisibleItemsList)
                currentItemsHashes.Add(item.GetHashCode());

            if (currentItemsHashes.Count != oldItemsHashes.Count)
                return true;

            for (int i = 0; i < oldItemsHashes.Count; i++)
                if (!oldItemsHashes[i].Equals(currentItemsHashes[i]))
                    return true;

            return false;
        }

        public int GetRowIndex(TDAO item)
        {
            foreach (DataGridViewRow row in GridView.Rows)
                if (row.Tag == item)
                    return row.Index;

            return -1;
        }

        private void ItemChanged(DAO dao, DAOEntityEventArgs? e)
        {
            if (e == null)
                return;

            if (dao is not TDAO tDao)
                return;

            int rowIndex = GetRowIndex(tDao);

            switch (e.Operation)
            {
                case DAOOperation.Insert:
                    if (rowIndex == -1)
                        AppendItem(tDao);

                    LocateItem(tDao);
                    break;
                case DAOOperation.Modify:
                    if (rowIndex > -1)
                    {
                        UpdateValues(rowIndex);
                        GridView.InvalidateRow(rowIndex);
                    }
                    break;
                case DAOOperation.Delete:
                    if (rowIndex > -1)
                    {
                        dao.ChangeHandler -= ItemChanged;
                        GridView.Rows.RemoveAt(rowIndex);
                    }
                    break;
            }
        }

        protected int AppendItem(TDAO item)
        {
            int rowIndex = GridView.Rows.Add();
            GridView.Rows[rowIndex].Tag = item;
            UpdateValues(rowIndex);
            SetChangeHandler(item);
            return rowIndex;
        }

        protected void LocateItem(TDAO item)
        {
            GridView.ClearSelection();
            SortGrid();
            int rowIndex = GetRowIndex(item);

            if ((rowIndex > -1) && GridView.Rows[rowIndex].Visible)
            {
                selector.FocusOnRow(rowIndex);
                GridView.Rows[rowIndex].Selected = true;
            }
        }

        private void SortGrid()
        {
            SaveState();
            SuspendLayout();

            try
            {
                GridView.Sort(
                    new GridComparer<TField, TDAO>(selector, ItemsList)
                );
            }
            finally
            {
                ResumeLayout();
                RestoreState();
            }
        }

        protected void SaveState() =>
            selector.SaveState();

        protected void RestoreState()
        {
            GridView.SelectionChanged -= GridSelectionChangedHandler;

            try
            {
                selector.RestoreState();
            }
            finally
            {
                GridView.SelectionChanged += GridSelectionChangedHandler;
                GridSelectionChangedHandler(this, EventArgs.Empty);
            }
        }

        private void SetChangeHandler(TDAO item) =>
            item.ChangeHandler += ItemChanged;

        private int UnSetChangeHandler(TDAO item)
        {
            item.ChangeHandler -= ItemChanged;
            return 0;
        }


        protected void UpdateValues(int rowIndex)
        {
            TDAO? item = selector.GetDaoFromRow(rowIndex);

            if (item == null)
                return;

            Decorator<TField, TDAO> decorator = DecoratorFactory.Decorator(
                DecoratorType.Table, item
            );

            foreach (TField field in gridFieldColumns.Keys)
                GridView[gridFieldColumns[field].Index, rowIndex].Value = decorator[field] ?? string.Empty;

            foreach (var column in customGridColumns)
                GridView[column.Value.Index, rowIndex].Value = column.Key.ValueGetter(item) ?? string.Empty;
        }


        private readonly FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

        private void CreateAdditionalColumn(CustomGridColumn<TField, TDAO> gridColumn)
        {
            DataGridViewColumn dataColumn = new DataGridViewTextBoxColumn
            {
                Name = "column" + gridColumn.Text,
                HeaderText = gridColumn.Text,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                Width = gridColumn.Width + 20,
                Frozen = false
            };
            dataColumn.DefaultCellStyle.ApplyStyle(EngineStyles.Cell_Default);
            dataColumn.HeaderCell.Style.ApplyStyle(EngineStyles.Cell_Default);
            dataColumn.HeaderCell.Style.BackColor = EngineStyles.ElementControlColor;
            dataColumn.HeaderCell.Style.SelectionBackColor = EngineStyles.ElementControlColor;
            GridView.Columns.Add(dataColumn);
            customGridColumns.Add(gridColumn, dataColumn);
        }

        private void CreateColumn(TField field)
        {
            FieldType fieldType = fieldHelper.GetFieldType(field);
            DataGridViewColumn dataColumn = fieldType == FieldType.Image
                ? new DataGridViewImageColumn()
                : new DataGridViewTextBoxColumn();

            dataColumn.Name = "column" + TypeHelper.ShortName(field);
            dataColumn.HeaderText = fieldHelper.ColumnCaption(field);
            dataColumn.SortMode = fieldType == FieldType.Image 
                ? DataGridViewColumnSortMode.NotSortable 
                : DataGridViewColumnSortMode.Programmatic;
            dataColumn.Width = fieldHelper.ColumnWidth(field) + 20;
            dataColumn.Frozen =
                (GridView.ColumnCount == 0 || GridView.Columns[GridView.Columns.Count - 1].Frozen) &&
                fieldHelper.MandatoryFields.Contains(field);
            dataColumn.DefaultCellStyle.ApplyStyle(fieldHelper.ColumnStyle(field));
            dataColumn.HeaderCell.Style.ApplyStyle(fieldHelper.ColumnStyle(field));
            dataColumn.HeaderCell.Style.BackColor = EngineStyles.ElementControlColor;
            dataColumn.HeaderCell.Style.SelectionBackColor = EngineStyles.ElementControlColor;
            GridView.Columns.Add(dataColumn);
            gridFieldColumns.Add(field, dataColumn);
        }

        private bool PrepareColumns()
        {
            int rowsCount = GridView.RowCount;
            gridFieldColumns.Clear();
            customGridColumns.Clear();
            GridView.Columns.Clear();

            foreach (TField field in fieldHelper.FullList(FieldsVariant.Table))
                if (IsAvailableColumn(field))
                    CreateColumn(field);

            if (additionalColumns != null)
                foreach (CustomGridColumn<TField, TDAO> gridColumn in additionalColumns)
                    CreateAdditionalColumn(gridColumn);

            GridView.EnableHeadersVisualStyles = false;
            return rowsCount > 0;
        }

        protected bool IsAvailableColumn(TField field)
        {
            if (fields != null)
                return fields.Contains(field);

            return Usage switch
            {
                GridUsage.SelectItem or
                GridUsage.ChooseItems =>
                    fieldHelper.MandatoryFields.Contains(field),
                _ =>
                    ListController.Settings.TableFields.Fields.Contains(field),
            };
        }

        protected void SetRowVisible(DataGridViewRow row, bool visible)
        {
            if (row.Visible == visible)
                return;

            row.Visible = visible;

            if (row.Visible)
            {
                UpdateValues(row.Index);
                GridView.InvalidateRow(row.Index);
            }
        }

        private void NotifyAboutFill() =>
            GridFillCompleted?.Invoke(this, EventArgs.Empty);


        protected IListController<TField, TDAO> ListController = 
            DataManager.ListController<TField, TDAO>();

        protected GridSelector<TField, TDAO> selector;
        private bool readOnly = false;
        private readonly DecoratorFactory<TField, TDAO> DecoratorFactory = 
            DataManager.DecoratorFactory<TField, TDAO>();

        public void ClearQuickFilter()
        {
            SuspendLayout();

            try
            {
                foreach (DataGridViewRow row in GridView.Rows)
                    SetRowVisible(row, true);
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        public void ApplyQuickFilter(IMatcher<TField>? quickFilter)
        {
            if (quickFilter == null || quickFilter.FilterIsEmpty)
            {
                ClearQuickFilter();
                return;
            }

            SuspendLayout();
            SaveState();

            try
            {
                foreach (DataGridViewRow row in GridView.Rows)
                    SetRowVisible(row, quickFilter.Match(selector.GetDaoFromRow(row)));
            }
            finally
            {
                RestoreState();
                ResumeLayout(true);
            }
        }

        public readonly GridUsage Usage;

        public readonly ItemsGridToolBar ToolBar = new();

        public override void ReAlignControls()
        {
            base.ReAlignControls();
            GridView.SendToBack();
            ToolBar.SendToBack();
        }

        public new event EventHandler DoubleClick
        {
            add => GridView.DoubleClick += value;
            remove => GridView.DoubleClick -= value;
        }

        public DataGridViewSelectedRowCollection SelectedRows => GridView.SelectedRows;

        public readonly OxDataGridView GridView = new()
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToOrderColumns = true,
            AllowUserToResizeColumns = false,
            AllowUserToResizeRows = false,
            BorderStyle = BorderStyle.None,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            CellBorderStyle = DataGridViewCellBorderStyle.Single,
            EditMode = DataGridViewEditMode.EditProgrammatically,
            ReadOnly = true,
            RowHeadersVisible = false,
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        public override Color DefaultColor =>
            new OxColorHelper(EngineStyles.ElementControlColor).Darker(2);

        public event ToolbarActionClick ToolbarActionClick
        {
            add => ToolBar.ToolbarActionClick += value;
            remove => ToolBar.ToolbarActionClick -= value;
        }

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            GridView.Parent = ContentContainer;
            GridView.RowTemplate.Height = 40;
            PrepareToolBar();
            Paddings.LeftOx = OxSize.Medium;
        }

        private void PrepareToolBar()
        {
            ToolBar.Parent = ContentContainer;
            ToolBar.Dock = DockStyle.Top;
            ToolBar.Margins.TopOx = OxSize.None;
            ToolBar.Margins.BottomOx = OxSize.Medium;
            ToolBar.Borders.RightOx = OxSize.None;
            ToolBar.Borders.LeftOx = OxSize.None;
            ToolBar.AllowEditingActions = false;
        }

        protected override void SetHandlers()
        {
            base.SetHandlers();
            GridView.SelectionChanged += (s, e) => 
                ToolBar.AllowEditingActions = GridView.SelectedRows.Count > 0 
                && GridView.SelectedRows[0].Visible;
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();
            ToolBar.BaseColor = BaseColor;
            GridView.BackgroundColor = Colors.Lighter(7);
        }

        public bool ReadOnly
        {
            get => readOnly;
            set
            {
                readOnly = value;
                ToolBar.Visible = !value;
            }
        }
    }
}