using OxXMLEngine.Data;

namespace OxXMLEngine.Grid
{
    public delegate DialogResult CanSelectEvent<TField, TDAO>(TDAO currentItem, RootListDAO<TField, TDAO> selectedList)
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new();

    public class ItemsChooserParams<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public string Title { get; set; } = "Selecting Items";
        public string AvailableTitle { get; set; } = "Available Items";
        public string SelectedTitle { get; set; } = "Selected Items";
        public Color BaseColor { get; set; } = EngineStyles.CardColor;

        public RootListDAO<TField, TDAO> InitialSelectedItems;
        public RootListDAO<TField, TDAO> AvailableItems;

        public CanSelectEvent<TField, TDAO>? CanSelectItem;
        public CanSelectEvent<TField, TDAO>? CanUnselectItem;

        public EventHandler? CompleteSelect;
        public EventHandler? CompleteUnselect;

        public List<TField>? AvailableGridFields { get; set; }
        public List<TField>? SelectedGridFields { get; set; }

        public List<CustomGridColumn<TField, TDAO>>? AvailableGridAdditionalColumns { get; set; }
        public List<CustomGridColumn<TField, TDAO>>? SelectedGridAdditionalColumns { get; set; }

        public ItemsChooserParams(RootListDAO<TField, TDAO> availableItems,
            RootListDAO<TField, TDAO> initialSelectedItems)
        {
            AvailableItems = availableItems;
            InitialSelectedItems = initialSelectedItems;
        }
    }
}