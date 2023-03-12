using OxXMLEngine.Data.Fields;
using OxXMLEngine.SystemEngine;

namespace OxXMLEngine.Settings.ControlFactory
{
    public class GeneralSettingsControlFactory : SystemControlFactory<GeneralSetting>
    {
        protected override FieldType GetFieldControlTypeInternal(GeneralSetting field) => 
            field switch
            {
                GeneralSetting.ShowCustomizeButtons or 
                GeneralSetting.ColorizePanels or 
                GeneralSetting.DarkerHeaders => 
                    FieldType.Boolean,
                _ => 
                    FieldType.Custom,
            };
    }
}