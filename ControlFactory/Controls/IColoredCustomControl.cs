using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.Controls;

public interface IColoredCustomControl
{
    Color BackColor { get; set; }
    IOxControl Control { get; }
    bool ReadOnly { get; set; }
    Color ControlColor { get; set; }
}