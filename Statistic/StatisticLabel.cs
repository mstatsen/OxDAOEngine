using OxLibrary.Controls;
using OxLibrary.Panels;

namespace OxXMLEngine.Statistic
{
    public class StatisticLabel : OxFrame
    {
        public string StatisticText
        {
            get => Label.Text;
            set
            {
                Label.Text = value;
                SetLabelLocation();
            }
        }

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            Label.Parent = ContentContainer;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SetLabelLocation();
        }

        private void SetLabelLocation()
        {
            if (Width < Label.Width)
                SetContentSize(Label.Width, SavedHeight);

            Label.Top = (ContentContainer.Height - Label.Height) / 2;
            Label.Left = (ContentContainer.Width - Label.Width) / 2;
        }

        private readonly OxLabel Label = new()
        {
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = EngineStyles.DefaultFont
        };
    }
}