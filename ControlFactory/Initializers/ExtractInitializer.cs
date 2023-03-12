using OxXMLEngine.Data;
using OxXMLEngine.Data.Types;
using OxXMLEngine.Data.Extract;
using OxXMLEngine.Data.Filter;
using OxLibrary.Controls;

namespace OxXMLEngine.ControlFactory.Initializers
{
    public class ExtractInitializer<TField, TDAO> 
        : EmptyControlInitializer, IFilteredInitializer<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TField Field;
        private OxComboBox? ComboBox;

        public IMatcher<TDAO>? Filter { get; set; }

        public ExtractInitializer(TField field, bool fullExtract)
            : this(field, false, fullExtract, null) { }

        public ExtractInitializer(TField field, bool addAnyObject, bool fullExtract) 
            : this(field, addAnyObject, fullExtract, null) { }

        public ExtractInitializer(TField field, bool addAnyObject, bool fullExtract, IMatcher<TDAO>? filter)
        {
            Field = field;
            FullExtract = fullExtract;
            Filter = filter;
            AddAnyObject = addAnyObject;
        }

        private readonly bool AddAnyObject;

        protected IListController<TField, TDAO> ListController = 
            DataManager.ListController<TField, TDAO>();

        private RootListDAO<TField, TDAO> SourceItems => FullExtract
            ? ListController.FullItemsList
            : ListController.VisibleItemsList;

        public override void InitControl(Control? control)
        {
            if (control == null)
                return;

            RootListDAO<TField, TDAO> items = SourceItems; 

            if (items == null)
                return;

            items.ModifiedChangeHandler -= ListModifiedChangedHandler;
            ListController.ListChanged -= ListChangedHandler;

            if (ComboBox == null)
            {
                ComboBox = (OxComboBox)control;
                ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            }

            ComboBox.Items.Clear();

            if (AddAnyObject)
                ComboBox.Items.Add(TypeHelper.AnyObject);

            foreach (object extractItem in AvailableItems)
                ComboBox.Items.Add(
                    extractItem is Enum
                        ? TypeHelper.TypeObject(extractItem)
                        : extractItem);

            if (AddAnyObject)
                ComboBox.SelectedIndex = 0;

            items.ModifiedChangeHandler += ListModifiedChangedHandler;
            ListController.ListChanged += ListChangedHandler;
        }

        private void RenewControl()
        {
            object? selectedValue = ComboBox?.SelectedItem;
            InitControl(ComboBox);
            
            if (ComboBox != null)
                ComboBox.SelectedItem = selectedValue;
        }

        public FieldExtract AvailableItems => 
            new FieldExtractor<TField, TDAO>(SourceItems).Extract(Field, Filter, true, true);

        private void ListModifiedChangedHandler(DAO dao, bool Modified) => 
            RenewControl();

        private void ListChangedHandler(object? sender, EventArgs e) => 
            RenewControl();

        private readonly bool FullExtract;
    }
}
