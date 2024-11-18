using OxDAOEngine.Data.Filter.Types;

namespace OxDAOEngine.Data.Filter
{
    public class MatchAggregator<TField>
        where TField : notnull, Enum
    {
        private readonly FilterConcat Concat;

        public MatchAggregator(FilterConcat concat)
        {
            Concat = concat;
            Matched = Concat is FilterConcat.AND;
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
            Concat is FilterConcat.AND 
            && !Matched;

        public static bool IsEmpty(IMatcherList<TField> matcher)
        {
            MatchAggregator<TField> aggregator = new(FilterConcat.OR);

            foreach (IMatcher<TField> filter in matcher.MatchList)
            {
                aggregator.Aggregate(!filter.FilterIsEmpty);

                if (aggregator.Complete)
                    break;
            }

            return !aggregator.Matched;
        }

        public static bool Match(IMatcherList<TField> matcher, IFieldMapping<TField>? dao)
        {
            if (matcher.FilterIsEmpty)
                return true;

            MatchAggregator<TField> aggregator = new(matcher.FilterConcat);

            foreach (IMatcher<TField> matcherItem in matcher.MatchList)
            {
                aggregator.Aggregate(matcherItem.Match(dao));

                if (aggregator.Complete)
                    break;
            }

            return aggregator.Matched;
        }
    }
}