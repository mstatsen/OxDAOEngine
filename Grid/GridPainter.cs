using OxDAOEngine.Data;

namespace OxDAOEngine.Grid
{
    public abstract class GridPainter<TField, TDAO>
        where TField : notnull, Enum
        where TDAO: DAO
    {
        public GridPainter(GridFieldColumns<TField> columnsDictionary) =>
            ColumnsDictionary = columnsDictionary;

        public void CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (sender is null 
                || e.RowIndex < 0)
                return;

            TDAO? item = (TDAO?)((DataGridView)sender).Rows[e.RowIndex].Tag;

            if (item is null)
                return;

            SetCellStyle(item, e);
        }

        public abstract DataGridViewCellStyle? GetCellStyle(TDAO? item, TField field, bool selected = false);

        private  void SetCellStyle(TDAO item, DataGridViewCellPaintingEventArgs e) =>
            e.CellStyle.ApplyStyle(
                GetCellStyle(
                    item,
                    ColumnsDictionary.GetField(e.ColumnIndex), 
                    (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected
                )
            );

        protected readonly GridFieldColumns<TField> ColumnsDictionary;
    }
}