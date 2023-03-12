using OxXMLEngine.Data.Filter;
using OxXMLEngine.Grid;

namespace OxXMLEngine.Data.Sorting
{
    public class GridSorting<TField, TDAO> : ISorting<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public SortOrder SortOrder { get; set; }
        public readonly CustomGridColumn<TField, TDAO> GridColumn;

        public GridSorting(CustomGridColumn<TField, TDAO> gridColumn, SortOrder sortOrder)
        {
            SortOrder = sortOrder;
            GridColumn = gridColumn;
        }

        public int Compare(TDAO? x, TDAO? y)
        {
            int result;

            if (x == null)
                result = y == null ? 0 : -1;
            else
            if (y == null)
                result = 1;
            else
            {

                string? xValue = GridColumn.ValueGetter(x)?.ToString();
                string? yValue = GridColumn.ValueGetter(y)?.ToString();

                if (xValue == null)
                    result = yValue == null ? 0 : -1;
                else
                if (yValue == null)
                    result = 1;
                else
                    result = xValue.CompareTo(yValue);
            }

            if (result != 0 &&
                SortOrder == SortOrder.Descending)
                result = -result;

            return result;
        }
    }
}