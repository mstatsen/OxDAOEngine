using OxDAOEngine.Data.Fields.Types;
using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.Controls.Fields
{
    public interface IFieldsPanel : IOxFrameWithHeader
    {
        List<object> Fields { get; set; }
        void ResetFields(FieldsFilling filling = FieldsFilling.Default);
        FieldsVariant Variant { get; }
    }
}