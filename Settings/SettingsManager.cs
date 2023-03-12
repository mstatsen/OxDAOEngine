using OxXMLEngine.Data;

namespace OxXMLEngine.Settings
{
    public static class SettingsManager
    {
        public static readonly List<ISettingsController> Controllers = new();
        private static readonly Dictionary<Type, ISettingsController> DAOControllers = new();

        public static void Register<TField>(ISettingsController settings)
            where TField : notnull, Enum
        {
            DAOControllers.Add(typeof(TField), settings);
            Register(settings);
        }

        public static void Register(ISettingsController settings)
        {
            if (Controllers.Contains(settings))
                return;

            Controllers.Add(settings);
            DataManager.Register(settings);
        }

        public static TSettings Settings<TSettings>()
            where TSettings : ISettingsController
        {
            foreach (ISettingsController controller in Controllers)
                if (controller is TSettings settings)
                    return settings;

            throw new KeyNotFoundException(
                $"Settings {typeof(TSettings).FullName} is not exist "
            );
        }

        public static IDAOSettings<TField> DAOSettings<TField>()
            where TField : notnull, Enum =>
            Settings<IDAOSettings<TField>>();

        public static DAOSettings<TField, TDAO> DAOSettings<TField, TDAO>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new() =>
            Settings<DAOSettings<TField, TDAO>>();

        static SettingsManager() => 
            Register(new GeneralSettings());

        public static void Init() {}

        public static void RememberState()
        {
            foreach (ISettingsController settings in Controllers)
                settings.Observer.RememberState();
        }

        public static void RenewChanges()
        {
            foreach (ISettingsController settings in Controllers)
                settings.Observer.RenewChanges();
        }

        public static void SetFullApplies(bool fullApplies)
        {
            foreach (ISettingsController settings in Controllers)
                settings.Observer.FullApplies = fullApplies;
        }
    }
}
