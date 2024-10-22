using OxDAOEngine.ControlFactory.Controls.Links;
using OxDAOEngine.Data.Links;

namespace OxDAOEngine.ControlFactory.ValueAccessors
{
    public class LinkButtonValueAccessor<TField> : ValueAccessor
        where TField : notnull, Enum
    {
        private LinkButton LinkButton => (LinkButton)Control;
        public override object? GetValue() =>
            new Link<TField>() 
            { 
                Name = LinkButton.Text ?? string.Empty,
                Url = LinkButton.Url
            };

        public override void SetValue(object? value)
        {
            if (value == null || 
                (value is not Link<TField> link))
                return;

            LinkButton.Text = link.Name;
            LinkButton.Url = link.Url;
        }
    }
}