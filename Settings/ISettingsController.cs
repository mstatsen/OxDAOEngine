using OxXMLEngine.ControlFactory.Accessors;
using OxXMLEngine.Data;
using OxXMLEngine.Settings.Helpers;
using OxXMLEngine.Settings.Observers;

namespace OxXMLEngine.Settings
{
    public interface ISettingsController : IDataController
    {
        object? this[string setting] { get; set; }
        ISettingHelper Helper { get; }
        ISettingsObserver Observer { get; }
        IControlAccessor Accessor(string setting);
        object? GetDefault(string setting);
    }

    public interface ISettingsController<TSetting> : ISettingsController
    {
        object? this[TSetting setting] { get; set; }
    }
}