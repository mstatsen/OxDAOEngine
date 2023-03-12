﻿using OxLibrary.Controls;

namespace OxXMLEngine.ControlFactory.ValueAccessors
{
    public class ImageValueAccessor : ValueAccessor
    {
        private OxPicture Picture =>
            (OxPicture)Control;

        public override object? GetValue() =>
            Picture.Image;

        public override void SetValue(object? value) =>
            Picture.Image = (Image?)value;
    }
}