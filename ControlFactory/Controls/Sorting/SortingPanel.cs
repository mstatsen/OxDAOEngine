using OxLibrary;
using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Sorting.Types;
using OxDAOEngine.Settings;

namespace OxDAOEngine.ControlFactory.Controls.Sorting
{
    public partial class SortingPanel<TField, TDAO> : FunctionsPanel<TField, TDAO>, ISortingPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, IFieldMapping<TField>, new()
    {
        public SortingPanel(SortingVariant variant, ControlScope scope) : base()
        {
            Variant = variant;
            Scope = scope;
            Text = variantHelper.Name(variant);
            ClearButton.Text = SortingVariantHelper.ClearButtonText(variant);
            BaseColor = variantHelper.BaseColor(variant);

            sortingControlAccessor = DataManager.Builder<TField, TDAO>(Scope).SortingListAccessor();
            sortingControlAccessor.Parent = this;
            sortingControlAccessor.Dock = DockStyle.Fill;
            sortingControlAccessor.Value = sortings;
            sortingControlAccessor.ValueChangeHandler += ChangeSortingHandler;

            PrepareColors();
            Size = new(OxWh.W240, OxWh.W94);
        }

        public ControlScope Scope { get; set; }

        public void ResetToDefault() =>
            ClearControls();

        public void RenewControls()
        {
            DisableValueChangeHandler();
            sortingControlAccessor.Value = sortings;
            EnableValueChangeHandler();
        }

        public void DisableValueChangeHandler() =>
            sortingControlAccessor.ValueChangeHandler -= ChangeSortingHandler;

        public void EnableValueChangeHandler() =>
            sortingControlAccessor.ValueChangeHandler += ChangeSortingHandler;

        protected override Color FunctionColor => EngineStyles.SortingColor;

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            ClearButton.Size = new(OxWh.W80, OxWh.W23);
            ClearButton.Click += (s, e) => ClearControls();
            Header.AddToolButton(ClearButton);
        }

        private void ClearControls()
        {
            DisableValueChangeHandler();

            try
            {
                sortings.Clear(Variant);
                sortingControlAccessor.Value = sortings;
            }
            finally
            {
                EnableValueChangeHandler();
            }
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (sortingControlAccessor is not null)
                ((SortingsControl<TField, TDAO>)sortingControlAccessor.Control).BaseColor = BaseColor;
        }

        private void ChangeSortingHandler(object? sender, EventArgs e)
        {
            sortings.CopyFrom((FieldSortings<TField, TDAO>?)sortingControlAccessor.Value);
            ExternalChangeHandler?.Invoke(sortings, new DAOEntityEventArgs(DAOOperation.Modify));
        }

        private readonly OxButton ClearButton = new("Clear", OxIcons.Eraser)
        {
            Font = Styles.Font(-1, FontStyle.Bold)
        };
        private readonly IControlAccessor sortingControlAccessor;

        public SortingVariant Variant { get; internal set; }

        private readonly SortingVariantHelper variantHelper = TypeHelper.Helper<SortingVariantHelper>();

        private readonly FieldSortings<TField, TDAO> sortings = new();

        public DAOEntityEventHandler? ExternalChangeHandler { get; set; }

        public FieldSortings<TField, TDAO> Sortings
        {
            get
            {
                FieldSortings<TField, TDAO> result = new();
                result.CopyFrom(sortings);
                return result;
            }
            set
            {
                sortings.CopyFrom(value);
                RenewControls();
            }
        }

        protected override DAOSetting VisibleSetting => DAOSetting.ShowQuickFilter;

        protected override DAOSetting PinnedSetting => DAOSetting.QuickFilterPinned;

        protected override DAOSetting ExpandedSetting => DAOSetting.QuickFilterExpanded;

        protected override void ApplySettingsInternal()
        {
            if (Observer.SortingFieldsChanged)
                RenewControls();
        }
    }
}