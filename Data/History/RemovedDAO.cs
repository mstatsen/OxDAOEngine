namespace OxDAOEngine.Data.History
{
    public class RemovedDAO<TField, TDAO> : ItemHistory<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public RemovedDAO() : base() { }
        public RemovedDAO(TDAO dao) : base(dao) { }
        public override DAOOperation Operation => DAOOperation.Remove;
        public override object? NewValue { get => null; }
        public override object? OldValue { get => DAO; }
    }
}