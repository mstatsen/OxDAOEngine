namespace OxDAOEngine.Data.History
{
    public class AddedDAO<TField, TDAO> : ItemHistory<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public AddedDAO() : base() { }
        public AddedDAO(TDAO dao) : base(dao) { }
        public override DAOOperation Operation => DAOOperation.Add;
        public override object? NewValue { get => DAO; }
        public override object? OldValue { get => null; }
    }
}