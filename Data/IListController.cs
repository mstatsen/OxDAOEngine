using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data.Decorator;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.History;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Editor;
using OxDAOEngine.Export;
using OxDAOEngine.Settings;
using OxDAOEngine.Summary;
using OxDAOEngine.View;
using OxDAOEngine.Grid;

namespace OxDAOEngine.Data
{
    public interface IListController<TField, TDAO> : IFieldController<TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        new RootListDAO<TField, TDAO> FullItemsList { get; }
        RootListDAO<TField, TDAO> VisibleItemsList { get; }
        DAOImageList<TField, TDAO> ImageList { get; }
        DAOImage? ImageInfo(Guid imageId);
        Bitmap? Image(Guid imageId);
        Bitmap? Icon { get; }

        ItemHistoryList<TField, TDAO> History { get; }

        TDAO? Item(TField field, object value);

        void AddItem();
        void EditItem(TDAO? item, ItemsRootGrid<TField, TDAO>? parentGrid = null);
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
        DAOEntityEventHandler<TDAO>? AddHandler { get; set; }
        DAOEntityEventHandler<TDAO>? RemoveHandler { get; set;  }
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
        List<ToolStripMenuItem>? MenuItems(TDAO? item);

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