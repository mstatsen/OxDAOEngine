using System.Collections;

namespace OxDAOEngine.Data
{
    public interface IListDAO<T> : IEnumerable<T>, IEnumerable
        where T : DAO, new()
    {
        void LinkedCopyFrom(IListDAO<T>? otherList);
        bool SaveEmptyList { get; set; }
        void Sort();
        List<string?> StringList { get; }
        void NotifyAboutItemAdded(T item);
        T Add();
        T Add(T item);
        void AddRange(IEnumerable<T> collection);
        void RemoveAll(Predicate<T> match);
        bool Remove(T item);
        bool Remove(Predicate<T> match);
        int IndexOf(T? value);
        T? Find(Predicate<T> match);
        List<T> FindAll(Predicate<T> match);
        bool Contains(Predicate<T> match);
        bool Contains(T item);
        int Count { get; }
        public event DAOEntityEventHandler? ItemAddHandler;
        public event DAOEntityEventHandler? ItemRemoveHandler;

        int ModifiedCount { get; }

        string ToString();

        string OneColumnText();

        T? First { get; }

        List<object> ObjectList { get; set; }
        TListDAO Distinct<TListDAO>(Func<T, TListDAO, bool> CheckUnique)
            where TListDAO : ListDAO<T>, new();

        ListDAO<T> Distinct<ListDAO>(Func<T, ListDAO<T>, bool> CheckUnique);
    }
}