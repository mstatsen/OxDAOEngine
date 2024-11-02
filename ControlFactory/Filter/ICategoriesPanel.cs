using OxLibrary.Panels;

namespace OxDAOEngine.ControlFactory.Filter
{
    public interface ICategoriesPanel : IOxFrameWithHeader
    {
        List<object> Categories { get; set; }
    }
}
