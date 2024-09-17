using OxDAOEngine.Data.Fields;
using OxDAOEngine.Settings;

namespace OxDAOEngine.Data
{
    public interface IFieldController<TField> : IDataController
        where TField : notnull, Enum
    {
        FieldHelper<TField> FieldHelper { get; }
        IDAOSettings<TField> SettingsByField { get; }
    }
}