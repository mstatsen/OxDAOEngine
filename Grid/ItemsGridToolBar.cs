using OxLibrary.Controls;
using OxDAOEngine.Data;
using OxLibrary;

namespace OxDAOEngine.Grid
{
    public class ItemsGridToolBar<TField, TDAO> : OxToolBar<OxButton>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override void AfterCreated()
        {
            base.AfterCreated();
            CreateButtons();
            BorderVisible = OxB.F;
        }

        protected void CreateButtons()
        {
            AddButton(OxToolbarAction.New);

            IListController<TField, TDAO>? listController = null;

            if (DataManager.ListControllerExists<TField, TDAO>())
                listController = DataManager.ListController<TField, TDAO>();

            if (listController is not null && 
                listController.AvailableCopyItems)
                AddButton(OxToolbarAction.Copy);

            AddButton(OxToolbarAction.Edit, true);

            if (listController is not null 
                && listController.AvailableBatchUpdate)
                AddButton(OxToolbarAction.Update);

            AddButton(OxToolbarAction.Delete, true);
            AddButton(OxToolbarAction.Export, true, OxDock.Right);
        }

        protected override void SetToolBarPaddings() => 
            Padding.SetSize(4, 0, 2, 2);

        private OxBool actionsVisible = OxB.T;
        public OxBool ActionsVisible 
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