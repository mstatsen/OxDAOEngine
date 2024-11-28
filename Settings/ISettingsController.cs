using OxLibrary;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Settings.Helpers;
using OxDAOEngine.Settings.Observers;

namespace OxDAOEngine.Settings
{
    public interface ISettingsController : IDataController, IOxWithIcon
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