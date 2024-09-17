using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Extract
{
    public class ExtractCompareTypeHelper : AbstractTypeHelper<ExtractCompareType>
    {
        public override ExtractCompareType EmptyValue() => ExtractCompareType.Default;

        public override string GetName(ExtractCompareType value) => 
            value switch
            {
                ExtractCompareType.Abc => "Alphabetical",
                ExtractCompareType.Count => "By count",
                _ => "By default",
            };
    }
}