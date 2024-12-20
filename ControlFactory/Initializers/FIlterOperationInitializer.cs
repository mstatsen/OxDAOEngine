﻿using OxDAOEngine.ControlFactory.Controls.Fields;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Initializers
{
    public class FilterOperationInitializer<TField> : TypedComboBoxInitializer<FilterOperation>
        where TField : Enum
    {
        public TField Field { get; set; } = default!;
        public override bool AvailableValue(FilterOperation value) => 
            FieldHelper.AvailableFilterOperations[Field].Contains(value);

        private readonly FieldHelper<TField> FieldHelper = TypeHelper.FieldHelper<TField>();

        public override void InitControl(Control control)
        {
            base.InitControl(control);

            if (control is OxComboBox comboBox)
                comboBox.SelectedItem = FieldHelper.DefaultFilterOperation(Field);
        }

    }
}
