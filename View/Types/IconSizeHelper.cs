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

        public OxWidth Width(IconSize size) =>
            size switch
            {
                IconSize.Thumbnails => OxWh.W110,
                IconSize.Small => OxWh.W125,
                IconSize.Medium => OxWh.W162,
                IconSize.Large => OxWh.W200,
                _ => 0,
            };

        public OxWidth Height(IconSize size) =>
            size.Equals(IconSize.Thumbnails)
                ? OxWh.Sub(OxWh.Div(Width(size), 2), OxWh.W3)
                : OxWh.Mul(OxWh.Div(Width(size), 25), OxWh.W18);

        public OxWidth LeftDelta(IconSize size) =>
            size switch
            {
                IconSize.Thumbnails or
                IconSize.Small =>
                    OxWh.W6,
                IconSize.Medium =>
                    OxWh.W4,
                _ =>
                    OxWh.W0,
            };

        public OxWidth AddInfoWidth(IconSize size) =>
            size switch
            {
                IconSize.Small => OxWh.W36,
                IconSize.Medium => OxWh.W40,
                IconSize.Large => OxWh.W44,
                _ => OxWh.W0,
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