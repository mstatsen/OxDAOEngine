using OxDAOEngine.Data.Types;
using OxLibrary;

namespace OxDAOEngine.View.Types
{
    public class IconSizeHelper : AbstractTypeHelper<IconSize>
    {
        public override IconSize EmptyValue() => IconSize.Large;

        public override string GetName(IconSize value) =>
            value switch
            {
                IconSize.Thumbnails => "Thumbnails",
                IconSize.Small => "Small",
                IconSize.Medium => "Medium",
                IconSize.Large => "Large",
                _ => string.Empty,
            };

        public short Width(IconSize size) =>
            size switch
            {
                IconSize.Thumbnails => 110,
                IconSize.Small => 125,
                IconSize.Medium => 162,
                IconSize.Large => 200,
                _ => 0,
            };

        public short Height(IconSize size) =>
            (short)(size.Equals(IconSize.Thumbnails)
                ? Width(size) / 2 - 3
                : Width(size) / 25 * 18);

        public short LeftDelta(IconSize size) =>
            size switch
            {
                IconSize.Thumbnails or
                IconSize.Small =>
                    6,
                IconSize.Medium =>
                    4,
                _ =>
                    0,
            };

        public short AddInfoWidth(IconSize size) =>
            size switch
            {
                IconSize.Small => 36,
                IconSize.Medium => 40,
                IconSize.Large => 44,
                _ => 0,
            };

        public float FontSize(IconSize size) =>
            size switch
            {
                IconSize.Small => OxStyles.DefaultFontSize - 3,
                IconSize.Medium => OxStyles.DefaultFontSize - 2,
                _ => OxStyles.DefaultFontSize,
            };

        public int FontSizeDelta(IconSize size) =>
            size switch
            {
                IconSize.Small or 
                IconSize.Medium => 
                    1,
                _ => 
                    2,
            };
    }
}