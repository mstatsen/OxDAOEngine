﻿using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Controls.Fields
{
    public class FieldsControl<TField, TDAO>
        : CustomMovableItemsControl<FieldColumns<TField>, FieldColumn<TField>, OxListBox, FieldEditor<TField, TDAO>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override string GetText() => "Fields";

        protected override string ItemName() => "Field";

        protected override int MaximumItemsCount =>
            TypeHelper.FieldHelper<TField>().AvailableFieldsCount(Context.Scope);
    }
}