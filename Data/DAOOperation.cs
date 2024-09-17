namespace OxDAOEngine.Data
{
    public enum DAOOperation
    {
        Add,
        Modify,
        Remove
    };

    public delegate RootListDAO<TField, TDAO> GetListEvent<TField, TDAO>()
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new();

    public class DAOEntityEventArgs
    {
        public readonly DAOOperation Operation;

        public DAOEntityEventArgs(DAOOperation operation) =>
            Operation = operation;
    }

    public class DAOModifyEventArgs
    {
        public bool Modified { get; }
        public object? OldValue { get; }
        public DAOModifyEventArgs(bool modified, object? oldValue)
        {
            Modified = modified;
            OldValue = oldValue;
        }
    }

    public delegate void DAOEntityEventHandler(DAO dao, DAOEntityEventArgs e);
    public delegate void ModifiedChangeHandler(DAO dao, DAOModifyEventArgs e);
}