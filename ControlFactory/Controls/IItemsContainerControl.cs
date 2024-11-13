using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
{
    public interface IItemsContainerControl<TField, TDAO> : ICustomControl<TField, TDAO>, IWin32Window
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        OxPane ButtonsPanel { get; }
        OxPane ControlPanel { get; }
        IItemsContainer ItemsContainer { get; }
        void DisableValueChangeHandler();

        void EnableValueChangeHandler();
        EventHandler? ItemAdded { get; set; }
        EventHandler? ItemRemoved { get; set; }
    }
}