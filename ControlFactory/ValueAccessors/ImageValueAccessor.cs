using OxLibrary;
using OxLibrary.BitmapWorker;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class ImageValueAccessor : ValueAccessor
    {
        private OxPicture Picture =>
            (OxPicture)Control;

        public override object? GetValue() =>
            Picture.Image;

        public override void SetValue(object? value)
        {
            if (value is int intValue)
                value = OxBitmapWorker.BoxingImage(
                    intValue > 0 
                        ? OxIcons.Tick 
                        : OxIcons.Cross, 
                    new OxSize(16, 16));

            if (value is bool boolValue)
                value = OxBitmapWorker.BoxingImage(
                    boolValue 
                        ? OxIcons.Tick 
                        : OxIcons.Cross, 
                    new OxSize(16, 16));

            Picture.Image = (Image?)value;
        }
    }
}