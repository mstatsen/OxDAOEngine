using OxDAOEngine.Data;
using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.Controls
{
    public interface ICustomControl<TField, TDAO> : IOxPanel
        where TField: Enum
        where TDAO : RootDAO<TField>, new()
    {
        Control Control { get; }
        bool ReadOnly { get; set; }
        EventHandler? ItemEdited { get; set; }
        TDAO? OwnerDAO { get; set; }
        ReadonlyMode ReadonlyMode { get; }
    }
}