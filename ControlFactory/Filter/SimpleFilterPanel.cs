﻿using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxLibrary.Controls;
using OxLibrary;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;

namespace OxDAOEngine.ControlFactory.Filter
{
    public class SimpleFilterPanel<TField, TDAO> : OxPane
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public readonly int Number;
        private readonly int GroupNumber;
        private readonly ControlBuilder<TField, TDAO> Builder;
        private readonly FieldAccessor<TField, TDAO> FieldControl = default!;
        private readonly IControlAccessor OperationControl;
        private IControlAccessor? ValueAccessor;

        private readonly OxIconButton RemoveRuleButton = new(OxIcons.Minus, 20);

        public SimpleFilter<TField, TDAO> Rule 
        {
            get => GrabRule();
            set => FillRule(value);
        }

        public EventHandler? RemoveRule;

        private void PrepareRemoveRuleButton()
        {
            RemoveRuleButton.Parent = this;
            RemoveRuleButton.ToolTipText = "Remove rule";
            RemoveRuleButton.Top = 4;
            RemoveRuleButton.Left = Width - RemoveRuleButton.Width - 1;
            RemoveRuleButton.Anchor = AnchorStyles.Right;
            RemoveRuleButton.Click += RemoveRuleButtonClickHandler;
        }

        private void RemoveRuleButtonClickHandler(object? sender, EventArgs e) =>
            RemoveRule?.Invoke(this, EventArgs.Empty);

        private void FillRule(SimpleFilter<TField, TDAO> value)
        {
            if (value.Rules.Count == 0)
                FieldControl.Value = ((OxComboBox)FieldControl.Control).Items[0];
            else
            {
                FieldControl.Value = value.Rules.First!.Field;
                OperationControl.Value = value.Rules.First!.Operation;
            }

            LayoutValueControl();

            if (ValueAccessor != null)
                ValueAccessor.Value = value.GetFieldValue(FieldControl.EnumValue);
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

        public SimpleFilterPanel(SimpleFilter<TField, TDAO> rule, ControlBuilder<TField, TDAO> builder, int groupNumber, int number) 
            : base(new Size(1, 32))
        { 
            Builder = builder;
            GroupNumber = groupNumber;
            Number = number;
            Dock = DockStyle.Top;
            FieldControl = (FieldAccessor<TField,TDAO>)
                CreateSimpleControl("SimpleFilter:Field", FieldType.MetaData, 24, 160);
            FieldControl.ValueChangeHandler += FieldControlValueChangeHandler;
            OperationControl = CreateSimpleControl("SimpleFilter:Operation", FieldType.Enum, 188, 110);
            PrepareRemoveRuleButton();
            Rule = rule;
        }

        private readonly FieldHelper<TField> FieldHelper = TypeHelper.FieldHelper<TField>();

        public bool FieldIsEmpty =>
            FieldControl.Value == null
            || (FieldControl.Value is IEmptyChecked ec && ec.IsEmpty);

        private void FieldControlValueChangeHandler(object? sender, EventArgs e)
        {
            OperationControl.Value = FieldHelper.DefaultFilterOperation((TField)FieldControl.Value!);
            //TODO: renew OperationControl with initializer

            LayoutValueControl();
        }

        private void LayoutValueControl()
        {
            HideValueControl();

            if (FieldIsEmpty)
                return;

            ValueAccessor = CreateValueAccessor(
                FieldControl.EnumValue,
                OperationControl.Right + 4,
                220);
            ValueAccessor.Control.Parent = this;
            ValueAccessor.Control.Top = OperationControl.Top;

            ValueAccessor.Control.Height = 
                FieldHelper.GetFieldType(FieldControl.EnumValue) switch
                {
                    FieldType.Boolean or
                    FieldType.List or
                    FieldType.Integer =>
                        26,
                    _ =>
                        22,
                };
            
            ValueAccessor.Visible = true;
            PrepareColors();
        }

        private void HideValueControl()
        {
            if (ValueAccessor == null)
                return;

            ValueAccessor.Control.Parent = null;
            ValueAccessor.Control.Visible = false;
        }

        private IControlAccessor CreateValueAccessor(TField field, int left, int width) => 
            CreateSimpleControl(
                "SimpleFilter:Value", 
                FieldHelper.GetFieldType(field), 
                left, 
                width,
                new FieldAdditionalContext<TField>(field, DefaultAdditinalContext));

        private string DefaultAdditinalContext => $"{GroupNumber}_{Number}";

        private IControlAccessor CreateSimpleControl(string key, FieldType fieldType, int left, int width, 
            object? additionalContext = null)
        {
            IControlAccessor result = Builder.Accessor(
                key,
                fieldType,
                additionalContext ?? DefaultAdditinalContext
            );
            result.Parent = this;
            result.Left = left;
            result.Top = 4;
            result.Width = width;
            result.Height = 22;
            return result;
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (FieldControl != null)
                ControlPainter.ColorizeControl(FieldControl, BaseColor);

            if (OperationControl != null)
                ControlPainter.ColorizeControl(OperationControl, BaseColor);

            if (RemoveRuleButton != null)
                RemoveRuleButton.BaseColor = BaseColor;

            if (ValueAccessor != null && ValueAccessor.Control != null)
                ControlPainter.ColorizeControl(ValueAccessor.Control, BaseColor);
        }
    }
}