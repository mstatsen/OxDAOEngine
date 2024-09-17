using OxDAOEngine.ControlFactory.Controls;
using OxLibrary.Controls;
using OxLibrary;

namespace OxDAOEngine.ControlFactory
{
    public static class ControlPainter
    {
        public static void ColorizeControl(Control control, Color baseColor)
        {
            switch (control)
            {
                case IButtonEdit buttonEdit:
                    buttonEdit.ControlColor = new OxColorHelper(baseColor).Lighter(control.Enabled ? 7 : 8);
                    break;
                case OxSpinEdit spinEdit:
                    spinEdit.BaseColor = baseColor;
                    break;
                case OxCheckBox checkBox:
                    checkBox.BackColor = Color.Transparent;
                    break;
                case IColoredCustomControl customControl:
                    customControl.ControlColor = baseColor;
                    break;
                case OxPictureContainer pictureContainer:
                    pictureContainer.BaseColor = baseColor;
                    break;
                case OxButton button:
                    button.BaseColor = baseColor;
                    break;
                default:
                    control.BackColor = new OxColorHelper(baseColor).Lighter(control.Enabled ? 7 : 8);
                    break;
            }
        }
    }
}
