using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Geometry;
using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory;

public class PlacedControl<TField>
    where TField : notnull, Enum
{
    public IOxControl Control { get; internal set; }
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

    public short LabelLeft
    {
        get => 
            OxSH.IfElse(
                Label is not null,
                Label!.Left,
                short.MaxValue
            );
        set
        {
            if (Label is not null)
                Label.Left = value;
        }
    }

    public short LabelRight =>
        OxSH.IfElseZero(Label is not null, Label!.Right);

    public PlacedControl(IOxControl control, OxLabel? label, ControlLayout<TField> layout)
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
            IOxBox parent = Control.Parent;
            parent.Controls.Remove((Control)Control);

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

    private void SetMeasureHandlers(IOxControl control)
    {
        control.LocationChanged += (s, e) => AlignLabel();
        control.SizeChanged += (s, e) => AlignLabel();
    }

    private void AlignLabel() => 
        OxControlHelper.AlignByBaseLine(Control, Label);
}