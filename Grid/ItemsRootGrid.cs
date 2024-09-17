using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.BatchUpdate;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Decorator;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Sorting;

namespace OxDAOEngine.Grid
{
    public partial class ItemsRootGrid<TField, TDAO> : ItemsGrid<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public ItemsRootGrid(GridUsage usage = GridUsage.Edit) : this(null, usage) { }

        public ItemsRootGrid(RootListDAO<TField, TDAO>? itemsList, GridUsage usage = GridUsage.Edit) : base(itemsList, usage)
        {
            ListController.ItemsSortChangeHandler += (s, e) => SortGrid();
            ListController.AddHandler += (d, e) => d.ChangeHandler += ItemChanged;
            GridView.DoubleClick += (s, e) => ExecuteAction(OxToolbarAction.Edit);
            GridView.KeyUp += GridView_KeyUp;
            GridView.ContextMenuStrip = new ItemsRootGridContextMenu<TField, TDAO>(this)
            {
                Enabled = Usage == GridUsage.Edit
            };
            Painter = ListController.ControlFactory.CreateGridPainter(GridFieldColumns, usage);
        }

        private void GridView_KeyUp(object? sender, KeyEventArgs e)
        {
            ExecuteAction(
                e.KeyCode switch
                {
                    Keys.Delete => OxToolbarAction.Delete,
                    Keys.Insert => OxToolbarAction.New,
                    Keys.Enter => OxToolbarAction.Edit,
                    _ => OxToolbarAction.Empty
                });
            e.Handled = true;
        }

        private void SetGridSorting()
        {
            GridView.ColumnSorting.Clear();

            foreach (FieldSorting<TField, TDAO> sorting in ListController.Settings.Sortings)
            {
                if (!GridFieldColumns.TryGetValue(sorting.Field, out var column))
                    continue;

                GridView.ColumnSorting.Add(column, sorting.SortOrder);
                column.HeaderCell.SortGlyphDirection = sorting.SortOrder;
            }
        }

        protected override void ApplySortigns(List<ISorting<TField, TDAO>> newSortings)
        {
            base.ApplySortigns(newSortings);

            if (Usage == GridUsage.Edit && CustomItemsList == null)
            {
                ListController.Settings.Sortings.SortingsList = newSortings;
                ListController.Sort();
            }
        }

        public override IRootListDAO<TField, TDAO> ItemsList =>
            CustomItemsList ?? (Usage == GridUsage.Edit
                    ? DataManager.VisibleItemsList<TField, TDAO>()
                    : DataManager.FullItemsList<TField, TDAO>());

        public void ExecuteAction(OxToolbarAction action)
        {
            if (ReadOnly)
            {
                if (action == OxToolbarAction.Edit && CurrentItem != null)
                    ListController.ViewItem(CurrentItem);

                return;
            }

            switch (action)
            {
                case OxToolbarAction.New:
                    ListController.AddItem();
                    break;
                case OxToolbarAction.Edit:
                    if (CurrentItem != null)
                        ListController.EditItem(CurrentItem); 
                    break;
                case OxToolbarAction.Copy:
                    if (CurrentItem != null)
                        ListController.CopyItem(CurrentItem);
                    break;
                case OxToolbarAction.Delete:
                    ListController.Delete(GetSelectedItems());
                    break;
                case OxToolbarAction.Update:
                    BatchUpdate();
                    break;
                case OxToolbarAction.Export:
                    ListController.ExportController.Export();
                    break;
            }
        }

        private BatchUpdateForm<TField, TDAO>? batchUpdateForm;

        private void BatchUpdate()
        {
            batchUpdateForm ??= new BatchUpdateForm<TField, TDAO>()
                {
                    ItemsGetter = GetSelectedItems,
                    BatchUpdateCompleted = (s, e) => BatchUpdateCompleted?.Invoke(s, e)
                };

            batchUpdateForm.ShowDialog();
        }

        public EventHandler? BatchUpdateCompleted;


        protected override bool IsAvailableColumn(TField field)
        {
            if (Fields != null)
                return Fields.Contains(field);

            return Usage switch
            {
                GridUsage.SelectItem or
                GridUsage.ChooseItems =>
                    fieldHelper.MandatoryFields.Contains(field),
                _ =>
                    ListController.Settings.TableFields.Fields.Contains(field),
            };
        }

        public override void Fill(IMatcher<TField>? filter = null, bool force = false)
        {
            SetGridSorting();
            base.Fill(filter, force);
        }

        private Decorator<TField, TDAO>? decorator;

        private Decorator<TField, TDAO> Decorator(TDAO item)
        {
            if (decorator == null || decorator.Dao != item)
                decorator = DecoratorFactory.Decorator(
                    DecoratorType.Table, item
                );

            return decorator;
        }

        protected override object? GetFieldValue(TField field, TDAO item) => Decorator(item)[field];

        private readonly IListController<TField, TDAO> ListController = 
            DataManager.ListController<TField, TDAO>();

        private readonly DecoratorFactory<TField, TDAO> DecoratorFactory = 
            DataManager.DecoratorFactory<TField, TDAO>();
    }
}