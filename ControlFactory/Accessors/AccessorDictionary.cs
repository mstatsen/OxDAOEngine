﻿using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Geometry;
using OxLibrary.Interfaces;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;


namespace OxDAOEngine.ControlFactory.Accessors;

public class AccessorDictionary<TField, TDAO> : Dictionary<object, IControlAccessor>
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
{
    public void AlignAccessors()
    {
        short maxLeft = 0;

        foreach (IControlAccessor accessor in Values)
            maxLeft = OxSH.Max(maxLeft, accessor.Left);

        foreach (IControlAccessor accessor in Values)
            accessor.Left = maxLeft;
    }

    public new void Clear()
    {
        ClearAccessorsParent();
        base.Clear();
    }

    private const short HorizontalSpace = 28;

    public IControlAccessor CreateAccessor(TField field, object key, IOxBox parent, 
        string? caption, object value, OxSize location)
    {
        OxLabel captionLabel = new()
        {
            Parent = parent,
            Left = location.Width,
            Text = caption is null ? string.Empty : caption,
            Font = OxStyles.Font(FontStyle.Italic),
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = true,
            Cursor = Cursors.Hand
        };
        captionLabel.Click += (s, e) =>
        {
            FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>()!;

            if (fieldHelper.GetFieldType(field) is FieldType.Enum
                && key is string stringValue)
            {
                ITypeHelper? helper = fieldHelper.GetHelper(field);

                if (helper is not null)
                    key = helper.Parse(stringValue);
            }

            DataManager.ViewItems<TField, TDAO>(field, key);
        };
        captionLabel.MouseEnter += LabelMouseEnter;
        captionLabel.MouseLeave += LabelMouseLeave;

        IControlAccessor accessor = DataManager.Builder<TField, TDAO>(ControlScope.Inline)
            .Accessor(
                $"{typeof(TField).Name}_{typeof(TDAO).Name}_AD{caption!}", 
                FieldType.Label
            );
        accessor.Left = OxSH.Add(captionLabel.Right, HorizontalSpace);
        accessor.Top = location.Height;
        accessor.Parent = parent;
        accessor.Value = value;
        accessor.Control.Font = OxStyles.Font(FontStyle.Bold);
        OxControlHelper.AlignByBaseLine(accessor.Control, captionLabel);
        Add(new KeyValuePair<TField, object>(field, key), accessor);
        return accessor;
    }

    private void LabelMouseLeave(object? sender, EventArgs e)
    {
        OxLabel? label = (OxLabel?)sender;

        if (label is null)
            return;

        label.Font = new(label.Font, label.Font.Style & ~FontStyle.Underline);
        label.ForeColor = new OxColorHelper(label.ForeColor).HBluer(-6).Lighter(4);
    }

    private void LabelMouseEnter(object? sender, EventArgs e)
    {
        OxLabel? label = (OxLabel?)sender;

        if (label is null)
            return;

        label.Font = new(label.Font, label.Font.Style | FontStyle.Underline);
        label.ForeColor = new OxColorHelper(label.ForeColor).HDarker(4).Bluer(6);
    }

    private void ClearAccessorsParent()
    {
        foreach (IControlAccessor accessor in Values)
            accessor.Parent = null;
    }
};