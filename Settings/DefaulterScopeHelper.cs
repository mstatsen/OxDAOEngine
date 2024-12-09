using OxDAOEngine.Data.Types;
using OxLibrary;

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

        public short Width(DefaulterScope scope) => 
            scope switch
            {
                DefaulterScope.CurrentPage => 160,
                _ => 86,
            };

        public override DefaulterScope EmptyValue() =>
            DefaulterScope.All;

        public readonly short DefaultButtonHeight = 23;
        public readonly short DefaultButtonsSpace = 4;
    }
}