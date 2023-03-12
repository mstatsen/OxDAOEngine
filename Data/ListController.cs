using OxLibrary.Dialogs;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory;
using OxXMLEngine.Data.Decorator;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Editor;
using OxXMLEngine.Grid;
using OxXMLEngine.Settings;
using OxXMLEngine.Summary;
using OxXMLEngine.View;
using System.Xml;

namespace OxXMLEngine.Data
{
    public abstract class ListController<TField, TDAO, TFieldGroup, TListController> 
        : IListController<TField, TDAO, TFieldGroup>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TFieldGroup : notnull, Enum
        where TListController : ListController<TField, TDAO, TFieldGroup, TListController>, new()
    {
        private readonly RootListDAO<TField, TDAO> fullItemsList;
        public RootListDAO<TField, TDAO> FullItemsList => fullItemsList;
        public bool Modified => FullItemsList.Modified;

        public ListController()
        {
            fieldHelper = RegisterFieldHelper();
            fieldGroupHelper = RegisterFieldGroupHelper();
            RegisterHelpers();
            Settings = new DAOSettings<TField, TDAO>();
            fullItemsList = new();
            SetListHandlers();
        }

        private void AfterLoad() =>
            OnAfterLoad?.Invoke(this, EventArgs.Empty);

        protected virtual void AfterSave() { }

        protected virtual void BeforeLoad() { }

        protected virtual void BeforeSave() { }

        public void Save(XmlElement? parentElement)
        {
            BeforeSave();
            FullItemsList.Save(parentElement);
            Sort();
            AfterSave();
        }

        public void Load(XmlElement? parentElement)
        {
            BeforeLoad();
            FullItemsList.Load(parentElement);
            AfterLoad();
        }

        protected void NotifyAll()
        {
            if (FullItemsList.Loading)
                return;

            ProcessNotifyAll();
        }

        protected virtual void ProcessNotifyAll()
        {
            Sort();
            ListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Sort() =>
            FullItemsList.Sort(Settings.Sortings.SortingsList, true);

        protected virtual void SetListHandlers()
        {
            FullItemsList.ChangeHandler += ListChangedHandler;
            FullItemsList.ModifiedChangeHandler += ListModifiedChangeHandler;
            FullItemsList.ItemAddHandler += ListAddHandler;
            FullItemsList.ItemRemoveHandler += ListRemoveHandler;
            FullItemsList.SortChangeHandler += ItemListSortChanger;
        }

        private void ItemListSortChanger(object? sender, EventArgs e)
        {
            RenewVisibleItems();
            ItemsSortChangeHandler?.Invoke(this, e);
        }

        private void ListAddHandler(DAO dao, DAOEntityEventArgs e) =>
            AddHandler?.Invoke(dao, e);

        private void ListRemoveHandler(DAO dao, DAOEntityEventArgs e) =>
            RemoveHandler?.Invoke(dao, e);

        protected virtual void ListModifiedChangeHandler(DAO dao, bool Modified)
        {
            Sort();
            RenewListsAndNotifyAll();
            ModifiedHandler?.Invoke(dao, Modified);

            if (!Modified)
            {
                deletedCount = 0;
                addedCount = 0;
            }
        }

        private void ListChangedHandler(DAO dao, DAOEntityEventArgs? e) =>
            RenewListsAndNotifyAll();

        private Category<TField, TDAO>? category;

        public Category<TField, TDAO>? Category
        {
            get => category;
            set
            {
                category = value;
                CategoryChanged?.Invoke(category, EventArgs.Empty);
                RenewListsAndNotifyAll();
            }
        }

        public event EventHandler? CategoryChanged;

        protected void RenewListsAndNotifyAll()
        {
            if (RenewVisibleItems())
                NotifyAll();
        }

        private RootListDAO<TField, TDAO>? visibleItemsList;

        public RootListDAO<TField, TDAO> VisibleItemsList
        {
            get
            {
                if (visibleItemsList == null)
                    RenewVisibleItems();

                return visibleItemsList ?? new();
            }
        }

        public bool RenewVisibleItems()
        {
            RootListDAO<TField, TDAO> newVisibleItems = FullItemsList.FilteredList(Category, 
                Settings.Sortings.SortingsList);

            if (newVisibleItems.Equals(visibleItemsList))
                return false;

            visibleItemsList = newVisibleItems;
            return true;
        }

        private void EditNewItem(TDAO item)
        {
            if (GetItemEditor(item).ShowDialog() == DialogResult.OK)
            {
                addedCount++;
                FullItemsList.NotifyAboutItemAdded(item);
                item.NotifyAll(DAOOperation.Insert);
                FullItemsList.Add(item);
                RenewVisibleItems();
                Sort();
                ItemFieldChanged?.Invoke(item, new DAOEntityEventArgs(DAOOperation.Insert));
            }
        }

        public void AddItem() => 
            EditNewItem(new());

        public void EditItem(TDAO? item)
        {
            if (item == null)
                return;

            if (GetItemEditor(item).ShowDialog() == DialogResult.OK)
            {
                RenewListsAndNotifyAll();
                ItemFieldChanged?.Invoke(item, new DAOEntityEventArgs(DAOOperation.Modify));
            }
        }

        private ItemSelector<TField, TDAO>? itemSelector;

        public bool SelectItem(out TDAO? selectedItem, OxForm parentForm, TDAO? initialItem = null, IMatcher<TDAO>? filter = null)
        {
            if (itemSelector == null)
                itemSelector = new();

            itemSelector.BaseColor = parentForm.MainPanel.BaseColor;
            itemSelector.Filter = filter;
            itemSelector.SelectedItem = initialItem;

            bool result = itemSelector.ShowAsDialog(OxDialogButton.OK | OxDialogButton.Cancel) == DialogResult.OK;
            selectedItem = result ? itemSelector.SelectedItem : null;
            return result;
        }

        public void CopyItem(TDAO? item)
        {
            if (item == null)
                return;

            TDAO newItem = new();
            newItem.CopyFrom(item, true);
            EditNewItem(newItem);
        }

        public void ViewItem(TField field, object value, ItemViewMode viewMode = ItemViewMode.Simple) =>
            ViewItem(Item(field, value), viewMode);

        public void ViewItem(TDAO? item, ItemViewMode viewMode = ItemViewMode.Simple)
        {
            if (item == null)
                return;

            IItemCard<TField, TDAO>? card = ControlFactory.CreateCard(viewMode);

            if (card == null)
            {
                EditItem(item);
                return;
            }

            card.Item = item;
            card.ShowAsDialog();
        }

        private DAOEditor<TField, TDAO, TFieldGroup>? editor;

        public DAOEditor<TField, TDAO, TFieldGroup> Editor
        {
            get
            {
                if (editor == null)
                    editor = CreateEditor();

                return editor;
            }
        }

        private DAOEditor<TField, TDAO, TFieldGroup> GetItemEditor(TDAO item)
        {
            Editor.Item = item;
            return Editor;
        }


        protected abstract DAOEditor<TField, TDAO, TFieldGroup> CreateEditor();
        protected abstract DAOWorker<TField, TDAO, TFieldGroup> CreateWorker();

        private DAOWorker<TField, TDAO, TFieldGroup>? worker;
        public DAOWorker<TField, TDAO, TFieldGroup> Worker
        {
            get
            {
                if (worker == null)
                    worker = CreateWorker();

                return worker;
            }
        }

        public int TotalCount => FullItemsList.Count;
        public int FilteredCount => VisibleItemsList.Count;
        public int ModifiedCount => FullItemsList.ModifiedCount;

        public int DeletedCount => deletedCount;

        public int AddedCount => addedCount;

        private int deletedCount = 0;
        private int addedCount = 0;

        public void Delete(RootListDAO<TField, TDAO> list)
        {
            string messageBase = "Are you sure you want to delete selected";
            string messageSuffix = list.Count > 1 ? $"({list.Count}) items" : "item";

            if (!OxMessage.Confirmation($"{messageBase} {messageSuffix}?"))
                return;

            foreach (TDAO item in list)
                item.NotifyAll(DAOOperation.Delete);

            FullItemsList.StartSilentChange();

            foreach (TDAO item in list)
                if (FullItemsList.Remove(item))
                    deletedCount++;

            FullItemsList.FinishSilentChange();
            RenewVisibleItems();
        }

        protected virtual void RegisterHelpers() { }

        protected string GetFileName() => $"{Name}.xml";
        public string FileName => GetFileName();

        public abstract string Name { get; }

        public static void Init()
        {
            DataManager.Init();
            TListController instance = (TListController)DataManager.Register<TField>(new TListController());
            SettingsManager.Register<TField>(instance.Settings);
        }

        public DAOSettings<TField, TDAO> Settings { get; private set; }

        public IDAOSettings<TField> SettingsByField => SettingsManager.DAOSettings<TField>();

        public TDAO? Item(TField field, object? value) =>
            FullItemsList.Find((d) => (d[field] != null) && d[field]!.Equals(value));

        private readonly FieldHelper<TField> fieldHelper;

        protected abstract FieldHelper<TField> RegisterFieldHelper();

        public FieldHelper<TField> FieldHelper => fieldHelper;

        private readonly FieldGroupHelper<TField, TFieldGroup> fieldGroupHelper;

        protected abstract FieldGroupHelper<TField, TFieldGroup> RegisterFieldGroupHelper();

        public FieldGroupHelper<TField, TFieldGroup> FieldGroupHelper => fieldGroupHelper;

        public DecoratorFactory<TField, TDAO>? decoratorFactory;

        public DecoratorFactory<TField, TDAO> DecoratorFactory
        {
            get
            {
                if (decoratorFactory == null)
                    decoratorFactory = CreateDecoratorFactory();

                return decoratorFactory;
            }
        }

        public ControlFactory<TField, TDAO>? controlFactory;

        public ControlFactory<TField, TDAO> ControlFactory
        {
            get
            {
                if (controlFactory == null)
                    controlFactory = CreateControlFactory();

                return controlFactory;
            }
        }

        public DAOEntityEventHandler? AddHandler { get; set; }
        public DAOEntityEventHandler? RemoveHandler { get; set; }
        public ModifiedChangeHandler? ModifiedHandler { get; set; }
        public EventHandler? ListChanged { get; set; }
        public EventHandler? OnAfterLoad { get; set; }

        public event EventHandler? ItemsSortChangeHandler;
        public DAOEntityEventHandler? ItemFieldChanged { get; set; } 

        public bool IsSystem => false;

        protected abstract DecoratorFactory<TField, TDAO> CreateDecoratorFactory();

        protected abstract ControlFactory<TField, TDAO> CreateControlFactory();

        public virtual FieldSortings<TField, TDAO>? DefaultSorting() => null;

        public virtual Categories<TField, TDAO>? SystemCategories => null;

        public ItemsFace<TField, TDAO> Face
        {
            get
            { 
                if (face == null)
                    face = new ItemsFace<TField, TDAO>();

                return face;
            }
        }

        public SettingsPart ActiveSettingsPart => Face.ActiveSettingsPart;

        private ItemsFace<TField, TDAO>? face;
        ISettingsController IDataController.Settings => Settings;
        OxPane? IDataController.Face => Face;

        public virtual List<ISummaryPanel>? GeneralSummaries => null;
    }
}