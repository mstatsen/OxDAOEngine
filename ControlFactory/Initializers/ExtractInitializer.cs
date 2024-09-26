using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Filter;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Initializers
{
    public class ExtractInitializer<TField, TDAO> 
        : EmptyControlInitializer, IFilteredInitializer<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TField Field;
        private OxComboBox? ComboBox;

        public IMatcher<TField>? Filter { get; set; }

        public ExtractInitializer(TField field, bool addAnyObject = false, bool fullExtract = false, bool fixedExtract = false, IMatcher<TField>? filter = null)
        {
            Field = field;
            FullExtract = fullExtract;
            Filter = filter;
            AddAnyObject = addAnyObject;
            FixedExtract = fixedExtract;
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
                ComboBox.DropDownStyle = 
                    FixedExtract 
                        ? ComboBoxStyle.DropDownList 
                        : ComboBoxStyle.DropDown;
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

        private void ListModifiedChangedHandler(DAO dao, DAOModifyEventArgs e) => 
            RenewControl();

        private void ListChangedHandler(object? sender, EventArgs e) => 
            RenewControl();

        private readonly bool FullExtract;
        private readonly bool FixedExtract;
    }
}