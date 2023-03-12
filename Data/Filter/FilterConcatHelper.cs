using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Data.Filter
{
    public class FilterConcatHelper
        : AbstractTypeHelper<FilterConcat>
    {
        public override FilterConcat EmptyValue() => 
            FilterConcat.AND;

        public override string GetName(FilterConcat value) => 
            value switch
            {
                FilterConcat.AND => "and",
                FilterConcat.OR => "or",
                _ => string.Empty,
            };
    }
}