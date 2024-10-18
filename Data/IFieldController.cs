using OxDAOEngine.Data.Fields;
using OxDAOEngine.Settings;

namespace OxDAOEngine.Data
{
    public interface IFieldController<TField> : IDataController
        where TField : notnull, Enum
    {
        FieldHelper<TField> FieldHelper { get; }
        IDAOSettings<TField> SettingsByField { get; }
        bool UseImageList { get; }
        DAOImage? GetImageInfo(Guid imageId);
        DAOImage? SuitableImage(Bitmap? value);
        DAOImage UpdateImage(Guid imageId, Bitmap? image);
        IListDAO FullItemsList { get; }
    }
}