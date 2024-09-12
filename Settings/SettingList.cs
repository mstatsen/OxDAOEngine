namespace OxXMLEngine.Settings
{
    public class SettingList<TSetting> : List<TSetting>
        where TSetting : Enum
    {
        private string StringConverter(TSetting t) => t.ToString();

        public List<string> StringList => 
            ConvertAll(new Converter<TSetting, string>(StringConverter));
    }
}
