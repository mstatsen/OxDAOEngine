using OxLibrary;
using OxLibrary.Dialogs;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.BatchUpdate
{
    public class BatchUpdateForm<TField, TDAO> : OxDialog
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new ()
    {
        public GetListEvent<TField, TDAO>? ItemsGetter
        {
            get => MainPanel.ItemsGetter;
            set
            {
                MainPanel.ItemsGetter += value;
                SetItemsCount();
            }
        }

        private void SetItemsCount() => 
            MainPanel.SetItemsCount();

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Size = new(360, 120);
            SetItemsCount();
        }

        public EventHandler? BatchUpdateCompleted
        {
            get => MainPanel.BatchUpdateCompleted;
            set => MainPanel.BatchUpdateCompleted += value;
        }

        public override Bitmap FormIcon =>
            OxIcons.Batch_edit;

        public new BatchUpdatePanel<TField, TDAO> MainPanel
        {
            get => (BatchUpdatePanel<TField, TDAO>)base.MainPanel;
            set => base.MainPanel = value;
        }

        public override bool CanOKClose()
        {
            if (MainPanel.FieldIsEmpty)
            {
                OxMessage.ShowError("Field is empty", this);
                return false;
            }

            MainPanel.UpdateItems();
            return base.CanOKClose();
        }

        protected override sealed OxFormMainPanel CreateMainPanel() =>
            new BatchUpdatePanel<TField, TDAO>(this);
    }
}