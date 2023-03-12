using OxLibrary;
using OxLibrary.Controls;

namespace OxXMLEngine.Grid
{
    public class ItemsGridToolBar : OxToolBar
    {
        protected override void AfterCreated()
        {
            base.AfterCreated();
            CreateButtons();
            BorderVisible = false;
        }

        protected virtual void CreateButtons()
        {
            AddButton(OxToolbarAction.New);
            AddButton(OxToolbarAction.Copy);
            AddButton(OxToolbarAction.Edit, true);
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
    }
}