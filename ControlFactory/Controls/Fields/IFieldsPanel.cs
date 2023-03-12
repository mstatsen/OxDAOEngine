using OxLibrary.Panels;
using OxXMLEngine.Data.Fields;

namespace OxXMLEngine.ControlFactory.Controls
{
    public interface IFieldsPanel : IOxFrameWithHeader
    {
        List<object> Fields { get; set; }
        void ResetFields(FieldsFilling filling = FieldsFilling.Default);
        FieldsVariant Variant { get; }
    }
}