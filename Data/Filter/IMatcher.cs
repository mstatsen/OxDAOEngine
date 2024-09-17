namespace OxDAOEngine.Data.Filter
{
    public interface IMatcher<TField>
        where TField : notnull, Enum
    {
        bool Match(IFieldMapping<TField>? dao);
        bool FilterIsEmpty { get; }
    }
}