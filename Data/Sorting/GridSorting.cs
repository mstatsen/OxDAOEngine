using OxDAOEngine.Data.Filter;
using OxDAOEngine.Grid;

namespace OxDAOEngine.Data.Sorting
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

            switch (x)
            {
                case null:
                    result = y is null 
                        ? 0 
                        : -1;
                    break;
                default:
                    if (y is null)
                        result = 1;
                    else
                    {

                        string? xValue = GridColumn.ValueGetter(x)?.ToString();
                        string? yValue = GridColumn.ValueGetter(y)?.ToString();

                        result = xValue switch
                        {
                            null => yValue is null 
                                ? 0 
                                : -1,
                            _ => yValue is null 
                                ? 1 
                                : xValue.CompareTo(yValue),
                        };
                    }

                    break;
            }

            if (result != 0 
                && SortOrder.Equals(SortOrder.Descending))
                result = -result;

            return result;
        }
    }
}