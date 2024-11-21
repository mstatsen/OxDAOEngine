using OxDAOEngine.Data.Filter;
using OxLibrary;

namespace OxDAOEngine.ControlFactory.Filter
{
    public interface IQuickFilterLayouter<TField>
        where TField : notnull, Enum
    {
        TField TextFilterContainer { get; }
        OxWidth FieldWidth(TField field);
        string FieldCaption(TField field, QuickFilterVariant variant);
        bool IsLastLayoutForOneRow(TField field, TField lastField);
    }
}
