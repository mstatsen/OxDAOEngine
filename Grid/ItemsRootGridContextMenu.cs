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
            Grid.CurrentItemChanged += RenewItemAccordingItems;
            Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.New));
            Items.Add(new ToolStripSeparator());
            Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.Edit));

            if (DataManager.ListController<TField, TDAO>().AvailableCopyItems)
                Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.Copy));

            Items.Add(new ToolStripSeparator());
            Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.Delete));
            Items.Add(new ToolStripSeparator());
            Items.Add(new ItemsRootGridActionToolStripMenuItem(OxToolbarAction.ExportSelected));
            Items.Add(CurrentItemSeparator);

            ItemClicked += ItemClickedHandler;
        }

        private readonly ToolStripSeparator CurrentItemSeparator = new()
        { 
            Visible = false
        };

        private void RenewItemAccordingItems(object? sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in ItemAccordingItems)
                Items.Remove(item);

            List<ToolStripMenuItem>? CurrentItemMenuItems = DataManager.ListController<TField, TDAO>().MenuItems(Grid.CurrentItem);

            if (CurrentItemMenuItems != null
                && CurrentItemMenuItems.Count > 0)
            {
                CurrentItemSeparator.Visible = true;

                foreach (ToolStripMenuItem item in CurrentItemMenuItems)
                {
                    ItemAccordingItems.Add(item);
                    Items.Add(item);
                }
            }
            else 
                CurrentItemSeparator.Visible = false;
        }

        private readonly List<ToolStripMenuItem> ItemAccordingItems = new();

        private void ItemClickedHandler(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ItemsRootGridActionToolStripMenuItem actionItem)
                Grid.ExecuteAction(actionItem.Action);
        }
    }
}