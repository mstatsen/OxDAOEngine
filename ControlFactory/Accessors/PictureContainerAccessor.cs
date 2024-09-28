using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class PictureContainerAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public PictureContainerAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        protected override Control CreateControl() =>
            new OxPictureContainer();

        protected override ValueAccessor CreateValueAccessor() =>
            new PictureContainerValueAccessor();

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) { }
        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) { }

        public override void Clear() =>
            Value = null;

        protected override Control? CreateReadOnlyControl() => null;
    }
}
