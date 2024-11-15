using OxDAOEngine.Data;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Controls
{
    public abstract class TreeItemsControl<TList, TItem, TEditor, TField, TDAO> 
        : CustomItemsContainerControl<TList, TItem, OxTreeView, TEditor, TField, TDAO>
        where TList : ListDAO<TItem>, new ()
        where TItem : TreeItemDAO<TItem>, new ()
        where TEditor : CustomItemEditor<TItem, TField, TDAO>, new ()
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new ()
    {
        private OxTreeView TreeView => (OxTreeView)ItemsContainer;

        protected override void PrepareItemsContainer() => 
            TreeItemsControlHelper.PrepareTreeView(TreeView);

        protected override void AddValueToItemsContainer(TItem valuePart) =>
            TreeItemsControlHelper.AddValueNode(TreeView, valuePart);

        protected override void RemoveCurrentItemFromContainer() =>
            TreeItemsControlHelper.RemoveCurrentNode<TItem>(TreeView);
    }
}