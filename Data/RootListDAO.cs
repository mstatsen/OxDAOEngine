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

        public FieldModified<TField>? FieldModified { get; set; } //TODO: iterate all items and set its handlers after change this

        private void MemberFieldModified(FieldModifiedEventArgs<TField> e) =>
            FieldModified?.Invoke(e);

        private IListController<TField, TDAO>? listController;
        private IListController<TField, TDAO>? ListController
        {
            get
            {
                listController ??= DataManager.ListController<TField, TDAO>();
                return listController;
            }
        }
        private static readonly bool ListControllerExists = DataManager.ListControllerExists<TField, TDAO>();

        protected override void SetMemberHandlers(DAO member, bool set = true)
        {
            base.SetMemberHandlers(member, set);

            if (member is TDAO daoMember)
            {
                if (FieldModified != null)
                {
                    if (set)
                        daoMember.FieldModified += MemberFieldModified;
                    else daoMember.FieldModified -= MemberFieldModified;
                }

                if (ListControllerExists)
                {
                    daoMember.UseImageList = ListController!.UseImageList;

                    if (set)
                    {
                        daoMember.OnGetImageInfo += ListController.OnGetImageInfoHandler;
                        daoMember.OnUpdateImage += ListController.OnUpdateImageHanlder;
                    }
                    else
                    {
                        daoMember.OnGetImageInfo += ListController.OnGetImageInfoHandler;
                        daoMember.OnUpdateImage += ListController.OnUpdateImageHanlder;
                    }
                }
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

            if (sortings != null)
                List.Sort(new BySortingsComparer<TField, TDAO>(sortings));

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

                if (filter == null || filter.FilterIsEmpty)
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
            if (dao == null)
                return false;

            return Find(d => d.Equals(dao)) != null;
        }

        protected override bool AutoSorting => false;

        public bool FilterIsEmpty => Count == 0;

    }
}