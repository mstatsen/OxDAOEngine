using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.Data
{
    public interface IFieldMapping<TField>
        where TField : notnull, Enum
    {
        object? this[TField field] { get; set; }
        object ParseCaldedValue(TField field, string value);
        FilterOperation DefaultFilterOperation(TField field);
        bool IsCalcedField(TField field);
    }
}