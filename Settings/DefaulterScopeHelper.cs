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

        public OxWidth Width(DefaulterScope scope) => 
            scope switch
            {
                DefaulterScope.CurrentPage => OxWh.W160,
                _ => OxWh.W86,
            };

        public override DefaulterScope EmptyValue() =>
            DefaulterScope.All;

        public readonly OxWidth DefaultButtonHeight = OxWh.W23;
        public readonly OxWidth DefaultButtonsSpace = OxWh.W4;
    }
}