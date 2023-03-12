using System;
using System.Windows.Forms;

namespace OxXMLEngine.ControlFactory.Initializers
{
    public class TypedComboBoxInitializer<TItem> : ITypedComboBoxInitializer<TItem>
        where TItem : Enum
    {
        public virtual bool AvailableValue(TItem value) => true;

        public virtual void InitControl(Control control) { }

    }
}
