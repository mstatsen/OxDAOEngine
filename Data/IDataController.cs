using OxLibrary.Panels;
using OxDAOEngine.Settings;
using System.Xml;

namespace OxDAOEngine.Data
{
    public interface IDataController
    {
        void Save(XmlElement? parentElement);
        void Load(XmlElement? parentElement);
        string FileName { get; }
        string Name { get; }
        bool Modified { get; }
        bool IsSystem { get; }
        event ModifiedChangeHandler? ModifiedHandler;
        ISettingsController Settings { get; }
        SettingsPart ActiveSettingsPart { get; }
        OxPane? Face { get; }
    }
}