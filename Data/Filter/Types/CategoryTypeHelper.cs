using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Filter.Types
{
    public class CategoryTypeHelper : AbstractTypeHelper<CategoryType>
    {
        public override CategoryType EmptyValue() => 
            CategoryType.Filter;

        public override string GetName(CategoryType value) =>
            value switch
            {
                CategoryType.Filter => "Filter",
                CategoryType.FieldExtraction => "Field Extraction",
                _ => string.Empty,
            };
    }
}