using OxLibrary.Panels;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Controls
{
    public interface ICustomControl<TField, TDAO> : IOxPane
        where TField: Enum
        where TDAO : RootDAO<TField>, new()
    {
        Control Control { get; }
        bool ReadOnly { get; set; }
    }
}