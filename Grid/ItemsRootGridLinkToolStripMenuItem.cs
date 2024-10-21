using OxDAOEngine.Data.Links;
using OxLibrary;
using System.Diagnostics;

namespace OxDAOEngine.Grid
{
    public class ItemsRootGridLinkToolStripMenuItem<TField> : ToolStripMenuItem
        where TField : notnull, Enum
    {
        public readonly Link<TField> Link;
        public ItemsRootGridLinkToolStripMenuItem(Link<TField> link)
        {
            Link = link;
            Text = link.Name;
            Image = OxIcons.Go;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = Link.Url,
                    UseShellExecute = true
                }
            );
        }
    }
}