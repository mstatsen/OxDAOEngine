namespace OxXMLEngine.Grid
{
    public class GridFieldColumns<TField> : Dictionary<TField, DataGridViewColumn>
        where TField: notnull, Enum
    {
        public TField GetField(int dataColumnIndex)
        {
            foreach (KeyValuePair<TField, DataGridViewColumn> item in this)
                if (item.Value.Index == dataColumnIndex)
                    return item.Key;

            return default!;
        }
    }
}