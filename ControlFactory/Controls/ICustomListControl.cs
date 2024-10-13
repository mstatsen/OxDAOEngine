using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
{
    public interface ICustomListControl<TItem, TItems>
        where TItem : DAO, new()
        where TItems : ListDAO<TItem>, new()
    {
        TItems? FixedItems { get; set; }
    }
}