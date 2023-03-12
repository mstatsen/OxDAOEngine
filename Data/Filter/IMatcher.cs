namespace OxXMLEngine.Data.Filter
{
    public interface IMatcher<TDAO>
        where TDAO : DAO
    {
        bool Match(TDAO? dao);
        bool FilterIsEmpty { get; }
    }
}