using OxLibrary.Controls;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Links;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Initializers
{
    public class LinkNameInitializer<TField, TDAO> : EmptyControlInitializer
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly Links<TField> existingLinks = new();
        private List<object>? existingItems;
        public List<object>? ExistingItems 
        { 
            get => existingItems;
            set
            {
                existingItems = value;
                FillExistingLinks();
            }
        }

        private void FillExistingLinks()
        {
            existingLinks.Clear();

            if (existingItems is null)
                return;

            foreach(object item in existingItems)
                if (item is Link<TField> link)
                    existingLinks.Add(link);
        }

        private void AddLinkNameToComboBox(string? linkName)
        {
            if (ComboBox!.Items.IndexOf(linkName) < 0
                && !existingLinks.Contains(l => l.Name.Equals(linkName)))
                ComboBox!.Items.Add(linkName);
        }

        private OxComboBox? ComboBox;

        public override void InitControl(Control control)
        {
            ComboBox = (OxComboBox)control;
            ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            ComboBox.Items.Clear();

            ILinkHelper<TField> linkHelper = TypeHelper.FieldHelper<TField>()!.GetLinkHelper()!; 

            foreach (object item in linkHelper.All())
                if (linkHelper.IsMandatoryLink(item))
                    AddLinkNameToComboBox(linkHelper.Name(item));
            
            List<object> linksNames = new FieldExtractor<TField, TDAO>(
                DataManager.FullItemsList<TField, TDAO>()).Extract(linkHelper.ExtractFieldName, true);

            foreach (object linkName in linksNames)
                AddLinkNameToComboBox(linkName.ToString());

            if (ComboBox.Items.Count > 0)
                ComboBox.SelectedIndex = 0;
        }

        public LinkNameInitializer() { }
    }
}