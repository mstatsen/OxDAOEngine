﻿using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxDAOEngine.ControlFactory.Controls.Fields;

namespace OxDAOEngine.ControlFactory.Filter
{
    public class RulePanel<TField, TDAO> : OxPane
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public readonly int Number;
        private readonly int GroupNumber;
        private readonly ControlBuilder<TField, TDAO> Builder;
        private readonly FieldAccessor<TField, TDAO> FieldControl = default!;
        private readonly EnumAccessor<TField, TDAO, FilterOperation> OperationControl;
        private IControlAccessor? ValueAccessor;

        private readonly OxIconButton RemoveRuleButton = new(OxIcons.Minus, 20);

        public FilterRule<TField> Rule 
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

        private void FillRule(FilterRule<TField>? value)
        {
            if (value is null)
                FieldControl.SelectFirst();
            else
                FieldControl.Value = value.Field;

            RenewOperationControl();

            if (value is null)
                OperationControl.SelectFirst();
            else
                OperationControl.Value = value.Operation;
            
            LayoutValueControl();

            if (ValueAccessor is not null)
                ValueAccessor.Value = value?[FieldControl.EnumValue];

            SetValueAccessorVisible();
        }

        private FilterRule<TField> GrabRule()
        {
            FilterRule<TField> result = new()
            {
                Field = FieldControl.EnumValue,
                Operation = OperationControl.EnumValue
            };

            if (ValueAccessor is not null)
                result[FieldControl.EnumValue] = ValueAccessor.Value;

            return result;
        }

        public RulePanel(FilterRule<TField> rule, ControlBuilder<TField, TDAO> builder, int groupNumber, int number) 
            : base(new(1, 32))
        { 
            Builder = builder;
            GroupNumber = groupNumber;
            Number = number;
            Dock = DockStyle.Top;
            FieldControl = (FieldAccessor<TField,TDAO>)
                CreateSimpleControl("SimpleFilter:Field", FieldType.MetaData, 24, 160);
            FieldControl.ValueChangeHandler += FieldControlValueChangeHandler;
            OperationControl = (EnumAccessor<TField, TDAO, FilterOperation>)
                CreateSimpleControl("SimpleFilter:Operation", FieldType.Enum, 188, 110);
            OperationControl.ValueChangeHandler += OperationControlValueChangeHandler;
            PrepareRemoveRuleButton();
            Rule = rule;
        }

        private readonly FilterOperationHelper FilterOperationHelper = TypeHelper.Helper<FilterOperationHelper>();

        private void OperationControlValueChangeHandler(object? sender, EventArgs e) =>
            SetValueAccessorVisible();

        private void SetValueAccessorVisible()
        {
            if (ValueAccessor is null)
                return;

            ValueAccessor.Visible = !FilterOperationHelper.IsUnaryOperation(OperationControl.EnumValue);
        }

        private readonly FieldHelper<TField> FieldHelper = TypeHelper.FieldHelper<TField>();

        public bool FieldIsEmpty =>
            FieldControl.Value is null
            || (FieldControl.Value is IEmptyChecked ec && ec.IsEmpty);

        private void FieldControlValueChangeHandler(object? sender, EventArgs e)
        {
            RenewOperationControl();
            LayoutValueControl();
        }

        private void RenewOperationControl()
        {
            if (OperationControl.Context.Initializer is not FilterOperationInitializer<TField> initializer)
                return;

            initializer.Field = FieldControl.EnumValue;
            OperationControl.RenewControl(true);
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

            ValueAccessor.Control.Width =
                FieldHelper.GetFieldType(FieldControl.EnumValue) switch
                {
                    FieldType.Boolean or
                    FieldType.Integer =>
                        80,
                FieldType.Enum =>
                        120,
                    _ =>
                        220,
                };

            ValueAccessor.Visible = true;
            ValueAccessor.Clear();
            PrepareColors();
        }

        private void HideValueControl()
        {
            if (ValueAccessor is null)
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

            if (FieldControl is not null)
                ControlPainter.ColorizeControl(FieldControl, BaseColor);

            if (OperationControl is not null)
                ControlPainter.ColorizeControl(OperationControl, BaseColor);

            if (RemoveRuleButton is not null)
                RemoveRuleButton.BaseColor = BaseColor;

            if (ValueAccessor is not null 
                && ValueAccessor.Control is not null)
                ControlPainter.ColorizeControl(ValueAccessor.Control, BaseColor);
        }
    }
}