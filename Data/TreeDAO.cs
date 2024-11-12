namespace OxDAOEngine.Data
{
    public class TreeDAO<TDAO> : ListDAO<TDAO>
        where TDAO : TreeItemDAO<TDAO>, new()
    {
        public TreeDAO() : base() { }
    }
}