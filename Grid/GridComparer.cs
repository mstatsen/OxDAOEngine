using OxDAOEngine.Data;
using System.Collections;

namespace OxDAOEngine.Grid
{
    public class GridComparer<TField, TDAO> : IComparer
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public GridComparer(GridSelector<TField, TDAO> gridSelector, 
            IRootListDAO<TField, TDAO>? itemsList)
        {
            GridSelector = gridSelector;
            ItemsList = itemsList;
        }

        private TDAO? GetDAO(object? row) => 
            GridSelector.GetDaoFromRow((DataGridViewRow?)row);

        private int DAOIndex(object? row) =>
            ItemsList is null 
                ? 0 
                : ItemsList.IndexOf(GetDAO(row));

        public int Compare(object? x, object? y) =>
            DAOIndex(x).CompareTo(DAOIndex(y));

        private readonly GridSelector<TField, TDAO> GridSelector;
        private readonly IRootListDAO<TField, TDAO>? ItemsList;
    }
}