using OxDAOEngine.Data;

namespace OxDAOEngine.Grid
{
    public delegate object? GetColumnValue<TField, TDAO>(TDAO item);

    public class CustomGridColumn<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public readonly string Text;

        public readonly GetColumnValue<TField, TDAO> ValueGetter;

        public readonly int Width;

        public CustomGridColumn(string text, GetColumnValue<TField, TDAO> valueGetter, int width = 100)
        {
            Text = text;
            ValueGetter = valueGetter;
            Width = width;
        }
    }
}
