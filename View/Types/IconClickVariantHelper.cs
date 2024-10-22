using OxDAOEngine.Data.Types;

namespace OxDAOEngine.View.Types
{
    public class IconClickVariantHelper : AbstractTypeHelper<IconClickVariant>
    {
        public override IconClickVariant EmptyValue() => IconClickVariant.Nothing;
        public override IconClickVariant DefaultValue() => IconClickVariant.ShowCard;

        public override string GetName(IconClickVariant value) =>
            value switch
            {
                IconClickVariant.ShowKey => "Show item key",
                IconClickVariant.ShowCard => "Show item card",
                IconClickVariant.ShowEditor => "Edit item",
                _ => "Do nothing",
            };

        public override string GetShortName(IconClickVariant value) =>
            value switch
            {
                IconClickVariant.ShowKey => "Key",
                IconClickVariant.ShowCard => "Card",
                IconClickVariant.ShowEditor => "Editor",
                _ => "Nothing",
            };
    }
}