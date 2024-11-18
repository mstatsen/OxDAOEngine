using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class PictureContainerValueAccessor : ValueAccessor
    {
        private OxPictureContainer PictureContainer =>
            (OxPictureContainer)Control;

        public override object? GetValue() =>
            PictureContainer.Image;

        public override void SetValue(object? value) =>
            PictureContainer.Image = 
                value is Image imageValue 
                    ? imageValue 
                    : null;
    }
}