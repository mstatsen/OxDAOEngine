using OxLibrary.Controls;

namespace OxXMLEngine.Grid
{
    public class ItemsRootGridActionToolStripMenuItem : ToolStripMenuItem
    {
        public readonly OxToolbarAction Action;
        public ItemsRootGridActionToolStripMenuItem(OxToolbarAction action)
        {
            Action = action;
            Text = OxToolbarActionHelper.Text(action);
            Image = OxToolbarActionHelper.Icon(action);
        }
    }
}