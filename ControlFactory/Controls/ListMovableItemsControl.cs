using OxLibrary;
using OxLibrary.Controls;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
{
    public abstract class ListMovableItemsControl<TList, TItem, TEditor, TField, TDAO> 
        : ListItemsControl<TList, TItem, TEditor, TField, TDAO>
        where TList : ListDAO<TItem>, new ()
        where TItem : DAO, new ()
        where TEditor : ListItemEditor<TItem, TField, TDAO>, new ()
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
            buttonUp.Enabled = ListBox.SelectedIndex > 0;
            buttonDown.Enabled = (ListBox.SelectedIndex > -1) 
                && (ListBox.SelectedIndex < ListBox.Items.Count - 1);
        }

        private void MoveUpHandler(object? sender, EventArgs e)
        {
            ListBox.MoveUp();
            ValueChangeHandler?.Invoke(this, e);
        }

        private void MoveDownHandler(object? sender, EventArgs e)
        {
            ListBox.MoveDown();
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
