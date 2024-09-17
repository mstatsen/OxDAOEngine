using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.Data.Extract
{
    public class FieldExtractor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public FieldExtract Extract(TField field, bool ignoreDubles, bool ignoreEmpty = false) =>
            Extract(field, null, ignoreDubles, ignoreEmpty);

        public FieldExtract<T> Extract<T>(TField field, IMatcher<TField>? filter,
            bool ignoreDubles, bool ignoreEmpty = false)
        {
            FieldExtract extract = Extract(field, filter, ignoreDubles, ignoreEmpty);
            FieldExtract<T> result = new();

            foreach (object value in extract)
                result.Add((T)value);

            return result;
        }

        public int Sum(TField field, IMatcher<TField>? filter)
        {
            List<int> extract = Extract<int>(field, filter, false, true);

            int result = 0;

            foreach (int value in extract)
                result += value;

            return result;
        }

        private static bool NeedIgnore(bool ignoreEmpty, object? value) =>
            ignoreEmpty
            && (value == null
                ||
                (value is string
                && value.ToString() == string.Empty));


        public FieldExtract Extract(TField field, IMatcher<TField>? filter, 
            bool ignoreDubles, bool ignoreEmpty = false)
        {
            FieldExtract result = new();

            if (Items == null)
                return result;

            foreach (TDAO item in Items.FilteredList(filter))
            {
                object? value = item[field];

                if (FieldExtractor<TField, TDAO>.NeedIgnore(ignoreEmpty, value))
                    continue;

                if (value == null)
                    continue;

                if (ignoreDubles && result.Contains(value))
                    continue;

                result.Add(value);
            }

            return result.Sort();
        }

        public FieldCountExtract CountExtract(TField field, bool ignoreDubles,
            ExtractCompareType compareType = ExtractCompareType.Default) =>
            CountExtract(field, null, ignoreDubles, compareType);


        public FieldCountExtract CountExtract(TField field,
            IMatcher<TField>? filter, bool ignoreDubles, 
            ExtractCompareType compareType = ExtractCompareType.Default)
        {
            FieldCountExtract result = new();

            if (Items == null || !AvailableCountExtract(field))
                return result;

            RootListDAO<TField, TDAO> extractItems = Items.FilteredList(filter);

            foreach (object value in Extract(field, filter, ignoreDubles, true))
                result.Add(value, extractItems.FilteredList(
                    new Category<TField, TDAO>().AddFilter(field, value)
                ).Count);

            return result.Sort(compareType);
        }

        protected virtual bool AvailableCountExtract(TField field) => true;

        public RootListDAO<TField, TDAO>? Items;

        public FieldExtractor(RootListDAO<TField, TDAO>? items) =>
            Items = items;

        public FieldExtractor() { }
    }
}