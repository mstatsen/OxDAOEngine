namespace OxDAOEngine.Data.Extract
{
    public class CountExtractComparer : IComparer<object>
    {
        public CountExtractComparer(FieldCountExtract extract, ExtractCompareType type)
        {
            Extract = extract;
            Type = type;
        }

        public int Compare(object? x, object? y)
        {
            int result = 0;

            if (Type is ExtractCompareType.Count)
                result =
                    x is null
                        ? y is null
                            ? 0
                            : -1
                        : y is null
                            ? 1
                            : Extract[x].CompareTo(Extract[y]);

            if (result is 0)
                result = 
                    string.Compare(
                        x?.ToString(),
                        y?.ToString()
                    );

            return result;
        }

        private readonly ExtractCompareType Type;
        private readonly FieldCountExtract Extract;
    }
}