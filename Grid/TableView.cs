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
            Borders.Size = OxWh.W0;
            Grid.Parent = this;
            Grid.Dock = OxDock.Fill;
            Grid.ToolbarActionClick += (s, e) => ExecuteAction(e.Action);
            Grid.CurrentItemChanged += CurrentItemChangeHandler;
            InfoPanel = DataManager.ControlFactory<TField, TDAO>().CreateInfoPanel();
            Margin.Bottom = OxWh.W2;
            Margin.Right = OxWh.W4;
            PrepareInfoPanel();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UpdateCurrentItemFullCard();
        }

        private void PrepareInfoPanel()
        {
            if (InfoPanel is null)
                return;

            InfoPanel.Parent = this;
            InfoPanel.Size = new(500, 250);
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

                if (!Grid.ToolBar.Actions[OxToolbarAction.Update].Enabled.Equals(calcedBatchUpdateAvailable))
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
            Grid.BaseColor = BaseColor;
        }

        private void UpdateCurrentItemFullCard()
        {
            if (InfoPanel is null)
                return;

            InfoPanel.Item = CurrentItem;
            InfoPanel.ScrollToTop();
        }

        private static DAOSettings<TField, TDAO> Settings =>
            SettingsManager.DAOSettings<TField, TDAO>();

        private static DAOObserver<TField, TDAO> Observer =>
            Settings.Observer;

        public virtual void ApplySettings() 
        {
            if (Observer.QuickFilterFieldsChanged
                    || Observer.QuickFilterTextFieldsChanged
                    || Observer.SortingFieldsChanged
                    || Observer.TableFieldsChanged)
                Renew();

            InfoPanel?.ApplySettings();
            ReAlign();
        }

        public virtual void SaveSettings() =>
            InfoPanel?.SaveSettings();

        public readonly IItemInfo<TField, TDAO>? InfoPanel;
    }
}