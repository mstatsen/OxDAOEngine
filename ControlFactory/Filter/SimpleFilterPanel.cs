using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Filter.Types;
using OxLibrary.Panels;

namespace OxDAOEngine.ControlFactory.Filter
{
    public class SimpleFilterPanel<TField, TDAO> : OxPane
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly int Number;
        private readonly int GroupNumber;
        private readonly ControlBuilder<TField, TDAO> Builder;
        private readonly IControlAccessor FieldControl;
        private readonly IControlAccessor OperationControl;
        public SimpleFilter<TField, TDAO> Rule 
        {
            get => GrabRule();
            set => FillRule(value);
        }

        private void FillRule(SimpleFilter<TField, TDAO> value)
        {
            if (value.Rules.Count == 0)
            {
                FieldControl.Value = default!;
                OperationControl.Value = default!;
            }
            else
            {
                FieldControl.Value = value.Rules.First!.Field;
                OperationControl.Value = value.Rules.First!.Operation;
            }
        }

        private SimpleFilter<TField, TDAO> GrabRule()
        {
            SimpleFilter<TField, TDAO> result = new();
            FilterRule<TField> rule = result.Rules.Add(
                (TField)FieldControl.Value!,
                OperationControl.EnumValue<FilterOperation>()
            );

            //TODO: grab field value into rule

            return result;
        }

        public SimpleFilterPanel(SimpleFilter<TField, TDAO> rule, ControlBuilder<TField, TDAO> builder, int groupNumber, int number) : base(new Size(1, 32))
        { 
            Builder = builder;
            GroupNumber = groupNumber;
            Number = number;
            Dock = DockStyle.Top;
            FieldControl = CreateSimpleControl("SimpleFilter:Field", FieldType.MetaData, 24, 160);
            FieldControl.ValueChangeHandler += FieldControlValueChangeHandler;
            OperationControl = CreateSimpleControl("SimpleFilter:Operation", FieldType.Enum, 188, 110);
            Rule = rule;
        }

        private void FieldControlValueChangeHandler(object? sender, EventArgs e)
        {
            //TODO: renew OperationControl with initializer
        }

        private IControlAccessor CreateSimpleControl(string key, FieldType fieldType, int left, int width)
        {
            IControlAccessor result = Builder.Accessor(
                key,
                fieldType,
                $"{GroupNumber}_{Number}"
            );
            result.Parent = this;
            result.Left = left;
            result.Top = 4;
            result.Width = width;
            result.Height = 24;
            return result;
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (FieldControl != null)
                ControlPainter.ColorizeControl(FieldControl, BaseColor);

            if (OperationControl != null)
                ControlPainter.ColorizeControl(OperationControl, BaseColor);
        }
    }
}
