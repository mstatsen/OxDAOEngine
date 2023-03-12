using System.Collections.Generic;

namespace OxXMLEngine.Data.Filter
{
    public interface IMatcherList<TDAO>
        where TDAO : DAO
    {
        FilterConcat FilterConcat { get; }
        List<IMatcher<TDAO>> MatchList { get; }
        bool FilterIsEmpty { get; }
    }
}