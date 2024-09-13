using OxLibrary.Panels;
using OxXMLEngine.ControlFactory;
using OxXMLEngine.Data.Decorator;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.History;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Editor;
using OxXMLEngine.Export;
using OxXMLEngine.Settings;
using OxXMLEngine.Summary;
using OxXMLEngine.View;

namespace OxXMLEngine.Data
{
    public interface IListController<TField, TDAO> : IFieldController<TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        RootListDAO<TField, TDAO> FullItemsList { get; }
        RootListDAO<TField, TDAO> VisibleItemsList { get; }
        ItemHistoryList<TField, TDAO> History { get; }

        TDAO? Item(TField field, object value);

        void AddItem();
        void EditItem(TDAO? item);
        void CopyItem(TDAO? item);
        void ViewItem(TDAO? item, ItemViewMode viewMode = ItemViewMode.Simple);
        void ViewItem(TField field, object? value, ItemViewMode viewMode = ItemViewMode.Simple);
        void ViewItems(TField field, object? value);
        bool SelectItem(out TDAO? selectedItem, OxPane parentPane, TDAO? initialItem = null, IMatcher<TField>? filter = null);
        void ViewHistory();
        void Delete(RootListDAO<TField, TDAO> list);

        int TotalCount { get; }
        int FilteredCount { get; }
        int ModifiedCount { get; }
        int RemovedCount { get; }
        int AddedCount { get; }
        Category<TField, TDAO>? Category { get; set; }
        DAOEntityEventHandler? AddHandler { get; set; }
        DAOEntityEventHandler? RemoveHandler { get; set;  }
        EventHandler? ListChanged { get; set; }
        EventHandler? OnAfterLoad { get; set; }

        event EventHandler? ItemsSortChangeHandler;
        Categories<TField, TDAO>? SystemCategories { get; }
        new ItemsFace<TField, TDAO> Face { get; }
        DAOEntityEventHandler? ItemFieldChanged { get; set; }

        event EventHandler? CategoryChanged;
        DecoratorFactory<TField, TDAO> DecoratorFactory { get; }
        ControlFactory<TField, TDAO> ControlFactory { get; }
        ExportController<TField, TDAO> ExportController { get; }
        new DAOSettings<TField, TDAO> Settings { get; }
        void Sort();
        List<ISummaryPanel>? GeneralSummaries { get; }
        FieldSortings<TField, TDAO>? DefaultSorting();
        bool AvailableSummary { get; }
        bool AvailableCategories { get; }
        bool AvailableQuickFilter { get; }
        bool AvailableCards { get; }
        bool AvailableIcons { get; }
        bool AvailableBatchUpdate { get; }
        bool AvailableCopyItems { get; }
    }

    public interface IListController<TField, TDAO, TFieldGroup>
        : IListController<TField, TDAO>,
        IFieldGroupController<TField, TFieldGroup>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TFieldGroup : notnull, Enum
    {
        DAOWorker<TField, TDAO, TFieldGroup> Worker { get; }
        DAOEditor<TField, TDAO, TFieldGroup> Editor { get; }
    }
}