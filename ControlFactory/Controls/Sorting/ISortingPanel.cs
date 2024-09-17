using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Sorting;

namespace OxDAOEngine.ControlFactory.Controls
{
    public interface ISortingPanel : IOxFrameWithHeader
    {
        void ResetToDefault();
        void RenewControls();
        SortingVariant Variant { get; }
        DAOEntityEventHandler? ExternalChangeHandler { get; set; }
    }
}