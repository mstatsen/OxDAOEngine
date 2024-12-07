using OxLibrary;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Decorator;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Statistic;

namespace OxDAOEngine.Grid
{
    public class ItemsViewer<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override Bitmap? GetIcon() => OxIcons.Eye;
        public readonly ItemsRootGrid<TField, TDAO> Grid;

        public IMatcher<TField>? Filter { get; set; }

        public ItemsViewer(RootListDAO<TField, TDAO>? itemList = null, GridUsage usage = GridUsage.ViewItems)
            : base(new(1024, OxWh.W768))
        {
            Grid = new ItemsRootGrid<TField, TDAO>(itemList, usage)
            {
                Parent = this,
                Dock = OxDock.Fill,
            };
            Grid.Padding.Size = OxWh.W0;
            statisticPanel = CreateStatisticPanel();
        }

        public List<TField>? Fields
        {
            get => Grid.Fields;
            set => Grid.Fields = value;
        }

        public IRootListDAO<TField, TDAO>? CustomItemsList
        {
            get => Grid.CustomItemsList;
            set => Grid.CustomItemsList = value;
        }

        public List<CustomGridColumn<TField, TDAO>>? AdditionalColumns
        {
            get => Grid.AdditionalColumns;
            set => Grid.AdditionalColumns = value;
        }

        public virtual void Fill()
        {
            Grid.Fill(Filter, true);
            statisticPanel.Renew();
        }

        private StatisticPanel<TField, TDAO> CreateStatisticPanel() =>
            new(Grid)
            {
                Dock = OxDock.Bottom,
                Parent = this
            };

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Text = "Select Item";
        }

        public override void PrepareColors()
        {
            base.PrepareColors();

            if (Grid is not null)
                Grid.BaseColor = Colors.Lighter();

            if (statisticPanel is not null)
                statisticPanel.BaseColor = BaseColor;
        }

        protected override void PrepareDialog(OxPanelViewer dialog)
        {
            base.PrepareDialog(dialog);
            dialog.Sizable = true;
            dialog.CanMaximize = true;
            dialog.Shown += DialogShownHandler;
        }

        private void DialogShownHandler(object? sender, EventArgs e) => 
            Fill();

        public bool SelectItem(Predicate<TDAO> match) => Grid.SelectItem(match);
        public bool SelectItem(TDAO? item) => Grid.SelectItem(item);
        public void ClearSelection() => Grid.GridView.ClearSelection();
        public void BeginUpdate() => Grid.BeginUpdate();
        public void EndUpdate() => Grid.EndUpdate();
        public IRootListDAO<TField, TDAO> ItemsList => Grid.ItemsList;

        public TField? InitialField { get; set; }
        public object? InitialValue { get; set; }

        public bool UseCustomCaption { get; set; } = false;

        protected override void PrepareDialogCaption(out string? dialogCaption)
        {
            if (UseCustomCaption 
                || InitialField is null)
            { 
                base.PrepareDialogCaption(out dialogCaption);
                return;
            }

            TDAO captionDao = new();
            captionDao[InitialField] = InitialValue;

            if (InitialValue is not null)
                InitialValue = 
                    TypeHelper.IsTypeHelpered(InitialValue)
                        ? TypeHelper.Name(InitialValue)
                        : DataManager.DecoratorFactory<TField, TDAO>()
                            .Decorator(DecoratorType.Table, captionDao)[InitialField];

            string? caption = 
                InitialValue is null 
                || string.Empty.Equals(InitialValue.ToString())
                    ? "is blank" 
                    : $"= <<{InitialValue}>>";

            IListController<TField, TDAO> listController = DataManager.ListController<TField, TDAO>();
            dialogCaption = $"{listController.ListName}, where {listController.FieldHelper.Name(InitialField)} {caption}";
        }

        private readonly StatisticPanel<TField, TDAO> statisticPanel;
    }
}