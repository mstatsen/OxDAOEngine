using OxXMLEngine.Data;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Settings;

namespace OxXMLEngine.Export
{
    public abstract class Exporter<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public Exporter(ExportSettings<TField, TDAO> exportSettings)
        {
            Settings = exportSettings;
            Items = new RootListDAO<TField, TDAO>();
            Items.CopyFrom(ListController.FullItemsList
                .FilteredList(
                    ListController.SystemCategories?
                        .Find((c) => c.Name == SettingsManager.DAOSettings<TField, TDAO>().ExportSettings.CategoryName))
                .FilteredList(SettingsManager.DAOSettings<TField, TDAO>().ExportSettings.Filter));
            Items.Sort(Sortings()?.SortingsList);
        }

        protected IListController<TField, TDAO> ListController = DataManager.ListController<TField, TDAO>();

        public abstract string Text();

        protected abstract FieldSortings<TField, TDAO>? Sortings();

        protected readonly RootListDAO<TField, TDAO> Items;
        protected readonly ExportSettings<TField, TDAO> Settings;
    }
}
