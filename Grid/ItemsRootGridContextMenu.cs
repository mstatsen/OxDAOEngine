using OxLibrary.Controls;
using OxDAOEngine.Data;

namespace OxDAOEngine.Grid
{
    public class ItemsRootGridContextMenu<TField, TDAO> : ContextMenuStrip
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public readonly ItemsRootGrid<TField, TDAO> Grid;
        public ItemsRootGridContextMenu(ItemsRootGrid<TField, TDAO> grid)
        {
            Grid = grid;
            Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.New));
            Items.Add(new ToolStripSeparator());
            Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.Edit));

            if (DataManager.ListController<TField,TDAO>().AvailableCopyItems)
                Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.Copy));

            Items.Add(new ToolStripSeparator());
            Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.Delete));
            Items.Add(new ToolStripSeparator());
            Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.ExportSelected));
            ItemClicked += ItemClickedHandler;
        }

        private void ItemClickedHandler(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ItemsRootGridActionToolStripMenuItem actionItem)
                Grid.ExecuteAction(actionItem.Action);
        }
    }
}