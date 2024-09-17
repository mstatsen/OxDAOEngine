using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
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