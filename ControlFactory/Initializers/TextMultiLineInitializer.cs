namespace OxDAOEngine.ControlFactory.Initializers
{
    public class TextMultiLineInitializer : EmptyControlInitializer
    {
        public TextMultiLineInitializer(bool withBorder = false, bool withScrollBar = true) : base()
        {
            WithBorder = withBorder;
            WithScrollBar = withScrollBar;
        }

        private readonly bool WithBorder;
        private readonly bool WithScrollBar;

        public override void InitControl(Control control)
        {
            TextBox textBox = (TextBox)control;
            textBox.Multiline = true;
            textBox.ScrollBars = WithScrollBar ? ScrollBars.Vertical : ScrollBars.None;
            textBox.BorderStyle = WithBorder ? BorderStyle.FixedSingle : BorderStyle.None;
        }
    }
}
