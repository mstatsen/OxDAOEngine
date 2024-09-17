using OxDAOEngine.Data.Fields;
using OxDAOEngine.SystemEngine;

namespace OxDAOEngine.Settings.ControlFactory
{
    public class GeneralSettingsControlFactory : SystemControlFactory<GeneralSetting>
    {
        protected override FieldType GetFieldControlTypeInternal(GeneralSetting field) => 
            field switch
            {
                GeneralSetting.ShowCustomizeButtons or 
                GeneralSetting.ColorizePanels or 
                GeneralSetting.DarkerHeaders or
                GeneralSetting.DoublePinButtons => 
                    FieldType.Boolean,
                _ => 
                    FieldType.Custom,
            };
    }
}