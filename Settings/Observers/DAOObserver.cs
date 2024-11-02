using OxDAOEngine.Data;
using OxDAOEngine.Settings.Part;

namespace OxDAOEngine.Settings.Observers
{
    public class DAOObserver<TField, TDAO>
        : SettingsObserver<DAOSetting, DAOSettings<TField, TDAO>>, ISettingsObserver<DAOSetting>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public bool TableFieldsChanged { get; internal set; }
        public bool SortingFieldsChanged { get; internal set; }
        public bool QuickFilterFieldsChanged { get; internal set; }
        public bool QuickFilterTextFieldsChanged { get; internal set; }
        public bool CategoriesChanged { get; internal set; }
        public bool CurrentViewChanged { get; internal set; }

        public DAOObserver() : base() { }

        public override bool IsEmpty =>
            base.IsEmpty
            && !TableFieldsChanged
            && !SortingFieldsChanged
            && !QuickFilterFieldsChanged
            && !QuickFilterTextFieldsChanged
            && !CategoriesChanged
            && !CurrentViewChanged;

        public override void RenewChanges()
        {
            base.RenewChanges();

            if (fullApplies)
            {
                SortingFieldsChanged = true;
                TableFieldsChanged = true;
                QuickFilterFieldsChanged = true;
                QuickFilterTextFieldsChanged = true;
                CategoriesChanged = true;
            }
            else
            {
                SortingFieldsChanged = !OldValues.Sortings.Equals(Controller.Sortings);
                TableFieldsChanged = !OldValues.Fields[SettingsPart.Table].Equals(Controller.TableFields);
                QuickFilterFieldsChanged = !OldValues.Fields[SettingsPart.QuickFilter].Equals(Controller.QuickFilterFields);
                QuickFilterTextFieldsChanged = !OldValues.Fields[SettingsPart.QuickFilterText].Equals(Controller.QuickFilterTextFields);
                CategoriesChanged = !OldValues.Categories.Equals(Controller.Categories);
            }
        }

        protected override void Clear()
        {
            TableFieldsChanged = false;
            SortingFieldsChanged = false;
            QuickFilterFieldsChanged = false;
            QuickFilterTextFieldsChanged = false;
            CategoriesChanged = false;
            base.Clear();
        }
    }
}