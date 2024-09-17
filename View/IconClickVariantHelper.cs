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
                /*
    case IconClickVariant.GoToStratege:
    return "Go to Stratege.ru";
    case IconClickVariant.GoToPSNProfiles:
    return "Go to PSNProfiles.com";
    case IconClickVariant.GoToFirstWalk:
    return "Go to walktrough";
    */
                _ => "Do nothing",
            };

        public override string GetShortName(IconClickVariant value) => 
            value switch
            {
                IconClickVariant.ShowCard => "Card",
                IconClickVariant.ShowEditor => "Editor",
                IconClickVariant.Custom => "Custom",
                /*
    case IconClickVariant.GoToStratege:
    return "Stratege";
    case IconClickVariant.GoToPSNProfiles:
    return "PSNProfiles";
    case IconClickVariant.GoToFirstWalk:
    return "Walktrough";
    */
                _ => "Nothing",
            };
    }
}
