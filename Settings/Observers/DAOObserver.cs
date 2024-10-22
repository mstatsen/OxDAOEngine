using OxDAOEngine.Data;

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
        public bool CategoryFieldsChanged { get; internal set; }
        public bool CurrentViewChanged { get; internal set; }

        public DAOObserver() : base() { }

        public override bool IsEmpty =>
            base.IsEmpty
            && !TableFieldsChanged
            && !SortingFieldsChanged
            && !QuickFilterFieldsChanged
            && !QuickFilterTextFieldsChanged
            && !CategoryFieldsChanged
            && !CurrentViewChanged;

        public override void RenewChanges()
        {
            base.RenewChanges();

            SortingFieldsChanged = fullApplies ||
                !OldValues.Sortings.Equals(Controller.Sortings);
            TableFieldsChanged = fullApplies ||
                !OldValues.Fields[SettingsPart.Table].Equals(Controller.TableFields);
            QuickFilterFieldsChanged = fullApplies ||
                !OldValues.Fields[SettingsPart.QuickFilter].Equals(Controller.QuickFilterFields);
            QuickFilterTextFieldsChanged = fullApplies ||
                !OldValues.Fields[SettingsPart.QuickFilterText].Equals(Controller.QuickFilterTextFields);
            CategoryFieldsChanged = fullApplies ||
                !OldValues.Fields[SettingsPart.Category].Equals(Controller.CategoryFields);
        }

        protected override void Clear()
        {
            TableFieldsChanged = false;
            SortingFieldsChanged = false;
            QuickFilterFieldsChanged = false;
            QuickFilterTextFieldsChanged = false;
            CategoryFieldsChanged = false;
            base.Clear();
        }
    }
}