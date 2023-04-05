using OxXMLEngine.Data.Filter;

namespace OxXMLEngine.Data
{
    public interface IRootListDAO<TField, TDAO> : IMatcher<TField>, IListDAO<TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        void CallSortChangeHandler();
        FieldModified<TField>? FieldModified { get; set; }
        EventHandler? SortChangeHandler { get; set; }
        void Sort(List<ISorting<TField, TDAO>>? sortings, bool notifyAboutSort = true);
        RootListDAO<TField, TDAO> FilteredList(IMatcher<TField>? filter);
        void Iterate(Func<TDAO, int> iterator, IMatcher<TField>? filter);
        TList FilteredList<TList>(IMatcher<TField>? filter)
            where TList : ListDAO<TDAO>, new();
        RootListDAO<TField, TDAO> FilteredList(IMatcher<TField>? filter, List<ISorting<TField, TDAO>> sortings);
        RootListDAO<TField, TDAO> Distinct(Func<TDAO, RootListDAO<TField, TDAO>, bool> CheckUnique);
        bool Modified { get; set; }
    }
}