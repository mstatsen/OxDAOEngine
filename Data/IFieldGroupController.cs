using OxXMLEngine.Data.Fields;

namespace OxXMLEngine.Data
{
    public interface IFieldGroupController<TField, TFieldGroup> : IFieldController<TField>
        where TField : notnull, Enum
        where TFieldGroup : notnull, Enum
    {
        FieldGroupHelper<TField, TFieldGroup> FieldGroupHelper { get; }
    }
}