using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Interfaces;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.ControlFactory.Accessors;

namespace OxDAOEngine.ControlFactory;

public static class ControlPainter
{
    public static void ColorizeControl(IControlAccessor accessor, Color baseColor) =>
        ColorizeControl(accessor.Control, baseColor);

    public static void ColorizeControl(IOxControl control, Color baseColor)
    {
        switch (control)
        {
            case IButtonEdit buttonEdit:
                buttonEdit.ControlColor = new OxColorHelper(baseColor).Lighter(control.IsEnabled ? 7 : 8);
                break;
            case OxPanel spinEdit:
                spinEdit.BaseColor = baseColor;
                break;
            case OxCheckBox checkBox:
                checkBox.BackColor = Color.Transparent;
                break;
            case IColoredCustomControl customControl:
                customControl.ControlColor = baseColor;
                break;
            default:
                control.BackColor = new OxColorHelper(baseColor).Lighter(control.IsEnabled ? 7 : 8);
                break;
        }
    }
}