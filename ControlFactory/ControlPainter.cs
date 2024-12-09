﻿using OxDAOEngine.ControlFactory.Controls;
using OxLibrary.Controls;
using OxLibrary;
using OxDAOEngine.ControlFactory.Accessors;
using OxLibrary.Panels;
using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory
{
    public static class ControlPainter
    {
        public static void ColorizeControlOx(IOxControl control, Color baseColor) =>
            ColorizeControl((Control)control, baseColor);

        public static void ColorizeControl(IControlAccessor accessor, Color baseColor) =>
            ColorizeControl(accessor.Control, baseColor);

        public static void ColorizeControl(Control control, Color baseColor)
        {
            switch (control)
            {
                case IButtonEdit buttonEdit:
                    buttonEdit.ControlColor = new OxColorHelper(baseColor).Lighter(control.Enabled ? 7 : 8);
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
                    control.BackColor = new OxColorHelper(baseColor).Lighter(control.Enabled ? 7 : 8);
                    break;
            }
        }
    }
}