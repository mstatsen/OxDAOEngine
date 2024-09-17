using System.Drawing;
using System.Windows.Forms;

namespace OxDAOEngine.ControlFactory.Controls
{
    public interface IColoredCustomControl
    {
        Color BackColor { get; set; }
        Control Control { get; }
        bool ReadOnly { get; set; }
        Color ControlColor { get; set; }
    }
}
