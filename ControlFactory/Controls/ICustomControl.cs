using OxLibrary.Panels;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
{
    public interface ICustomControl<TField, TDAO> : IOxPane
        where TField: Enum
        where TDAO : RootDAO<TField>, new()
    {
        Control Control { get; }
        bool ReadOnly { get; set; }
    }
}