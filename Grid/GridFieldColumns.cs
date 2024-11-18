namespace OxDAOEngine.Grid
{
    public class GridFieldColumns<TField> : Dictionary<TField, DataGridViewColumn>
        where TField: notnull, Enum
    {
        public TField GetField(int dataColumnIndex)
        {
            foreach (var item in this)
                if (item.Value.Index.Equals(dataColumnIndex))
                    return item.Key;

            return default!;
        }
    }
}