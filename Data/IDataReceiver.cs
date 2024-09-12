namespace OxXMLEngine.Data
{
    public interface IDataReceiver
    {
        void FillData();
        void ApplySettings(bool firstLoad);
        void SaveSettings();
    }
}
