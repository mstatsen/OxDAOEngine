using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.Part;

namespace OxDAOEngine.Settings.Helpers
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