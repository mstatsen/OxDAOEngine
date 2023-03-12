using OxLibrary.Panels;
using OxXMLEngine.Settings;
using System.Xml;

namespace OxXMLEngine.Data
{
    public interface IDataController
    {
        void Save(XmlElement? parentElement);
        void Load(XmlElement? parentElement);
        string FileName { get; }
        string Name { get; }
        bool Modified { get; }
        bool IsSystem { get; }
        ModifiedChangeHandler? ModifiedHandler { get; set; }
        ISettingsController Settings { get; }
        SettingsPart ActiveSettingsPart { get; }
        OxPane? Face { get; }
    }
}