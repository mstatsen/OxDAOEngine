using OxDAOEngine.Data;

namespace OxDAOEngine.Grid
{
    public class CustomGridColumns<TField, TDAO> : Dictionary<CustomGridColumn<TField, TDAO>, DataGridViewColumn>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public CustomGridColumn<TField, TDAO>? GetCustomColumn(DataGridViewColumn column)
        { 
            foreach (var item in this)
                if (item.Value == column)
                    return item.Key;

            return null;
        }
    }
}