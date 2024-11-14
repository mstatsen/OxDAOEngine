using OxDAOEngine.Data;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Controls
{
    public static class TreeItemsControlHelper
    {
        public static void PrepareTreeView(OxTreeView treeView)
        {
            treeView.AllowCollapse = false;
            treeView.DoubleClickExpand = false;
        }

        public static void AddValueNode<TItem>(OxTreeView treeView, TItem item)
            where TItem : TreeItemDAO<TItem>, new()
        {
            if (item.Parent != null)
            {
                item.Parent.AddChild(item);
                treeView.SelectedItem = item.Parent;
                treeView.AddChild(item);
                treeView.SelectedItem = item;
            }
            else
                treeView.Add(item);

            foreach (ITreeItemDAO<TItem> child in item.Childs)
            {
                AddValueNode(treeView, (TItem)child);
                treeView.SelectedItem = item;
            }

            treeView.ExpandAll();
        }
    }
}
