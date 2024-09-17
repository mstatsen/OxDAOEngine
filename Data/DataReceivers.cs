using OxDAOEngine.Settings;

namespace OxDAOEngine.Data
{
    public static class DataReceivers
    {
        private static readonly List<IDataReceiver> Receivers = new();

        public static void Register(IDataReceiver receiver) =>
            Receivers.Add(receiver);

        public static void ApplySettings(bool fullApplies = false, bool firstLoad = false)
        { 
            if (fullApplies)
                SettingsManager.SetFullApplies(true);
            else
                SettingsManager.RenewChanges();

            try
            {
                foreach (IDataReceiver receiver in Receivers)
                    receiver.ApplySettings(firstLoad);
            }
            finally
            {
                if (fullApplies)
                    SettingsManager.SetFullApplies(false);
                else
                    SettingsManager.RememberState();
            }
        }

        public static void FillData()
        {
            ApplySettings(true, true);

            foreach (IDataReceiver receiver in Receivers)
                receiver.FillData();
        }

        public static void SaveSettings()
        {
            foreach (IDataReceiver receiver in Receivers)
                receiver.SaveSettings();

            SettingsManager.RememberState();
        }
    }
}
