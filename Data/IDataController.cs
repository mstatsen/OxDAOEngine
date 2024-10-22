using OxLibrary.Panels;
using OxDAOEngine.Settings;
using System.Xml;
using OxDAOEngine.Settings.Part;

namespace OxDAOEngine.Data
{
    public interface IDataController
    {
        void Save(XmlElement? parentElement);
        void Load(XmlElement? parentElement);
        string FileName { get; }
        string ListName { get; }
        string ItemName { get; }
        bool Modified { get; }
        bool IsSystem { get; }
        event ModifiedChangeHandler? ModifiedHandler;
        ISettingsController Settings { get; }
        SettingsPart ActiveSettingsPart { get; }
        OxPane? Face { get; }
    }
}