namespace OxDAOEngine.Data
{
    public interface ITreeDAO<TDAO> : IListDAO<TDAO>
        where TDAO : DAO, new()
    { 
    }

    public class TreeDAO<TDAO> : ListDAO<TDAO>
        where TDAO : TreeItemDAO<TDAO>, new()
    {
        public TreeDAO() : base() { }
    }
}