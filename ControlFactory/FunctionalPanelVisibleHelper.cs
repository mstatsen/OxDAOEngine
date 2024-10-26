using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory
{
    public class FunctionalPanelVisibleHelper : AbstractTypeHelper<FunctionalPanelVisible>
    {
        public override FunctionalPanelVisible EmptyValue() => 
            FunctionalPanelVisible.Float;

        public override string GetName(FunctionalPanelVisible value) => 
            value switch
            {
                FunctionalPanelVisible.Hidden => "Hidden",
                FunctionalPanelVisible.Fixed => "Show as fixed panel",
                _ => "Show as float panel"
            };

        public override string GetShortName(FunctionalPanelVisible value)
        {
            return value switch
            {
                FunctionalPanelVisible.Hidden => "Hidden",
                FunctionalPanelVisible.Fixed => "Fixed",
                _ => "Float",
            };
        }
    }
}
