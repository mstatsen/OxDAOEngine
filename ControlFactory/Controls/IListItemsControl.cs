using OxLibrary.Controls;
using OxLibrary.Panels;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Controls
{
    public interface IListItemsControl<TField, TDAO> : ICustomControl<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        OxPane ButtonsPanel { get; }
        OxPane ControlPanel { get; }
        OxListBox ListBox { get; }
        void DisableValueChangeHandler();

        void EnableValueChangeHandler();
    }
}