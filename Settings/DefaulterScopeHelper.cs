using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Settings
{
    public class DefaulterScopeHelper : AbstractTypeHelper<DefaulterScope>
    {
        public override string GetName(DefaulterScope scope) => 
            scope switch
            {
                DefaulterScope.CurrentPage => "Reset current page",
                DefaulterScope.All => "Reset all",
                _ => "Reset to defaults",
            };

        public static int Width(DefaulterScope scope) => 
            scope switch
            {
                DefaulterScope.CurrentPage => 140,
                _ => 80,
            };

        public override DefaulterScope EmptyValue() =>
            DefaulterScope.All;

        public const int DefaultButtonHeight = 23;
        public const int DefaultButtonsSpace = 4;
    }
}