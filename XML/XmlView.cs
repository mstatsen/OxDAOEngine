using OxLibrary.Dialogs;

namespace OxXMLEngine.XML
{
    public static class XmlView
    {
        public static void ShowXML(string XML)
        {
            OxDialog form = new()
            {
                DialogButtons = OxDialogButton.OK,
                MinimumSize = new Size(800, 600),
                Text = "XML Viewer"
            };
            _ = new RichTextBox
            {
                Parent = form,
                Dock = DockStyle.Fill,
                Multiline = true,
                AcceptsTab = true,
                BorderStyle = BorderStyle.None,
                Text = XML
            };

            form.ShowDialog();
        }
    }
}
