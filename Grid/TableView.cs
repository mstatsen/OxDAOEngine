using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Settings;
using OxXMLEngine.View;

namespace OxXMLEngine.Grid
{
    public class TableView<TField, TDAO> : OxFrame
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TableView() : base() 
        {
            Borders.SetSize(OxSize.None);
            //Borders.TopOx = OxSize.Small;

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
            CurrentInfoCard.OnExpandedChanged += (s, e) => UpdateCurrentItemFullCard();
            CurrentInfoCard.VisibleChanged += (s, e) => UpdateCurrentItemFullCard();
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

        public ItemsGrid<TField, TDAO> Grid = new();

        protected virtual void CurrentItemChangeHandler(object? sender, EventArgs e) 
        {
            bool calcedBatchUpdateAvailable = Grid.SelectedRows.Count > 1;

            if (Grid.ToolBar.Actions[OxToolbarAction.Update].Enabled != calcedBatchUpdateAvailable)
                Grid.ToolBar.Actions[OxToolbarAction.Update].Enabled = calcedBatchUpdateAvailable;

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

            InfoCardLoadingPanel.StartLoading();

            try
            {
                if (CurrentInfoCard.Item == CurrentItem)
                    return;

                if (CurrentItem != null)
                {
                    CurrentInfoCard.Item = CurrentItem;
                    CurrentInfoCard.Borders.LeftOx = OxSize.None;
                }
            }
            finally
            {
                InfoCardLoadingPanel.EndLoading();
            }
        }

        private static DAOSettings<TField, TDAO> Settings =>
            SettingsManager.DAOSettings<TField, TDAO>();

        private static DAOObserver<TField, TDAO> Observer =>
            TableView<TField, TDAO>.Settings.Observer;

        public virtual void ApplySettings() 
        {

            if (TableView<TField, TDAO>.Observer[DAOSetting.ShowItemInfo])
            {
                if (CurrentInfoCard != null && CurrentInfoCard.Visible != TableView<TField, TDAO>.Settings.ShowItemInfo)
                    CurrentInfoCard.Visible = TableView<TField, TDAO>.Settings.ShowItemInfo;
            }

            if (TableView<TField, TDAO>.Observer[DAOSetting.ItemInfoPanelExpanded])
            {
                bool savedInfoCardPlaceExpanded = TableView<TField, TDAO>.Settings.ItemInfoPanelExpanded;

                if (CurrentInfoCard != null && CurrentInfoCard.Expanded != savedInfoCardPlaceExpanded)
                    CurrentInfoCard.Expanded = savedInfoCardPlaceExpanded;
            }

            if (TableView<TField, TDAO>.Observer.QuickFilterFieldsChanged
                    || TableView<TField, TDAO>.Observer.QuickFilterTextFieldsChanged
                    || TableView<TField, TDAO>.Observer.SortingFieldsChanged
                    || TableView<TField, TDAO>.Observer.TableFieldsChanged)
                Renew();

            CurrentInfoCard?.ApplySettings();
        }

        public virtual void SaveSettings() =>
            CurrentInfoCard?.SaveSettings();

        public readonly IItemInfo<TField, TDAO>? CurrentInfoCard;
        protected readonly OxLoadingPanel InfoCardLoadingPanel = new();
    }
}