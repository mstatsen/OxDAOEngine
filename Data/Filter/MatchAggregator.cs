namespace OxXMLEngine.Data.Filter
{
    public class MatchAggregator<TDAO>
        where TDAO : DAO
    {
        private readonly FilterConcat Concat;

        public MatchAggregator(FilterConcat concat)
        {
            Concat = concat;
            Matched = Concat == FilterConcat.AND;
        }

        public void Aggregate(bool newMatch)
        {
            switch (Concat)
            {
                case FilterConcat.AND:
                    Matched &= newMatch;
                    break;
                case FilterConcat.OR:
                    Matched |= newMatch;
                    break;
            }
        }

        public bool Matched { get; internal set; }
        public bool Complete => 
            Concat == FilterConcat.AND && !Matched;

        public static bool IsEmpty(IMatcherList<TDAO> matcher)
        {
            MatchAggregator<TDAO> aggregator = new(FilterConcat.OR);

            foreach (IMatcher<TDAO> filter in matcher.MatchList)
            {
                aggregator.Aggregate(!filter.FilterIsEmpty);

                if (aggregator.Complete)
                    break;
            }

            return !aggregator.Matched;
        }

        public static bool Match(IMatcherList<TDAO> matcher, TDAO? dao)
        {
            MatchAggregator<TDAO> aggregator = new(matcher.FilterConcat);

            foreach (IMatcher<TDAO> matcherItem in matcher.MatchList)
            {
                aggregator.Aggregate(matcherItem.Match(dao));

                if (aggregator.Complete)
                    break;
            }

            return aggregator.Matched;
        }
    }
}