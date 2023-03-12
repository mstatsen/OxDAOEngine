using OxXMLEngine.Data.Fields;
using OxXMLEngine.Settings;

namespace OxXMLEngine.Data
{
    public interface IFieldController<TField> : IDataController
        where TField : notnull, Enum
    {
        FieldHelper<TField> FieldHelper { get; }
        IDAOSettings<TField> SettingsByField { get; }
    }
}