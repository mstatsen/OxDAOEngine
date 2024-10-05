using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Initializers
{
    public class CheckBoxInitializer : EmptyControlInitializer
    {
        private readonly string Caption;

        public CheckBoxInitializer(string caption) => 
            Caption = caption;

        public override void InitControl(Control control) => 
            ((OxCheckBox)control).Text = Caption;
    }
}
