using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Links;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class LinkButtonListValueAccessor<TField, TDAO> : ValueAccessor
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly List<LinkButtonAccessor<TField, TDAO>> ButtonsAccessors = new();

        private readonly IBuilderContext<TField, TDAO> Context;

        public LinkButtonListValueAccessor(IBuilderContext<TField, TDAO> context) : base() =>
            Context = context;

        private LinkButtonList LinkButtonList => (LinkButtonList)Control;

        public override object GetValue()
        {
            Links<TField> linkList = new();

            foreach (LinkButtonAccessor<TField, TDAO> accessor in ButtonsAccessors)
            {
                Link<TField>? value = (Link<TField>)accessor.Value!;
                linkList.Add(value);
            }
            
            return linkList;
        }

        public override void SetValue(object? value)
        {
            if (value == null || (value is not Links<TField> links))
                return;

            LinkButtonList.Clear();
            ButtonsAccessors.Clear();
            links.Sort();

            foreach (Link<TField> link in links)
            {
                LinkButtonAccessor<TField, TDAO> accessor = (LinkButtonAccessor<TField, TDAO>)Context.Builder
                    .Accessor( "Link", FieldType.Link, link.Url);
                accessor.Value = link;
                accessor.Parent = LinkButtonList;
                accessor.ButtonControl.BaseColor = link.LinkColor;
                ButtonsAccessors.Add(accessor);
                LinkButtonList.AddButton(accessor.ButtonControl);
            }

            return;
        }
    }
}