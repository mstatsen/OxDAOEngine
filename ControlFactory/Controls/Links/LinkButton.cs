using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Dialogs;
using System.Diagnostics;

namespace OxDAOEngine.ControlFactory.Controls.Links
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
            Size = new(Width, OxWh.W22);
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
                    OxMessage.ShowError("Unable to follw this link. Check the Url please.", this);
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
