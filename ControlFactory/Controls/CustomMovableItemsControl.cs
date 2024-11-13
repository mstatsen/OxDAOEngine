using OxLibrary;
using OxLibrary.Controls;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
{
    public abstract class CustomMovableItemsControl<TList, TItem, TItemsContainer, TEditor, TField, TDAO> 
        : CustomItemsContainerControl<TList, TItem, TItemsContainer, TEditor, TField, TDAO>
        where TList : ListDAO<TItem>, new ()
        where TItem : DAO, new ()
        where TItemsContainer : IItemsContainer, new()
        where TEditor : CustomItemEditor<TItem, TField, TDAO>, new ()
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly OxIconButton buttonUp = CreateButton(OxIcons.Up);
        private readonly OxIconButton buttonDown = CreateButton(OxIcons.Down);

        protected override void InitButtons()
        {
            base.InitButtons();
            buttonUp.ToolTipText = $"Move {ItemName().ToLower()} up";
            buttonDown.ToolTipText = $"Move {ItemName().ToLower()} down";
            PrepareEditButton(buttonUp, MoveUpHandler, true);
            PrepareEditButton(buttonDown, MoveDownHandler, true);
        }

        protected override void EnableControls()
        {
            base.EnableControls();
            buttonUp.Enabled = ItemsContainer.AvailableMoveUp;
            buttonDown.Enabled = ItemsContainer.AvailableMoveDown;
        }

        private void MoveUpHandler(object? sender, EventArgs e)
        {
            ItemsContainer.MoveUp();
            ValueChangeHandler?.Invoke(this, e);
        }

        private void MoveDownHandler(object? sender, EventArgs e)
        {
            ItemsContainer.MoveDown();
            ValueChangeHandler?.Invoke(this, e);
        }

        public bool AllowSorting
        {
            get => buttonDown.Visible;
            set
            {
                buttonDown.Visible = value;
                buttonUp.Visible = value;
            }
        }
    }
}