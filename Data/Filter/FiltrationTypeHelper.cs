using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Data.Filter
{
    public class FiltrationTypeHelper : AbstractTypeHelper<FiltrationType>
    {
        public override FiltrationType EmptyValue() =>
            FiltrationType.StandAlone;

        public override string GetName(FiltrationType value) => 
            value switch
            {
                FiltrationType.StandAlone => "StandAlone",
                FiltrationType.IncludeParent => "IncludeParent",
                FiltrationType.BaseOnChilds => "BaseOnChilds",
                _ => string.Empty,
            };
    }
}