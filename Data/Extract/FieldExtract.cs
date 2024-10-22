using System.Collections;

namespace OxDAOEngine.Data.Extract
{
    public class FieldExtract<T> : List<T>
    {
        public new FieldExtract<T> Sort()
        {
            base.Sort();
            return this;
        }
    }

    public class FieldExtract : FieldExtract<object>
    {
        public new FieldExtract Sort() =>
            (FieldExtract)base.Sort();

        public new void Add(object value)
        {
            if (value is not string &&
                value is IEnumerable enumerable)
            {
                foreach (object itemValue in enumerable)
                {
                    object keyValue = itemValue;

                    if (itemValue is DAO daoValue)
                        keyValue = daoValue.ExtractKeyValue;

                    if (!Contains(keyValue))
                        base.Add(keyValue);
                }

                return;
            }

            base.Add(value);
        }
    }
}
