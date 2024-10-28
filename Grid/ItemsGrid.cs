using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Fields.Types;

namespace OxDAOEngine.Grid
{

    public class ItemsGrid<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public GridFieldColumns<TField> GridFieldColumns { get; } = new();
        protected readonly CustomGridColumns<TField, TDAO> customGridColumns = new();
        public EventHandler? GridFillCompleted;
        public EventHandler? CurrentItemChanged;
        private readonly Dictionary<DAO, DataGridViewRow> itemsDictionary = new();

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

        protected override void PrepareDialog(OxPanelViewer dialog)
        {
            base.PrepareDialog(dialog);
            dialog.Sizeble = true;
            dialog.CanMaximize = true;
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

        private GridPainter<TField, TDAO>? painter;
        public GridPainter<TField, TDAO>? Painter 
        { 
            get => painter;
            set
            {
                if (painter == value)
                    return;

                if (painter != null)
                    GridView.CellPainting -= painter.CellPainting;

                painter = value;

                if (painter != null)
                    GridView.CellPainting += painter.CellPainting;
            } 
        }

        protected readonly FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

        private void CreateAdditionalColumn(CustomGridColumn<TField, TDAO> gridColumn)
        {
            DataGridViewColumn dataColumn = new DataGridViewTextBoxColumn
            {
                Name = $"column{gridColumn.Text}",
                HeaderText = gridColumn.Text,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                Width = gridColumn.Width + 20,
                Frozen = false
            };
            dataColumn.DefaultCellStyle.ApplyStyle(Styles.Cell_Default);
            dataColumn.HeaderCell.Style.ApplyStyle(Styles.Cell_Default);
            dataColumn.HeaderCell.Style.BackColor = EngineStyles.ElementControlColor;
            dataColumn.HeaderCell.Style.SelectionBackColor = EngineStyles.ElementControlColor;
            GridView.Columns.Add(dataColumn);
            customGridColumns.Add(gridColumn, dataColumn);
        }

        private void CreateColumn(TField field)
        {
            if (GridFieldColumns.TryGetValue(field, out var _))
                return;

            FieldType fieldType = fieldHelper.GetFieldType(field);
            DataGridViewColumn dataColumn = fieldType is FieldType.Boolean or FieldType.Image
                ? new DataGridViewImageColumn()
                : new DataGridViewTextBoxColumn();

            dataColumn.Name = $"column{TypeHelper.ShortName(field)}";
            dataColumn.HeaderText = fieldHelper.ColumnCaption(field);
            dataColumn.SortMode = fieldType == FieldType.Image
                ? DataGridViewColumnSortMode.NotSortable
                : DataGridViewColumnSortMode.Programmatic;
            dataColumn.Width = fieldHelper.ColumnWidth(field) + 20;
            dataColumn.Frozen = Usage == GridUsage.Edit
                && (GridView.ColumnCount == 0 || GridView.Columns[GridView.Columns.Count - 1].Frozen) 
                && fieldHelper.MandatoryFields.Contains(field);
            dataColumn.DefaultCellStyle.ApplyStyle(fieldHelper.ColumnStyle(field));
            dataColumn.HeaderCell.Style.ApplyStyle(fieldHelper.ColumnStyle(field));
            dataColumn.HeaderCell.Style.BackColor = EngineStyles.ElementControlColor;
            dataColumn.HeaderCell.Style.SelectionBackColor = EngineStyles.ElementControlColor;
            GridView.Columns.Add(dataColumn);
            GridFieldColumns.Add(field, dataColumn);
        }

        public int RowCount => GridView.RowCount;

        private bool PrepareColumns()
        {
            int rowsCount = GridView.RowCount;
            GridFieldColumns.Clear();
            customGridColumns.Clear();
            GridView.Columns.Clear();

            foreach (TField field in fieldHelper.MandatoryFields)
                CreateColumn(field);

            foreach (TField field in UsageFieldList())
                CreateColumn(field);

            GridView.EnableHeadersVisualStyles = false;
            return rowsCount > 0;
        }

        private List<TField> UsageFieldList() => 
            fields ?? Usage switch
            {
                GridUsage.SelectItem or
                GridUsage.ChooseItems =>
                    fieldHelper.MandatoryFields,
                _ =>
                    SettingsTableFields
            };

        public ItemsGrid(GridUsage usage = GridUsage.Edit) : this(null, usage) { }

        public ItemsGrid(RootListDAO<TField, TDAO>? itemsList, GridUsage usage = GridUsage.Edit)
        {
            this.itemsList = itemsList;
            selector = new GridSelector<TField, TDAO>(GridView);
            GridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            GridView.ColumnHeadersHeight = 40;
            GridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            GridView.MouseDown += GridViewMouseDownHandler;
            Usage = usage;
            ReadOnly = GridUsageHelper.IsReadOnly(usage);
            GridView.MultiSelect = Usage == GridUsage.Edit || Usage == GridUsage.ChooseItems;
            PrepareColumns();
        }

        private void GridViewMouseDownHandler(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = GridView.HitTest(e.X, e.Y);
                GridView.ClearSelection();
                GridView.Rows[hitTest.RowIndex].Selected = true;
            }
        }

