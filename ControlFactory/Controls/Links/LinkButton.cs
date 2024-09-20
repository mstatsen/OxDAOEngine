using OxLibrary.Controls;
using OxLibrary.Dialogs;
using System.Diagnostics;

namespace OxDAOEngine.ControlFactory.Controls
{
    public class LinkButton : OxButton
    {
        public string Url { get; set; }

        public LinkButton(string text, string url) : base(text, null) =>
            Url = url;

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Cursor = Cursors.Hand;
            SetContentSize(SavedWidth, 22);
        }

        protected override void SetHandlers()
        {
            base.SetHandlers();
            Click += (s, e) =>
            {
                try
                {
                    Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = Url,
                            UseShellExecute = true
                        }
                    );
                }
                catch 
                {
                    OxMessage.ShowError("Unable to follw this link. Check the Url please.");
                }
            };
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();
            ForeColor = Colors.Darker(3);
        }
    }
}
