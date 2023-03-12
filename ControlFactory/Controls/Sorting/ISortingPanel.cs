using OxLibrary.Panels;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Sorting;

namespace OxXMLEngine.ControlFactory.Controls
{
    public interface ISortingPanel : IOxFrameWithHeader
    {
        void ResetToDefault();
        void RenewControls();
        SortingVariant Variant { get; }
        DAOEntityEventHandler? ExternalChangeHandler { get; set; }
    }
}