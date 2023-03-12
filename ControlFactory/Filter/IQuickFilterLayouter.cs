using OxXMLEngine.Data.Filter;

namespace OxXMLEngine.ControlFactory.Filter
{
    public interface IQuickFilterLayouter<TField>
        where TField : notnull, Enum
    {
        TField TextFilterContainer { get; }
        int FieldWidth(TField field);
        string FieldCaption(TField field, QuickFilterVariant variant);
        bool IsLastLayoutForOneRow(TField field, TField lastField);
    }
}
