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
            Borders.TopOx = OxSize.Small;

            Grid.Parent = this;
            Grid.Dock = DockStyle.Fill;
            Grid.ToolbarActionClick += ToolBarActoinClickHandler;
            Grid.CurrentItemChanged += CurrentItemChangeHandler;

            InfoCardPlace = CreateInfoCardPlace();
            CurrentInfoCard = DataManager.ControlFactory<TField, TDAO>().CreateInfoCard();
            PrepareInfoCard();
            PrepareInfoCardLoadingPanel();
        }

        private void PrepareInfoCard()
        {
            if (CurrentInfoCard == null)
            {
                InfoCardPlace.Visible = false;
                return;
            }

            CurrentInfoCard.Parent = InfoCardPlace;
            CurrentInfoCard.Dock = DockStyle.Fill;
        }

        private void PrepareInfoCardLoadingPanel()
        {
            InfoCardLoadingPanel.Parent = InfoCardPlace;
            InfoCardLoadingPanel.UseParentColor = true;
            InfoCardLoadingPanel.Margins.SetSize(OxSize.None);
            InfoCardLoadingPanel.Borders.LeftOx = OxSize.None;
        }

        public void ApplyQuickFilter(IMatcher<TDAO>? filter) =>
            Grid.ApplyQuickFilter(filter);

        public EventHandler? BatchUpdateCompleted
        {
            get => Grid.BatchUpdateCompleted;
            set => Grid.BatchUpdateCompleted = value;
        }

        public TDAO? CurrentItem =>
            Grid.CurrentItem;

        public ItemsGrid<TField, TDAO> Grid = new();

        private void ToolBarActoinClickHandler(object? sender, ToolbarActionEventArgs EventArgs) =>
            ExecuteAction(EventArgs.Action);

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

        public bool InfoCardVisible
        {
            get => InfoCardPlace.Visible;
            set => InfoCardPlace.Visible = value;
        }

        private OxSidePanel CreateInfoCardPlace()
        {
            OxSidePanel result = new(new Size(500, 1))
            {
                Parent = this,
                Dock = DockStyle.Right,
                Expanded = false
            };

            result.Margins.SetSize(OxSize.Medium);
            result.Margins.LeftOx = OxSize.Large;
            result.Margins.TopOx = OxSize.Large;
            result.OnExpandedChanged += FullInfoCardPlaceExpandedChangedHandler;
            result.VisibleChanged += FullInfoCardPlaceVisibleChangedHandler;
            return result;
        }

        private void FullInfoCardPlaceVisibleChangedHandler(object? sender, EventArgs e) =>
            UpdateCurrentItemFullCard();

        private void FullInfoCardPlaceExpandedChangedHandler(object? sender, EventArgs e) =>
            UpdateCurrentItemFullCard();

        private void UpdateCurrentItemFullCard()
        {
            if (CurrentInfoCard == null)
                return;

            InfoCardPlace.BaseColor = new OxColorHelper(Grid.CurrentItemBackColor)
                .HDarker(SettingsManager.Settings<GeneralSettings>().DarkerHeaders ? 1 : 0).Darker(7);

            if (!InfoCardPlace.Visible
                || !InfoCardPlace.Expanded)
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
                if (InfoCardPlace.Visible != TableView<TField, TDAO>.Settings.ShowItemInfo)
                    InfoCardPlace.Visible = TableView<TField, TDAO>.Settings.ShowItemInfo;
            }

            if (TableView<TField, TDAO>.Observer[DAOSetting.ItemInfoPanelExpanded])
            {
                bool savedInfoCardPlaceExpanded = TableView<TField, TDAO>.Settings.GameInfoPanelExpanded;

                if (InfoCardPlace.Expanded != savedInfoCardPlaceExpanded)
                    InfoCardPlace.Expanded = savedInfoCardPlaceExpanded;
            }

            if (TableView<TField, TDAO>.Observer.QuickFilterFieldsChanged
                    || TableView<TField, TDAO>.Observer.QuickFilterTextFieldsChanged
                    || TableView<TField, TDAO>.Observer.SortingFieldsChanged
                    || TableView<TField, TDAO>.Observer.TableFieldsChanged)
                Renew();

            if (SettingsManager.Settings<GeneralSettings>().Observer[GeneralSetting.DarkerHeaders])
                InfoCardPlace.BaseColor = new OxColorHelper(Grid.CurrentItemBackColor)
                    .HDarker(SettingsManager.Settings<GeneralSettings>().DarkerHeaders ? 1 : 0).Darker(7);

            CurrentInfoCard?.ApplySettings();
        }

        public virtual void SaveSettings() => 
            SettingsManager.DAOSettings<TField>().GameInfoPanelExpanded = InfoCardPlace.Expanded;

        protected OxSidePanel InfoCardPlace;
        protected IItemView<TField, TDAO>? CurrentInfoCard;
        protected readonly OxLoadingPanel InfoCardLoadingPanel = new();
    }
}