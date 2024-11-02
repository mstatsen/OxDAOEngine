using System.Collections;

namespace OxDAOEngine.Data
{
    public interface IListDAO : IDAO, IEnumerable
    {
        bool SaveEmptyList { get; set; }
        void Sort();
        List<string?> StringList { get; }
        int Count { get; }
        public event DAOEntityEventHandler? ItemAddHandler;
        public event DAOEntityEventHandler? ItemRemoveHandler;
        int ModifiedCount { get; }
        string ToString();
        string OneColumnText();
        List<object> ObjectList { get; set; }
    }

    public interface IListDAO<TDAO> : IEnumerable<TDAO>, IListDAO
        where TDAO : IDAO, new()
    {
        void LinkedCopyFrom(IListDAO<TDAO>? otherList);
        void NotifyAboutItemAdded(TDAO item);
        TDAO Add();
        TDAO Add(TDAO item);
        void AddRange(IEnumerable<TDAO> collection);
        void RemoveAll(Predicate<TDAO> match);
        void RemoveAll(Predicate<TDAO> match, bool needSaveHistory);
        bool Remove(TDAO item);
        bool Remove(TDAO item, bool needSaveHistory);
        bool Remove(Predicate<TDAO> match);
        bool Remove(Predicate<TDAO> match, bool needSaveHistory);
        int IndexOf(TDAO? value);
        TDAO? Find(Predicate<TDAO> match);
        List<TDAO> FindAll(Predicate<TDAO> match);
        bool Contains(Predicate<TDAO> match);
        bool Contains(TDAO item);
        TDAO? First { get; }
        TListDAO Distinct<TListDAO>(Func<TDAO, TListDAO, bool> CheckUnique)
            where TListDAO : IListDAO<TDAO>, new();
        IListDAO<TDAO> Distinct<ListDAO>(Func<TDAO, IListDAO<TDAO>, bool> CheckUnique);
    }
}