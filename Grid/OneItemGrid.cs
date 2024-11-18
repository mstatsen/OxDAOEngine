using OxDAOEngine.Data;
using OxDAOEngine.Data.Decorator;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;
using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;

namespace OxDAOEngine.Grid
{
    public delegate bool GetFlagValue<TField>(TField field, object? value)
        where TField : notnull, Enum;

    public class OneItemGrid<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private enum GridColumn
        {
            Flag,
            Field,
            Caption,
            Value
        };

        private static class ColumnHelper
        {
            internal static string Caption(GridColumn column) =>
                column switch
                {
                    GridColumn.Field => "Field",
                    GridColumn.Caption => "Value",
                    _ => string.Empty
                };

            internal static int Width(GridColumn column) =>
                column switch
                {
                    GridColumn.Flag => 8,
                    GridColumn.Field => 180,
                    GridColumn.Caption => 400,
                    _ => 0,
                };

            internal static DataGridViewCellStyle Style(GridColumn column) =>
                column switch
                {
                    GridColumn.Field => Styles.Cell_LeftAlignment,
                    _ => Styles.Cell_Default
                };
        }

        private readonly bool ShowFlagColumn;

        public OneItemGrid(bool showFlagColumn = false) : base(new(640, 480))
        {
            ShowFlagColumn = showFlagColumn;
            PrepareColumns();
            GenerateRows();
            GridView.CellPainting += GridViewCellPainting;
        }

        private void GridViewCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (sender is null 
                || e.RowIndex < 0)
                return;

            Color backColor = Colors.Lighter(7);
            Color foreColor = ForeColor;
            Color selectionForeColor = foreColor;

            if (e.ColumnIndex == FlagColumn.Index)
            {
                backColor = DAO.BoolValue(e.Value)
                    ? Color.FromArgb(0XFF, 0xAF, 0XFF, 0XAF)
                    : Color.FromArgb(0XFF, 0xFF, 0XAF, 0XAF);
                foreColor = backColor;
                selectionForeColor = new OxColorHelper(backColor).Darker();
            }

