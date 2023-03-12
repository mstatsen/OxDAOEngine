namespace OxXMLEngine.Data.Filter
{
    public class FilterOperations : List<FilterOperation>
    {
        public static FilterOperations UnaryOperations => new()
        {
            FilterOperation.Blank,
            FilterOperation.NotBlank
        };

        public static FilterOperations StringOperations => new()
        {
            FilterOperation.Equals,
            FilterOperation.NotEquals,
            FilterOperation.Contains,
            FilterOperation.NotContains,
            FilterOperation.StartsWith,
            FilterOperation.EndsWith,
            FilterOperation.Blank,
            FilterOperation.NotBlank
        };

        public static FilterOperations ObjectOperations => new()
        {
            FilterOperation.Equals,
            FilterOperation.NotEquals,
            FilterOperation.Contains,
            FilterOperation.NotContains,
            FilterOperation.Blank,
            FilterOperation.NotBlank
        };

        public static FilterOperations NumericOperations => new()
        {
            FilterOperation.Equals,
            FilterOperation.NotEquals,
            FilterOperation.Greater,
            FilterOperation.Lower,
        };

        public static FilterOperations BoolOperations => new()
        {
            FilterOperation.Equals,
            FilterOperation.NotEquals,
        };

        public static FilterOperations EnumOperations => new()
        {
            FilterOperation.Equals,
            FilterOperation.NotEquals
        };
    }
}