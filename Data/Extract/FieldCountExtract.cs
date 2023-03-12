namespace OxXMLEngine.Data.Extract
{
    public class FieldCountExtract : Dictionary<object, int>
    {
        public FieldCountExtract Sort(ExtractCompareType type)
        {
            if (type == ExtractCompareType.Default)
                return this;

            FieldExtract list = new();
            FieldCountExtract countExtract = new();

            foreach (KeyValuePair<object, int> item in this)
            {
                list.Add(item.Key);
                countExtract.Add(item.Key, item.Value);
            }
            Clear();
            list.Sort(new CountExtractComparer(countExtract, type));

            foreach (object item in list)
                Add(item, countExtract[item]);

            return this;
        }
    }
}