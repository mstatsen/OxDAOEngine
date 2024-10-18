using System.Collections;

namespace OxDAOEngine.Data
{
    public interface IListDAO : IEnumerable
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

    public interface IListDAO<T> : IEnumerable<T>, IListDAO
        where T : DAO, new()
    {
        void LinkedCopyFrom(IListDAO<T>? otherList);
        void NotifyAboutItemAdded(T item);
        T Add();
        T Add(T item);
        void AddRange(IEnumerable<T> collection);
        void RemoveAll(Predicate<T> match);
        void RemoveAll(Predicate<T> match, bool needSaveHistory);
        bool Remove(T item);
        bool Remove(T item, bool needSaveHistory);
        bool Remove(Predicate<T> match);
        bool Remove(Predicate<T> match, bool needSaveHistory);
        int IndexOf(T? value);
        T? Find(Predicate<T> match);
        List<T> FindAll(Predicate<T> match);
        bool Contains(Predicate<T> match);
        bool Contains(T item);
        T? First { get; }
        TListDAO Distinct<TListDAO>(Func<T, TListDAO, bool> CheckUnique)
            where TListDAO : ListDAO<T>, new();
        ListDAO<T> Distinct<ListDAO>(Func<T, ListDAO<T>, bool> CheckUnique);
    }
}