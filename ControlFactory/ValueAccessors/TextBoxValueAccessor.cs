﻿using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class TextBoxValueAccessor : ValueAccessor
    {
        private OxTextBox TextBox => 
            (OxTextBox)Control;

        public override object? GetValue() => 
            TextBox.Text;

        public override void SetValue(object? value) =>
            TextBox.Text = 
                value is not null
                    ? value.ToString()
                    : string.Empty;
    }
}