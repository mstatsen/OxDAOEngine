using OxLibrary;
using OxLibrary.Controls;
using OxXMLEngine.Data;

namespace OxXMLEngine.Grid
{
    public class ItemsGridToolBar<TField, TDAO> : OxToolBar
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override void AfterCreated()
        {
            base.AfterCreated();
            CreateButtons();
            BorderVisible = false;
        }

        protected void CreateButtons()
        {
            AddButton(OxToolbarAction.New);

            if (DataManager.ListController<TField, TDAO>().AvailableCopyItems)
                AddButton(OxToolbarAction.Copy);

            AddButton(OxToolbarAction.Edit, true);

            if (DataManager.ListController<TField, TDAO>().AvailableBatchUpdate)
                AddButton(OxToolbarAction.Update);

            AddButton(OxToolbarAction.Delete, true);
            AddButton(OxToolbarAction.Export, true, DockStyle.Right);
        }

        protected override void SetToolBarPaddings()
        {
            Paddings.SetSize(OxSize.Medium);
            Paddings.LeftOx = OxSize.None;
            Paddings.TopOx = OxSize.Large;
        }

        private bool actionsVisible = true;
        public bool ActionsVisible 
        { 
            get => actionsVisible;
            set
            {
                actionsVisible = value;

                foreach (OxButton button in Actions.Values)
                    button.Visible = actionsVisible;
            }
        }
    }
}