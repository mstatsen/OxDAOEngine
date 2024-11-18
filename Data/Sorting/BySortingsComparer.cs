using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.Data.Sorting
{
    public class BySortingsComparer<TField, TDAO> : IComparer<TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly List<ISorting<TField, TDAO>> Sortings;

        public BySortingsComparer(List<ISorting<TField, TDAO>> sortings) =>
            Sortings = sortings;

        public int Compare(TDAO? x, TDAO? y)
        {
            if (x is null)
                return y is null ? 0 : -1;

            if (y is null)
                return 1;

            foreach (ISorting<TField, TDAO> sorting in Sortings)
            {
                int subResult = sorting.Compare(x, y);

                if (subResult == 0)
                    continue;

                return subResult;
            }

            return x.CompareTo(y);
        }
    }
}