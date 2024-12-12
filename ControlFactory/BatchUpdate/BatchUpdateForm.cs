using OxLibrary;
using OxLibrary.Forms;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.BatchUpdate
{
    public class BatchUpdateForm<TField, TDAO> :
        OxDialog<BatchUpdatePanel<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new ()
    {
        public GetListEvent<TField, TDAO>? ItemsGetter
        {
            get => FormPanel.ItemsGetter;
            set
            {
                FormPanel.ItemsGetter += value;
                SetItemsCount();
            }
        }

        private void SetItemsCount() =>
            FormPanel.SetItemsCount();

        protected override void OnShown(EventArgs e)
        {
            Size = new(360, 120);
            SetItemsCount();
            base.OnShown(e);
        }

        public EventHandler? BatchUpdateCompleted
        {
            get => FormPanel.BatchUpdateCompleted;
            set => FormPanel.BatchUpdateCompleted += value;
        }

        public override Bitmap FormIcon =>
            OxIcons.Batch_edit;

        public override bool CanOKClose()
        {
            if (FieldIsEmpty)
            {
                OxMessage.ShowError("Field is empty", this);
                return false;
            }

            UpdateItems();
            return base.CanOKClose();
        }

        private void UpdateItems() =>
            FormPanel.UpdateItems();

        public bool FieldIsEmpty => FormPanel.FieldIsEmpty;
    }
}