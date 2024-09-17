using System.Windows.Forms;

namespace OxDAOEngine.ControlFactory.Initializers
{
    public class CheckBoxInitializer : EmptyControlInitializer
    {
        private readonly string Caption;

        public CheckBoxInitializer(string caption) => 
            Caption = caption;

        public override void InitControl(Control control) => 
            ((CheckBox)control).Text = Caption;
    }
}
