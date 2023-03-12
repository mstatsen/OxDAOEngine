namespace OxXMLEngine.Data
{
    public enum DAOOperation
    {
        Insert,
        Modify,
        Delete
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

    public delegate void DAOEntityEventHandler(DAO dao, DAOEntityEventArgs e);
    public delegate void ModifiedChangeHandler(DAO dao, bool Modified);
}