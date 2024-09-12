using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Settings.Helpers
{
    public interface ISettingHelper : ITypeHelper
    {
        object? Default(string setting);
        SettingsPart Part(string setting);
        List<string> VisibleItems { get; }
        List<string> ItemsByPart(SettingsPart part);
        List<string>? CardSettingsItems { get; }
        List<string>? IconSettingsItems { get; }
        string Name(string value);
        int ControlWidth(string setting);
        bool IsDAOSetting(string setting);
    }
}