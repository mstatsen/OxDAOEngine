using OxLibrary.Panels;

namespace OxXMLEngine.Summary
{
    public interface ISummaryPanel : IOxCard
    {
        void AlignAccessors();
        void ClearAccessors();
        void CalcPanelSize();
        void FillAccessors();
    }
}