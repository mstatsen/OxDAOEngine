using System.Xml;
using System.Collections;

namespace OxDAOEngine.Data
{
    public class ListDAO<T> : DAO, IEnumerable<T>, IEnumerable, IListDAO<T>
        where T : DAO, new()
    {
        public readonly List<T> List = new();
        public ListDAO() : base() { }

        public ListDAO(List<object> list) : base() =>
            ObjectList = list;

        public override void Init()
        {
            AutoSaveMembers = false;
            AutoLoadMembers = false;
        }

        public override void Clear()
        {
            foreach (DAO item in List)
                RemoveMember(item);

            List.Clear();
        }

        private class DAOComparer : IComparer<DAO>
        {
            public int Compare(DAO? x, DAO? y) =>
                x == null
                    ? y == null
                        ? 0
                        : -1
                    : x.CompareTo(y);
        }

        public void LinkedCopyFrom(IListDAO<T>? otherList)
        {
            if (otherList == null)
            {
                CopyFrom(null);
                return;
            }
            
            
            StartCoping();
            try
            {
                Clear();

                foreach (T item in otherList)
                    Add(item);
            }
            finally
            {
                FinishCoping();
            }
        }

        public bool SaveEmptyList { get; set; } = true;

        public override void Save(XmlElement? parentElement, bool clearModified = true)
        {
            if (!SaveEmptyList 
                && Count == 0)
                return;

            base.Save(parentElement, clearModified);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            Sort();

            foreach (T item in List)
            {
                if (item.IsEmpty)
                    continue;

                item.State = State;
                item.Save(element, clearModified);
            }
        }

        public virtual void Sort()
        {
            if (AutoSorting)
                List.Sort(new DAOComparer());
        }

        private string? StringConverter(T t) => t.ToString();
        private object ObjectConverter(T t) => t;

        public List<string?> StringList =>
            List.ConvertAll(new Converter<T, string?>(StringConverter));

        protected override void LoadData(XmlElement element)
        {
            T item;

            foreach (XmlNode node in element.ChildNodes)
            {
                item = Add(new T
                    {
                        SilentChange = SilentChange
                    }
                );

                if (item.WithoutXmlNode || item.XmlElementName == node.Name)
                {
                    item.State = State;
                    item.Load((XmlElement)node);
                }
            }

            Sort();
        }

        public override string DefaultXmlElementName
        {
            get
            {
                string typeName = $"{typeof(T).Name}s";

                if (typeName.Contains('`'))
                    typeName = typeName[..typeName.IndexOf('`')];

                return typeName;
            }
        }

        public void NotifyAboutItemAdded(T item)
        {
            if (!SilentChange)
                CallItemAddHandler(item, new DAOEntityEventArgs(DAOOperation.Add));
        }

        protected virtual void CallItemAddHandler(T item, DAOEntityEventArgs args) =>
            ItemAddHandler?.Invoke(item, args);

        public T Add() => Add(new T());

        public virtual T Add(T item)
        {
            List.Add(item);
            item.SilentChange = SilentChange;
            NotifyAboutItemAdded(item);
            AddMember(item);
            SetMemberHandlers(item);
            Modified = true;
            return item;
        }

        protected void ItemsToMembers()
        {
            foreach (DAO item in List)
                if (!Members.Contains(item))
                {
                    AddMember(item);
                    SetMemberHandlers(item, true);
                }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            List.AddRange(collection);
            ItemsToMembers();
        }

        public void Insert(int index, T item) => List.Insert(index, item);

        public void RemoveAll(Predicate<T> match) => RemoveAll(match, true);

        public void RemoveAll(Predicate<T> match, bool needSaveHistory)
        {
            List<T> removingList = FindAll(match);

            foreach (T item in removingList)
                Remove(item, needSaveHistory);
        }

        public bool Remove(T item) => Remove(item, true);

        public bool Remove(T item, bool needSaveHistory)
        {
            bool needDisabledSilentChange = !SilentChange;

            if (!SilentChange)
                SilentChange = true;

            try
            {
                IDAO? oldValue = needSaveHistory ? new ListDAO<T>().CopyFrom(this) : null;

                if (List.Remove(item))
                {
                    ModifiedChangeHandler?.Invoke(this, new DAOModifyEventArgs(true, oldValue));
                    RemoveMember(item);
                    CallItemRemoveHandler(item, new DAOEntityEventArgs(DAOOperation.Remove));
                    Modified = true;
                    return true;
                }
                else
                    return false;
            }
            finally
            {
                if (needDisabledSilentChange)
                    SilentChange = false;
            }
        }

        protected virtual void CallItemRemoveHandler(T item, DAOEntityEventArgs args) =>
            ItemRemoveHandler?.Invoke(item, args);

        public bool Remove(Predicate<T> match) => Remove(match, true);

        public bool Remove(Predicate<T> match, bool needSaveHistory)
        {
            T? item = Find(match);
            return item != null && Remove(item, needSaveHistory);
        }

        public int IndexOf(T? value) => value == null ? -1 : List.IndexOf(value);

        public T? Find(Predicate<T> match) => List.Find(match);
        public List<T> FindAll(Predicate<T> match) => List.FindAll(match);

        public bool Contains(Predicate<T> match) => List.Find(match) != null;

        public bool Contains(T item) => List.Contains(item);

        public override bool Equals(object? obj)
        {
            if (obj is not ListDAO<T>)
                return false;

            ListDAO<T> other = (ListDAO<T>)obj;

            if (other.Count != Count)
                return false;

            if (AutoSorting)
            {
                foreach (T item in List)
                    if (!other.Contains(i => item.Equals(i)))
                        return false;
            }
            else
                for (int i = 0; i < List.Count; i++)
                    if (!List[i].Equals(other[i]))
                        return false;

            return true;
        }

        public override int GetHashCode() =>
            EqualityComparer<List<T>>.Default.GetHashCode(List);


        public T this[int index]
        {
            get => List[index];
            set => List[index] = value;
        }

        public int Count => List.Count;
        public override bool IsEmpty => List.Count == 0;
        public event DAOEntityEventHandler? ItemAddHandler;
        public event DAOEntityEventHandler? ItemRemoveHandler;

        public int ModifiedCount => List.FindAll(d => d.Modified).Count;

        public override string ToString() =>
            string.Join(", ", List);

        public override string ShortString
        {
            get
            {
                string result = string.Empty;

                foreach (T item in this)
                {
                    if (result != string.Empty)
                        result += ", ";

                    result += item.ShortString;
                }

                return result;
            }
        }

        public override string? MatchingString()
        {
            string? result = string.Empty;

            foreach (T item in this)
                result += $"$$_$${item}$$_$$";

            return result;
        }

        public IEnumerator<T> GetEnumerator() =>
            List.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            List.GetEnumerator();

        protected virtual bool AutoSorting => true;

        public string OneColumnText()
        {
            string result = string.Empty;

            foreach (T item in List)
                result += $"{item}\r\n";

            return result;

        }

        public T? First => Count > 0 ? List[0] : null;

        public List<object> ObjectList
        {
            get => List.ConvertAll(ObjectConverter);
            set 
            {
                Clear();

                if (value != null)
                    foreach (object item in value)
                        Add((T)item);
            }
        }

        public TListDAO Distinct<TListDAO>(Func<T, TListDAO, bool> CheckUnique)
            where TListDAO : IListDAO<T>, new()
        {
            //TODO: replce TListDAO with RootListDAO - need for get UseImageList flag
            TListDAO list = new();

            foreach (T dao in this)
                if (CheckUnique(dao, list))
                    list.Add(dao);

            return list;
        }

        public IListDAO<T> Distinct<ListDAO>(Func<T, IListDAO<T>, bool> CheckUnique) => 
            Distinct<ListDAO>(CheckUnique);
    }
}