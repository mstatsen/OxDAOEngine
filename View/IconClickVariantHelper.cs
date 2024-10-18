using OxDAOEngine.Data.Types;

namespace OxDAOEngine.View
{
    public class IconClickVariantHelper : AbstractTypeHelper<IconClickVariant>
    {
        public override IconClickVariant EmptyValue() => IconClickVariant.Nothing;
        public override IconClickVariant DefaultValue() => IconClickVariant.ShowCard;

        public override string GetName(IconClickVariant value) => 
            value switch
            {
                IconClickVariant.ShowCard => "Show item card",
                IconClickVariant.ShowEditor => "Edit item",
                IconClickVariant.Custom => "Custom",
                _ => "Do nothing",
            };

        public override string GetShortName(IconClickVariant value) => 
            value switch
            {
                IconClickVariant.ShowCard => "Card",
                IconClickVariant.ShowEditor => "Editor",
                IconClickVariant.Custom => "Custom",
                _ => "Nothing",
            };
    }
}