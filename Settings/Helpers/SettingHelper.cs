using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Fields.Types;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.Part;

namespace OxDAOEngine.Settings.Helpers;

public abstract class SettingHelper<TSetting> : FieldHelper<TSetting>, ISettingHelper
    where TSetting : Enum
{
    public abstract object? Default(TSetting? setting);

    public abstract SettingsPart Part(TSetting setting);

    public SettingList<TSetting> VisibleSettings
    {
        get
        {
            SettingList<TSetting> result = new();

            foreach (TSetting setting in TypeHelper.All<TSetting>())
                if (IsVisible(setting))
                    result.Add(setting);

            return result;
        }
    }

    List<string> ISettingHelper.VisibleItems => VisibleSettings.StringList;

    public SettingList<TSetting> SettingsByPart(SettingsPart part)
    {
        SettingList<TSetting> list = new();

        foreach (TSetting setting in VisibleSettings)
            if (Part(setting).Equals(part))
                list.Add(setting);

        return list;
    }

    public string Name(string value) => Name(ParseSetting(value));

    protected virtual bool IsVisible(TSetting setting) => true;

    public override TSetting EmptyValue() => default!;

    public virtual short ControlWidth(TSetting? setting) => 60;

    public short ControlWidth(string setting) => ControlWidth(ParseSetting(setting));

    public TSetting ParseSetting(string setting) => TypeHelper.Parse<TSetting>(setting);

    public object? Default(string setting) =>
        Default(ParseSetting(setting));

    public SettingsPart Part(string setting) =>
        Part(ParseSetting(setting));

    List<string> ISettingHelper.ItemsByPart(SettingsPart part) =>
        SettingsByPart(part).StringList;

    public bool IsDAOSetting(string setting) => IsDAOSetting(ParseSetting(setting));

    public virtual bool IsDAOSetting(TSetting? setting) => false;

    public virtual bool WithoutLabel(TSetting? setting) => false;

    public bool WithoutLabel(string setting) => WithoutLabel(ParseSetting(setting));

    public override TSetting FieldMetaData => default!;

    public override TSetting TitleField => default!;

    public override TSetting UniqueField => default!;

    protected override FieldFilterOperations<TSetting> GetAvailableFilterOperations() => new();

    protected override List<TSetting> GetCalcedFields() => new();

    protected override List<TSetting> GetCardFields() => new();

    protected override FilterOperation GetDefaultFilterOperation(TSetting field) =>
        FilterOperation.Equals;

    protected override List<TSetting> GetEditedFieldsExtended() => new();

    protected override List<TSetting> GetEditingFields() => new();

    protected override List<TSetting> GetInfoFields() => new();

    protected override List<TSetting> GetGroupByFields() => new();

    protected override List<TSetting> GetMandatoryFields() => new();

    protected override List<TSetting> GetSelectQuickFilterFields() => new();

    public override List<TSetting>? GetFieldsInternal(FieldsVariant variant, FieldsFilling filling) => All();
}