            e.CellStyle.ApplyStyle(
                new()
                {
                    BackColor = backColor,
                    SelectionBackColor = new OxColorHelper(backColor).Darker(),
                    ForeColor = foreColor,
                    SelectionForeColor = selectionForeColor,
                    Font = Styles.DefaultFont
                }
            );
        }

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
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            GridView.Parent = ContentContainer;
            GridView.RowTemplate.Height = 40;
            GridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            GridView.ColumnHeadersHeight = 40;
            GridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            GridView.Scroll += ScrollHandler;
            GridView.SelectionChanged += SelectionChangedHandler;
        }

        private DataGridViewColumn FlagColumn = default!;
        private DataGridViewColumn FieldColumn = default!;
        private DataGridViewColumn CaptionColumn = default!;
        private DataGridViewColumn ValueColumn = default!;

        private bool PrepareColumns()
        {
            int rowsCount = GridView.RowCount;
            GridView.Columns.Clear();
            FlagColumn = CreateColumn(GridColumn.Flag, ShowFlagColumn);
            FieldColumn = CreateColumn(GridColumn.Field);
            CaptionColumn = CreateColumn(GridColumn.Caption);
            ValueColumn = CreateColumn(GridColumn.Value, false);
            GridView.EnableHeadersVisualStyles = false;
            return rowsCount > 0;
        }

        private DataGridViewColumn CreateColumn(GridColumn gridColumn, bool visible = true)
        {
            DataGridViewColumn dataColumn =
                new DataGridViewTextBoxColumn
                {
                    Name = $"column{gridColumn}",
                    HeaderText = ColumnHelper.Caption(gridColumn),
                    Width = ColumnHelper.Width(gridColumn),
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Frozen = true,
                    Visible = visible
                };

            dataColumn.DefaultCellStyle.ApplyStyle(ColumnHelper.Style(gridColumn));
            dataColumn.HeaderCell.Style.ApplyStyle(ColumnHelper.Style(gridColumn));
            dataColumn.HeaderCell.Style.BackColor = EngineStyles.ElementControlColor;
            dataColumn.HeaderCell.Style.SelectionBackColor = EngineStyles.ElementControlColor;
            GridView.Columns.Add(dataColumn);
            return dataColumn;
        }

        public override Color DefaultColor =>
            new OxColorHelper(EngineStyles.ElementControlColor).Darker(2);

        protected override void PrepareColors()
        {
            base.PrepareColors();
            GridView.BackgroundColor = Colors.Lighter(7);
        }

        private readonly Dictionary<TField, DataGridViewRow> fieldsDictionary = new();

        private void GenerateRows()
        {
            foreach (TField field in fieldHelper.SynchronizedFields)
            {
                int rowIndex = GridView.Rows.Add();
                GridView.Rows[rowIndex].Tag = field;
                fieldsDictionary[field] = GridView.Rows[rowIndex];
                GridView[FieldColumn.Index, rowIndex].Value = fieldHelper.Name(field);
            }
        }

        private readonly TDAO tempItem = new();

        public void SetValue(TField field, object? value)
        {
            GridView[ValueColumn.Index, fieldsDictionary[field].Index].Value = value;
            tempItem[field] = value;
            GridView[CaptionColumn.Index, fieldsDictionary[field].Index].Value = 
                DataManager.DecoratorFactory<TField, TDAO>().
                    Decorator(DecoratorType.Table, tempItem!)!.Value(field);
            RecalcFlags();
        }

        public void RecalcFlags()
        {
            foreach (TField field in fieldHelper.SynchronizedFields)
                GridView[FlagColumn.Index, fieldsDictionary[field].Index].Value =
                    GetFlagValueHandler(
                        field,
                        GridView[ValueColumn.Index, fieldsDictionary[field].Index].Value
                    );
        }

        public object? GetValue(TField field) =>
            GridView[ValueColumn.Index, fieldsDictionary[field].Index].Value;

        private bool GetFlagValueHandler(TField field, object? value) =>
            GetFlagValue is null 
            || GetFlagValue.Invoke(field, value);

        public event GetFlagValue<TField>? GetFlagValue;

        private readonly FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();
        public TDAO? Item { get; private set; }
        public TField SelectedField
        {
            get =>(TField) GridView.SelectedRows[0].Tag!;
            set => fieldsDictionary[value].Selected = true;
        }

        public bool SelectionExists => GridView.SelectedRows.Count > 0;

        public void Fill(TDAO? item, List<TField>? availableFields = null)
        {
            Item = item;
            Text = 
                Item is not null 
                    ? Item.FullTitle() 
                    : "Empty";

            foreach (TField field in fieldHelper.SynchronizedFields)
            {
                SetValue(field, Item?[field]);

                if (availableFields is not null 
                    && !availableFields.Contains(field))
                    HideField(field);
            }
        }

        public new event EventHandler? Scroll;
        private void ScrollHandler(object? sender, EventArgs e) => 
            Scroll?.Invoke(sender, e);

        public event EventHandler? SelectionChanged;
        private void SelectionChangedHandler(object? sender, EventArgs e) =>
            SelectionChanged?.Invoke(sender, e);

        public void HideField(TField field) => 
            fieldsDictionary[field].Visible = false;

        public void HideFields(List<TField> fields)
        {
            foreach (TField field in fields)
                HideField(field);
        }

        public void ShowField(TField field) =>
            fieldsDictionary[field].Visible = true;

        public void ShowAllFields()
        {
            foreach (TField field in fieldsDictionary.Keys)
                ShowField(field);
        }

        public int FirstDisplayedScrollingRowIndex
        {
            get => GridView.FirstDisplayedScrollingRowIndex;
            set => GridView.FirstDisplayedScrollingRowIndex = value;
        }
    }
}