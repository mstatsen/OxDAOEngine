﻿namespace OxDAOEngine.Data.History
{
    public class ItemHistoryList<TField, TDAO> : RootListDAO<TField, ItemHistory<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public void ChangeField(TDAO dao, TField field, object? oldValue) => 
            Add(new FieldHistory<TField, TDAO>(dao, field, oldValue));

        public void AddDAO(TDAO dao) =>
            Add(new AddedDAO<TField, TDAO>(dao));

        public void RemoveDAO(TDAO dao) =>
            Add(new RemovedDAO<TField, TDAO>(dao));

        private ItemHistoryList<TField, TDAO> ByOperation(DAOOperation operation)
        {
            ItemHistoryList<TField, TDAO> result = new();
            result.AddRange(FindAll((h) => h.Operation.Equals(operation)));
            return result;
        }

        public ItemHistoryList<TField, TDAO> AddedList => ByOperation(DAOOperation.Add);

        public ItemHistoryList<TField, TDAO> RemovedList =>
            ByOperation(DAOOperation.Remove);

        public ItemHistoryList<TField, TDAO> ModifiedList =>
            ByOperation(DAOOperation.Modify);

        public ListDAO<TDAO> DistinctModifiedDAOList
        {
            get
            {
                ListDAO<TDAO> result = new();

                foreach (ItemHistory<TField, TDAO> history in this)
                    if (history.DAO is not null 
                        && !result.Contains(history.DAO) 
                        && history.Operation is DAOOperation.Modify)
                        result.Add(history.DAO);

                return result;
            }
        }
            

        public int AddedCount => AddedList.Count;
        public int RemovedCount => RemovedList.Count;
        public int DistinctModifiedDAOCount => DistinctModifiedDAOList.Count;
    }
}
