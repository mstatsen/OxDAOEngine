using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Controls.Links;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class LinkButtonListAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public LinkButtonListAccessor(IBuilderContext<TField, TDAO> context, ButtonListDirection buttonListDirection)
            : base(context) =>
            LinkButtonListControl.Direction = buttonListDirection;

        protected override Control CreateControl() =>
            new LinkButtonList();

        public override void Clear() =>
            LinkButtonListControl.Clear();

        public LinkButtonList LinkButtonListControl =>
            (LinkButtonList)Control;

        protected override ValueAccessor CreateValueAccessor() =>
            new LinkButtonListValueAccessor<TField, TDAO>(Context);

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) { }

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) { }
    }
}