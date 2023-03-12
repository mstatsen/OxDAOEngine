using System;
using System.Collections.Generic;

namespace OxXMLEngine.Data.Extract
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

            if (Type == ExtractCompareType.Count)
                result =
                    x == null
                        ? y == null
                            ? 0
                            : -1
                        : y == null
                            ? 1
                            : Extract[x].CompareTo(Extract[y]);

            if (result == 0)
                result = string.Compare(x?.ToString(),y?.ToString());

            return result;
        }

        private readonly ExtractCompareType Type;
        private readonly FieldCountExtract Extract;
    }
}