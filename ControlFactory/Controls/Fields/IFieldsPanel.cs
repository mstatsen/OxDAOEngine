using OxLibrary.Panels;
using OxDAOEngine.Data.Fields;

namespace OxDAOEngine.ControlFactory.Controls.Fields
{
    public interface IFieldsPanel : IOxFrameWithHeader
    {
        List<object> Fields { get; set; }
        void ResetFields(FieldsFilling filling = FieldsFilling.Default);
        FieldsVariant Variant { get; }
    }
}