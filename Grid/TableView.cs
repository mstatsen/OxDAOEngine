using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Observers;
using OxDAOEngine.View;

namespace OxDAOEngine.Grid
{
    public class TableView<TField, TDAO> : OxFrame
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TableView() : base() 
        {
            Borders.SetSize(OxSize.None);
            Grid.Parent = this;
            Grid.Dock = DockStyle.Fill;
            Grid.ToolbarActionClick += (s, e) => ExecuteAction(e.Action);
            Grid.CurrentItemChanged += CurrentItemChangeHandler;
            CurrentInfoCard = DataManager.ControlFactory<TField, TDAO>().CreateInfoCard();
            PrepareInfoCard();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UpdateCurrentItemFullCard();
        }

        private void PrepareInfoCard()
        {
            if (CurrentInfoCard == null)
                return;

            CurrentInfoCard.Parent = this;
            CurrentInfoCard.Margins.SetSize(OxSize.Medium);
            CurrentInfoCard.Margins.LeftOx = OxSize.Large;
            CurrentInfoCard.Margins.TopOx = OxSize.Large;
            CurrentInfoCard.Dock = DockStyle.Right;
            CurrentInfoCard.SetContentSize(500, 1);
        }

        public void ApplyQuickFilter(IMatcher<TField>? filter) =>
            Grid.ApplyQuickFilter(filter);

        public EventHandler? BatchUpdateCompleted
        {
            get => Grid.BatchUpdateCompleted;
            set => Grid.BatchUpdateCompleted = value;
        }

        public TDAO? CurrentItem =>
            Grid.CurrentItem;

        public ItemsRootGrid<TField, TDAO> Grid = new();

        protected virtual void CurrentItemChangeHandler(object? sender, EventArgs e) 
        {
            if (DataManager.ListController<TField, TDAO>().AvailableBatchUpdate)
            {
                bool calcedBatchUpdateAvailable = Grid.SelectedRows.Count > 1;

                if (Grid.ToolBar.Actions[OxToolbarAction.Update].Enabled != calcedBatchUpdateAvailable)
                    Grid.ToolBar.Actions[OxToolbarAction.Update].Enabled = calcedBatchUpdateAvailable;
            }

            UpdateCurrentItemFullCard();
        }

        public virtual void ExecuteAction(OxToolbarAction action) =>
            Grid.ExecuteAction(action);

        public void FillGrid(bool force = false) =>
            Grid.Fill(null, force);

        public event EventHandler CurrentItemChanged
        {
            add => Grid.CurrentItemChanged += value;
            remove => Grid.CurrentItemChanged -= value;
        }

        public event EventHandler GridFillCompleted
        {
            add => Grid.GridFillCompleted += value;
            remove => Grid.GridFillCompleted -= value;
        }

        public void Renew() =>
            Grid.Renew();

        public RootListDAO<TField, TDAO> GetSelectedItems() =>
            Grid.GetSelectedItems();

        public void SelectFirstItem() =>
            Grid.SelectFirstItem();

        protected override void PrepareColors()
        {
            base.PrepareColors();
            SetPaneBaseColor(Grid, BaseColor);
        }

        private void UpdateCurrentItemFullCard()
        {
            if (CurrentInfoCard == null)
                return;

            CurrentInfoCard.Item = CurrentItem;
        }

        private static DAOSettings<TField, TDAO> Settings =>
            SettingsManager.DAOSettings<TField, TDAO>();

        private static DAOObserver<TField, TDAO> Observer =>
            Settings.Observer;

        public virtual void ApplySettings() 
        {

            if (Observer[DAOSetting.ShowItemInfo])
            {
                if (CurrentInfoCard != null && CurrentInfoCard.Visible != Settings.ShowItemInfo)
                    CurrentInfoCard.Visible = Settings.ShowItemInfo;
            }

            if (Observer[DAOSetting.ItemInfoPanelExpanded])
            {
                bool savedInfoCardPlaceExpanded = Settings.ItemInfoPanelExpanded;

                if (CurrentInfoCard != null && CurrentInfoCard.Expanded != savedInfoCardPlaceExpanded)
                    CurrentInfoCard.Expanded = savedInfoCardPlaceExpanded;
            }

            if (Observer.QuickFilterFieldsChanged
                    || Observer.QuickFilterTextFieldsChanged
                    || Observer.SortingFieldsChanged
                    || Observer.TableFieldsChanged)
                Renew();

            CurrentInfoCard?.ApplySettings();
        }

        public virtual void SaveSettings() =>
            CurrentInfoCard?.SaveSettings();

        public readonly IItemInfo<TField, TDAO>? CurrentInfoCard;
    }
}