using OxDAOEngine.Data;

namespace OxDAOEngine.Grid
{
    public class GridSelector<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public void FocusOnFirstRow()
        {
            foreach (DataGridViewRow row in grid.Rows)
                if (row.Visible)
                {
                    FocusOnRow(row.Index);

                    if (row.Selected)
                        row.Selected = false;

                    row.Selected = true;
                    return;
                }
        }

        public GridSelector(DataGridView gridView)
        {
            grid = gridView;
            savedSelection = new();
        }

        public void SaveState()
        {
            topRow = grid.FirstDisplayedScrollingRowIndex;
            savedSelection.CopyFrom(GetSelectedItems());
        }

        public void RestoreState()
        {
            SelectItems();
            FocusOnRow(topRow);
        }

        public void FocusOnRow(int topRowIndex)
        {
            if (!grid.Visible 
                || grid.RowCount is 0)
                return;

            try
            {
                if (topRowIndex > -1
                    && topRowIndex < grid.RowCount
                    && grid.Rows[topRowIndex].Visible)
                    grid.FirstDisplayedScrollingRowIndex = topRowIndex;
                else
                    FocusOnFirstRow();
            }
            catch
            { 
            }
        }

        public RootListDAO<TField, TDAO> GetSelectedItems()
        {
            RootListDAO<TField, TDAO> list = new();

            foreach (DataGridViewRow row in grid.SelectedRows)
                if (row.Visible)
                {
                    TDAO? item = GetDaoFromRow<TDAO>(row);

                    if (item is not null)
                        list.Add(item);
                }

            return list;
        }

        public X? GetDaoFromRow<X>(DataGridViewRow? row) where X : TDAO =>
            (X?)row?.Tag;

        public X? GetDaoFromRow<X>(int rowIndex) where X : TDAO =>
            rowIndex < 0 ? null : GetDaoFromRow<X>(grid.Rows[rowIndex]);

        public TDAO? GetDaoFromRow(DataGridViewRow? row) =>
            GetDaoFromRow<TDAO>(row);

        public TDAO? GetDaoFromRow(int rowIndex) =>
            GetDaoFromRow<TDAO>(rowIndex);

        private int topRow;
        private readonly RootListDAO<TField, TDAO> savedSelection;
        private readonly DataGridView grid;

        private void SelectItems()
        {
            if (savedSelection.Count is 0)
                return;

            foreach (DataGridViewRow row in grid.Rows)
                row.Selected =
                    row.Visible
                    && savedSelection.Contains(i => i.Equals(GetDaoFromRow<TDAO>(row)));

            if (grid.SelectedRows.Count is 0 
                && grid.Rows.Count > 0)
                FocusOnFirstRow();
        }

        public DataGridViewRow? CurrentRow =>
            grid.SelectedRows.Count > 0
                ? grid.SelectedRows[0]
                : null;

        public int CurrentRowIndex
        {
            get
            {
                DataGridViewRow? currentRow = CurrentRow;
                return currentRow is null 
                    ? -1 
                    : currentRow.Index;
            }
        }

        public TDAO? FocusNextRow()
        {
            int currentRowIndex = CurrentRowIndex;

            if (currentRowIndex >= grid.RowCount)
                return null;

            grid.ClearSelection();
            DataGridViewRow currentRow = grid.Rows[currentRowIndex + 1];
            currentRow.Selected = true;
            return GetDaoFromRow(currentRow);
        }

        public TDAO? FocusPrevRow()
        {
            int currentRowIndex = CurrentRowIndex;

            if (currentRowIndex <= 0)
                return null;

            grid.ClearSelection();
            DataGridViewRow currentRow = grid.Rows[currentRowIndex - 1];
            currentRow.Selected = true;
            return GetDaoFromRow(currentRow);
        }
    }
}