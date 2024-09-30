using OxDAOEngine.Data;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Export;

namespace OxDAOEngine.Export
{
    public abstract class Exporter<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public Exporter(ExportSettings<TField, TDAO> exportSettings, ExportSettingsForm<TField, TDAO> settingsForm)
        {
            Settings = exportSettings;
            SettingsForm = settingsForm;

            Items = new RootListDAO<TField, TDAO>();
            Items.CopyFrom(
                SettingsForm.SelectedItems
                    ?? ListController.FullItemsList
                        .FilteredList(
                            ListController.SystemCategories?
                                .Find(c => c.Name == Settings.CategoryName))
                        .FilteredList(Settings.Filter)
            );
            Items.Sort(Sortings()?.SortingsList);
        }

        protected IListController<TField, TDAO> ListController = DataManager.ListController<TField, TDAO>();

        public abstract string Text();

        protected abstract FieldSortings<TField, TDAO>? Sortings();

        protected readonly RootListDAO<TField, TDAO> Items;
        protected readonly ExportSettings<TField, TDAO> Settings;
        protected readonly ExportSettingsForm<TField, TDAO> SettingsForm;
    }
}