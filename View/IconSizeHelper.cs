using OxLibrary;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.View
{
    public class IconSizeHelper : AbstractTypeHelper<IconSize>
    {
        public override IconSize EmptyValue() => IconSize.Large;

        public override string GetName(IconSize value) => 
            value switch
            {
                IconSize.Small => "Small",
                IconSize.Medium => "Medium",
                IconSize.Large => "Large",
                _ => string.Empty,
            };

        public static int Width(IconSize size) =>
            size switch
            {
                IconSize.Small => 125,
                IconSize.Medium => 162,
                IconSize.Large => 200,
                _ => 0,
            };

        public static int Height(IconSize size) =>
            Width(size) / 25 * 18;

        public static int LeftDelta(IconSize size) => 
            size switch
            {
                IconSize.Small => 6,
                IconSize.Medium => 4,
                _ => 0,
            };

        public static int AddInfoWidth(IconSize size) =>
            size switch
            {
                IconSize.Small => 36,
                IconSize.Medium => 40,
                IconSize.Large => 44,
                _ => 0,
            };

        public static float FontSize(IconSize size) =>
            size switch
            {
                IconSize.Small => Styles.DefaultFontSize - 3,
                IconSize.Medium => Styles.DefaultFontSize - 2,
                _ => Styles.DefaultFontSize,
            };

        public static int FontSizeDelta(IconSize size) => 
            size switch
            {
                IconSize.Small or IconSize.Medium => 1,
                _ => 2,
            };
    }
}