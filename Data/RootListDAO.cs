using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Sorting;

namespace OxXMLEngine.Data
{
    public sealed class RootListDAO<TField, TDAO> : ListDAO<TDAO>, IMatcher<TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public void CallSortChangeHandler() =>
           SortChangeHandler?.Invoke(this, EventArgs.Empty);

        public event EventHandler? SortChangeHandler;

        private bool sortingEnabled = true;

        protected override void AfterSave() =>
            sortingEnabled = true;

        protected override void BeforeSave() =>
            sortingEnabled = false;

        public void Sort(List<ISorting<TField, TDAO>>? sortings, bool notifyAboutSort = true)
        {
            if (!sortingEnabled)
                return;

            if (sortings != null)
                List.Sort(new BySortingsComparer<TField, TDAO>(sortings));

            if (notifyAboutSort)
                CallSortChangeHandler();
        }

        public RootListDAO<TField, TDAO> FilteredList(IMatcher<TDAO>? filter) =>
            FilteredList<RootListDAO<TField, TDAO>>(filter);

        public RootListDAO<TField, TDAO> FilteredList(IMatcher<TDAO>? filter,
            List<ISorting<TField, TDAO>> sortings)
        {
            RootListDAO<TField, TDAO> filteredList = FilteredList(filter);
            filteredList.Sort(sortings);
            return filteredList;
        }

        public RootListDAO<TField, TDAO> Distinct(Func<TDAO, RootListDAO<TField, TDAO>, bool> CheckUnique) =>
            Distinct<RootListDAO<TField, TDAO>>(CheckUnique);

        public bool Match(TDAO? dao)
        {
            if (dao == null)
                return false;

            return Find(d => d.Equals(dao)) != null;
        }

        protected override bool AutoSorting => false;

        public bool FilterIsEmpty => Count == 0;
    }
}