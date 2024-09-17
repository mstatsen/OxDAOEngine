using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class YearAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public YearAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        protected override void InitControl()
        {
            base.InitControl();
            ComboBox.Items.Clear();

            for (int y = DateTime.Today.Year; y > 1990; y--)
                ComboBox.Items.Add(y);
        }
    }
}
