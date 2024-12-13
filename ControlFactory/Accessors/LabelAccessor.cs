using OxLibrary.Controls;
using OxLibrary.Interfaces;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;
using OxLibrary;

namespace OxDAOEngine.ControlFactory.Accessors;

public class LabelAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
{
    public LabelAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
    protected override IOxControl CreateControl() =>
        new OxLabel
        {
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = OxB.T
        };

    protected override ValueAccessor CreateValueAccessor() =>
        new LabelValueAccessor();

    public override void Clear() => 
        Value = string.Empty;

    public override object? ObjectValue => ((LabelValueAccessor)ValueAccessor).ObjectValue;
}
