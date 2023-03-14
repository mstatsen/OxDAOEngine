using OxLibrary.Controls;

namespace OxXMLEngine.ControlFactory.Initializers
{
    public class NumericInitializer : EmptyControlInitializer
    {
        public NumericInitializer(int minimum, int maximum, int step = 1) : base()
        {
            Minimum = minimum;
            Maximum = maximum;
            Step = step;
        }

        public override void InitControl(Control control)
        {
            OxSpinEdit spinControl = (OxSpinEdit)control;
            spinControl.Minimum = Minimum;
            spinControl.Maximum = Maximum;
            spinControl.Step = Step;
        }

        private readonly int Minimum;
        private readonly int Maximum;
        private readonly int Step;
    }
}