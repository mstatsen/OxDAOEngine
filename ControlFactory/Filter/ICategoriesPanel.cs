using OxDAOEngine.Data;
using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.Filter
{
    public interface ICategoriesPanel : IOxFrameWithHeader
    {
        IListDAO Categories { get; set; }

        void ResetToDefault();
    }
}
