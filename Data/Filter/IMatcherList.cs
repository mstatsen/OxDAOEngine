namespace OxDAOEngine.Data.Filter
{
    public interface IMatcherList<TField>
        where TField : notnull, Enum
    {
        FilterConcat FilterConcat { get; }
        List<IMatcher<TField>> MatchList { get; }
        bool FilterIsEmpty { get; }
    }
}