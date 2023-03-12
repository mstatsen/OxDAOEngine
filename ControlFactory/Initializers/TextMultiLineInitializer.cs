using System.Windows.Forms;

namespace OxXMLEngine.ControlFactory.Initializers
{
    public class TextMultiLineInitializer : EmptyControlInitializer
    {
        public TextMultiLineInitializer(bool withBorder = false) : base() => 
            WithBorder = withBorder;

        private readonly bool WithBorder;

        public override void InitControl(Control control)
        {
            TextBox textBox = (TextBox)control;
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.BorderStyle = WithBorder ? BorderStyle.FixedSingle : BorderStyle.None;
        }
    }
}
