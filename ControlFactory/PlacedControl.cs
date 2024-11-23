using OxLibrary;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory
{
    public class PlacedControl<TField>
        where TField : notnull, Enum
    {
        public Control Control { get; internal set; }
        public OxLabel? Label { get; internal set; }
        public ControlLayout<TField> Layout { get; internal set; }

        public bool Visible
        {
            get => Control.Visible;
            set
            { 
                Control.Visible = value;

                if (Label is not null)
                    Label.Visible = value;
            }
        }

        public void RecalcLabel() =>
            Layout.RecalcLabel(this);

        public int LabelLeft
        {
            get => 
                Label is not null 
                    ? OxWh.Int(Label.Left)
                    : int.MaxValue;
            set
            {
                if (Label is not null)
                    Label.Left = OxWh.W(value);
            }
        }

        public int LabelRight =>
            Label is not null 
                ? OxWh.Int(Label.Right) 
                : 0;

        public PlacedControl(Control control, OxLabel? label, ControlLayout<TField> layout)
        {
            Control = control;
            Label = label;
            Layout = layout;
            SetHandlers();
        }

        public void DetachParent()
        {
            if (Control.Parent is not null)
            {
                Control parent = Control.Parent;
                parent.Controls.Remove(Control);

                if (Label is not null)
                    parent.Controls.Remove(Label);
            }

            Control.Parent = null;

            if (Label is not null)
                Label.Parent = null;
        }

        private void SetHandlers()
        {
            if ((Control is OxCheckBox))
                return;

            SetMeasureHandlers(Control);

            if (Label is not null)
                SetMeasureHandlers(Label);
        }

        private void SetMeasureHandlers(Control control)
        {
            control.LocationChanged += (s, e) => AlignLabel();
            control.SizeChanged += (s, e) => AlignLabel();
        }

        private void AlignLabel() => 
            OxControlHelper.AlignByBaseLine(Control, Label);
    }
}