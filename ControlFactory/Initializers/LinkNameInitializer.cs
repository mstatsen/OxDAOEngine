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
        private readonly Links<TField>? ExistingLinks;

        private void AddLinkNameToComboBox(string? linkName)
        {
            if (ComboBox!.Items.IndexOf(linkName) < 0
                && (ExistingLinks == null ||
                        !ExistingLinks.Contains(l => l.Name == linkName)))
                ComboBox!.Items.Add(linkName);
        }

        private OxComboBox? ComboBox;

        public override void InitControl(Control control)
        {
            ComboBox = (OxComboBox)control;
            ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            ComboBox.Items.Clear();
            AddLinkNameToComboBox("Stratege");
            AddLinkNameToComboBox("PSNProfiles");

            List<object> linksNames = new FieldExtractor<TField, TDAO>(
                DataManager.FullItemsList<TField, TDAO>()).Extract(TypeHelper.FieldHelper<TField>()!.GetFirstLinksField(), true);

            foreach (object linkName in linksNames)
                AddLinkNameToComboBox(linkName.ToString());

            if (ComboBox.Items.Count > 0)
                ComboBox.SelectedIndex = 0;
        }

        public LinkNameInitializer(Links<TField>? existingLinks) =>
            ExistingLinks = existingLinks;
    }
}