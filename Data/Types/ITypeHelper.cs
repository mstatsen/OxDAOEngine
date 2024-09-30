namespace OxDAOEngine.Data.Types
{
    public interface ITypeHelper
    {
        string Name(object value);
        string ShortName(object value);
        string FullName(object value);
        string XmlValue(object value);
        object? Value(object? typeObject);
        object? TypeObject(object? value);
        object Parse(string text);
        object EmptyValue();
        object DefaultValue();
        Type ItemType { get; }
        Type ItemObjectType { get;  }

        List<object> All();
        string Caption(object value);

        bool UseFullNameForControl { get; }
        bool UseShortNameForControl { get; }
        bool UseToolTipForControl { get; }
    }
}
