using OxLibrary;
using OxLibrary.Dialogs;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.BatchUpdate
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
                MainPanel.SetItemsCount();
            }
        }

        public EventHandler? BatchUpdateCompleted
        {
            get => MainPanel.BatchUpdateCompleted;
            set => MainPanel.BatchUpdateCompleted += value;
        }

        public override Bitmap FormIcon =>
            OxIcons.batch_edit;

        public new BatchUpdatePanel<TField, TDAO> MainPanel
        {
            get => (BatchUpdatePanel<TField, TDAO>)base.MainPanel;
            set => base.MainPanel = value;
        }

        public override bool CanOKClose()
        {
            if (MainPanel.FieldIsEmpty)
            {
                OxMessage.ShowError("Field is empty");
                return false;
            }

            MainPanel.UpdateItems();
            return base.CanOKClose();
        }

        protected override sealed OxFormMainPanel CreateMainPanel() =>
            new BatchUpdatePanel<TField, TDAO>(this);
    }
}