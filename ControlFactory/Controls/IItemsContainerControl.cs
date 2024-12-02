using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
{
    public interface IItemsContainerControl<TField, TDAO> : ICustomControl<TField, TDAO>, IWin32Window
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        OxPanel ButtonsPanel { get; }
        OxPanel ControlPanel { get; }
        IOxItemsContainer ItemsContainer { get; }
        void DisableValueChangeHandler();

        void EnableValueChangeHandler();
        EventHandler? ItemAdded { get; set; }
        EventHandler? ItemRemoved { get; set; }
    }
}