using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Sorting;

namespace OxDAOEngine.Data
{
    public class RootListDAO<TField, TDAO> : ListDAO<TDAO>, IMatcher<TField>, IRootListDAO<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public void CallSortChangeHandler() =>
           SortChangeHandler?.Invoke(this, EventArgs.Empty);

        private FieldModified<TField>? fieldModified;

        public FieldModified<TField>? FieldModified 
        { 
            get => fieldModified;
            set
            {
                fieldModified = value;
                SetMembersHandlers();
            }
        }

        public RootListDAO<TField, TDAO> FindAllRoot(Predicate<TDAO> match)
        {
            List<TDAO> list = List.FindAll(match);

            RootListDAO<TField, TDAO> result = new();

            foreach (TDAO t in list)
                result.Add(t);

            return result;
        }

        private void MemberFieldModified(FieldModifiedEventArgs<TField> e) =>
            FieldModified?.Invoke(e);

        protected override void SetMemberHandlers(IDAO member, bool set = true)
        {
            base.SetMemberHandlers(member, set);

            if (member is TDAO daoMember)
            {
                daoMember.FieldModified -= MemberFieldModified;

                if (FieldModified is not null)
                    if (set)
                        daoMember.FieldModified += MemberFieldModified;
            }
        }

        public EventHandler? SortChangeHandler { get; set; }

        private bool sortingEnabled = true;

        protected override void AfterSave() =>
            sortingEnabled = true;

        protected override void BeforeSave() =>
            sortingEnabled = false;

        public void Sort(List<ISorting<TField, TDAO>>? sortings, bool notifyAboutSort = true)
        {
            if (!sortingEnabled)
                return;

            if (sortings is not null)
                List.Sort(
                    new BySortingsComparer<TField, TDAO>(sortings)
                );

            if (notifyAboutSort)
                CallSortChangeHandler();
        }

        public RootListDAO<TField, TDAO> FilteredList(IMatcher<TField>? filter) =>
            FilteredList<RootListDAO<TField, TDAO>>(filter);

        public void Iterate(Func<TDAO, int> iterator, IMatcher<TField>? filter)
        {
            foreach (TDAO item in FilteredList<ListDAO<TDAO>>(filter))
                iterator(item);
        }

        public TList FilteredList<TList>(IMatcher<TField>? filter)
            where TList : ListDAO<TDAO>, new()
        {
            TList filteredList = new();
            filteredList.StartLoading();

            try
            {

                if (filter is null 
                    || filter.FilterIsEmpty)
                    filteredList.AddRange(List);
                else
                    foreach (TDAO item in List)
                        if (filter.Match(item))
                            filteredList.Add(item);
            }
            finally
            {
                filteredList.FinishLoading();
            }

            return filteredList;
        }

        public RootListDAO<TField, TDAO> FilteredList(IMatcher<TField>? filter,
            List<ISorting<TField, TDAO>> sortings)
        {
            RootListDAO<TField, TDAO> filteredList = FilteredList(filter);
            filteredList.Sort(sortings);
            return filteredList;
        }

        public RootListDAO<TField, TDAO> Distinct(Func<TDAO, RootListDAO<TField, TDAO>, bool> CheckUnique) =>
            Distinct<RootListDAO<TField, TDAO>>(CheckUnique);

        public bool Match(IFieldMapping<TField>? dao)
        {
            if (dao is null)
                return false;

            return Find(d => d.Equals(dao)) is not null;
        }

        protected override bool AutoSorting => false;

        public bool FilterIsEmpty => Count == 0;

        public new event DAOEntityEventHandler<TDAO>? ItemAddHandler;
        public new event DAOEntityEventHandler<TDAO>? ItemRemoveHandler;

        protected override void CallItemAddHandler(TDAO item, DAOEntityEventArgs args) =>
            ItemAddHandler?.Invoke(item, args);

        protected override void CallItemRemoveHandler(TDAO item, DAOEntityEventArgs args) =>
            ItemRemoveHandler?.Invoke(item, args);

        public RootListDAO<TField, TDAO> Reverse()
        {
            List.Reverse();
            return this;
        }
    }
}