        protected virtual void ApplySortigns(List<ISorting<TField, TDAO>> newSortings)
        {
            if (Usage != GridUsage.Edit || itemsList != null)
            {
                ItemsList?.Sort(newSortings, true);
                SortGrid();
            }
        }

        protected virtual void GridSortingChangeHandler(object? sender, DataGridViewCellMouseEventArgs e)
        {
            List<ISorting<TField, TDAO>> newSortings = new();

            foreach (var sorting in GridView.ColumnSorting)
            {
                if (GridFieldColumns.ContainsValue(sorting.Key))
                    newSortings.Add(
                        new FieldSorting<TField, TDAO>(
                            GridFieldColumns.GetField(sorting.Key.Index),
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

            ApplySortigns(newSortings);
        }

        public void Renew() =>
            Fill(null, PrepareColumns());

        private IRootListDAO<TField, TDAO>? itemsList;

        public IRootListDAO<TField, TDAO>? CustomItemsList
        {
            get => itemsList;
            set
            {
                itemsList = value;
                ClearGrid();
            }
        }

        public virtual void Fill(IMatcher<TField>? filter = null, bool force = false)
        {
            if (!ListOfItemsChanged() && !force)
            {
                SortGrid();
                return;
            }

            BeginUpdate();
            ClearGrid();
            SuspendLayout();

            try
            {
                IterateItems(AppendItem, filter);
            }
            finally
            {
                EndUpdate();
                ResumeLayout();
                NotifyAboutFill();
            }
        }

        private bool ListOfItemsChanged()
        {
            if (ItemsList == null)
                return false;

            List<int> oldItemsHashes = new();

            foreach (DataGridViewRow row in GridView.Rows)
            {
                TDAO? item = selector.GetDaoFromRow<TDAO>(row);

                if (item != null)
                    oldItemsHashes.Add(item.GetHashCode());
            }

            List<int> currentItemsHashes = new();

            foreach (TDAO item in ItemsList)
                currentItemsHashes.Add(item.GetHashCode());

            if (currentItemsHashes.Count != oldItemsHashes.Count)
                return true;

            for (int i = 0; i < oldItemsHashes.Count; i++)
                if (!oldItemsHashes[i].Equals(currentItemsHashes[i]))
                    return true;

            return false;
        }

        private void NotifyAboutFill() =>
            GridFillCompleted?.Invoke(this, EventArgs.Empty);

        private void ClearQuickFilter()
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

        public readonly ItemsGridToolBar<TField,TDAO> ToolBar = new();

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

        public event OxActionClick<OxToolbarAction> ToolbarActionClick
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
            GridView.SelectionChanged += GridSelectionChangedHandler;
            GridView.SortingChanged += GridSortingChangeHandler;

            if (painter != null)
                GridView.CellPainting += painter.CellPainting;
        }

        protected virtual void UnSetHandlers()
        {
            base.SetHandlers();
            GridView.SelectionChanged -= GridSelectionChangedHandler;
            GridView.SortingChanged -= GridSortingChangeHandler;

            if (painter != null)
                GridView.CellPainting -= painter.CellPainting;
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
                ToolBar.ActionsVisible = !readOnly;
                ToolBar.Visible = ToolBar.Buttons.Find((b) => b.Visible) != null;
            }
        }

        private void SetRowVisible(DataGridViewRow row, bool visible)
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

        private readonly GridSelector<TField, TDAO> selector;
        private bool readOnly = false;

        public TDAO? CurrentItem =>
            GridView.SelectedRows.Count > 0
                ? selector.GetDaoFromRow(GridView.SelectedRows[0])
                : null;

        public int CurrentItemIndex => GridView.SelectedRows[0].Index;

        public RootListDAO<TField, TDAO> GetSelectedItems() =>
            selector.GetSelectedItems();

        public int SelectedCount =>
            GetSelectedItems().Count;

        public void SelectFirstItem() =>
            selector.FocusOnFirstRow();

        public bool Updating { get; private set; }

        public void BeginUpdate()
        {
            if (Updating)
                return;

            Updating = true;
            SaveState();
            UnSetHandlers();
        }

        public void EndUpdate()
        {
            if (!Updating)
                return;

            SetHandlers();
            RestoreState();
            Updating = false;
        }

        private void GridSelectionChangedHandler(object? sender, EventArgs e)
        {
            CurrentItemChanged?.Invoke(sender, e);

            if (!ReadOnly)
                ToolBar.AllowEditingActions = 
                    GridView.SelectedRows.Count > 0
                    && GridView.SelectedRows[0].Visible;
        }

        public int GetRowIndex(TDAO item)
        {
            itemsDictionary.TryGetValue(item, out DataGridViewRow? row);
            return row != null ? row.Index : -1;
        }

        public bool SelectItem(Predicate<TDAO> match) => 
            SelectItem(ItemsList?.Find(match));


        public bool SelectItem(TDAO? item)
        {
            if (item == null)
                return false;

            int rowIndex = GetRowIndex(item);

            if (rowIndex == -1)
                return false;
            
            GridView.Rows[rowIndex].Selected = true;
            return true;
        }

        protected void ItemChanged(DAO dao, DAOEntityEventArgs? e)
        {
            if (e == null)
                return;

            if (dao is not TDAO tDao)
                return;

            int rowIndex = GetRowIndex(tDao);

            switch (e.Operation)
            {
                case DAOOperation.Add:
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
                case DAOOperation.Remove:
                    if (rowIndex > -1)
                    {
                        dao.ChangeHandler -= ItemChanged;
                        itemsDictionary.Remove(dao);
                        GridView.Rows.RemoveAt(rowIndex);
                    }
                    break;
            }
        }

        private int AppendItem(TDAO item)
        {
            int rowIndex = GridView.Rows.Add();
            GridView.Rows[rowIndex].Tag = item;
            UpdateValues(rowIndex);
            SetChangeHandler(item);
            itemsDictionary[item] = GridView.Rows[rowIndex];
            return rowIndex;
        }

        private void LocateItem(TDAO item)
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

        protected void SortGrid()
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

        private void SaveState() =>
            selector.SaveState();

        private void RestoreState()
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

        protected virtual object? GetFieldValue(TField field, TDAO item) => item[field];

        private void UpdateValues(int rowIndex)
        {
            TDAO? item = selector.GetDaoFromRow(rowIndex);

            if (item == null)
                return;

            foreach (TField field in GridFieldColumns.Keys)
            {
                object? value = GetFieldValue(field, item) ?? string.Empty;

                if (fieldHelper.GetFieldType(field) == FieldType.Image
                    && value is Bitmap image 
                    && image.Height > GridView.RowTemplate.Height)
                    value = OxImageBoxer.BoxingImage(
                        image,
                        new()
                        {
                            Height = GridView.RowTemplate.Height,
                            Width = Width * (image.Height / GridView.RowTemplate.Height)
                        }
                    );

                GridView[GridFieldColumns[field].Index, rowIndex].Value = value;
            }

            foreach (var column in customGridColumns)
                GridView[column.Value.Index, rowIndex].Value = 
                    column.Key.ValueGetter(item) ?? string.Empty;
        }

        public virtual IRootListDAO<TField, TDAO>? ItemsList
        {
            get => CustomItemsList;
            set => CustomItemsList = value;
        }

        private void IterateItems(Func<TDAO, int> iterator, IMatcher<TField>? filter = null) =>
            ItemsList?.Iterate(iterator, filter);

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

        public bool IsFirstRecord =>
            CurrentItemIndex == 0;

        public bool IsLastRecord =>
            CurrentItemIndex == GridView.RowCount-1;

        protected virtual List<TField> SettingsTableFields => 
            fieldHelper.FullList(FieldsVariant.Table);

        public TDAO? GoNext() => 
            selector.FocusNextRow();

        public TDAO? GoPrev() =>
            selector.FocusPrevRow();
    }
}