using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;

namespace OxDAOEngine.ControlFactory
{
    public static class TextViewer
    {
        public static void Show(string caption, string text, Color baseColor)
        {
            OxDialog form = new()
            {
                DialogButtons = OxDialogButton.OK,
                MinimumSize = new(OxWh.W800, OxWh.W600),
                Text = caption,
                BaseColor = baseColor
            };
            _ = new OxTextBox
            {
                Parent = form,
                Dock = DockStyle.Fill,
                Multiline = true,
                AcceptsTab = true,
                WordWrap = true,
                BorderStyle = BorderStyle.None,
                Text = text.Replace("\n", "\r\n"),
                ReadOnly = true,
                BackColor = new OxColorHelper(baseColor).Lighter(7),
                ScrollBars = ScrollBars.Vertical,
                Font = OxStyles.DefaultFont
            };
            form.Shown += ViewerShown;
            form.ShowDialog();
        }

        private static void ViewerShown(object? sender, EventArgs e)
        {
            if (sender is OxForm form)
                form.MoveToScreenCenter();
        }
    }
}
