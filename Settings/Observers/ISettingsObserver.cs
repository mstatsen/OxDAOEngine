namespace OxXMLEngine.Settings.Observers
{
    public interface ISettingsObserver
    {
        void RenewChanges();
        bool IsEmpty { get; }
        bool FullApplies { get; set; }
        void RememberState();
        bool this[Enum setting] { get; }
    }

    public interface ISettingsObserver<TSetting> : ISettingsObserver
        where TSetting : Enum
    {
        bool this[TSetting setting] { get; }
    }
}