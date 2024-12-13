﻿using OxLibrary.Handlers;
using OxLibrary.Interfaces;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;
using OxLibrary;

namespace OxDAOEngine.ControlFactory.Controls;

public abstract class CustomControl<TField, TDAO, TItem> : OxPanel, 
    ICustomControl<TField, TDAO>, IColoredCustomControl
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
    where TItem : DAO
{
    public CustomControl<TField, TDAO, TItem> Init(IBuilderContext<TField, TDAO> context)
    {
        Context = context;
        InitComponents();
        RecalcControls();
        return this;
    }

    public override void OnSizeChanged(OxSizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (!e.IsChanged)
            return;

        RecalcControls();
    }

    public IBuilderContext<TField, TDAO> Context { get; private set; } = default!;

    public EventHandler? ValueChangeHandler;
    protected abstract IOxControl GetControl();
    protected abstract void SetValue(TItem value);
    protected abstract TItem GetValue();
    protected abstract void InitComponents();

    public TItem Value
    {
        get => GetValue();
        set => SetValue(value);
    }

    public IOxControl Control =>
        GetControl();

    protected virtual Color GetControlColor() => 
        Control is null 
            ? BackColor 
            : Control.BackColor;

    protected virtual void SetControlColor(Color value)
    {
        if (Control is not null)
            Control.BackColor = value;
    }

    public Color ControlColor
    {
        get => GetControlColor();
        set => SetControlColor(value);
    }

    protected virtual void RecalcControls() { }

    public OxBool ReadOnly
    {
        get => GetReadOnly();
        set => SetReadOnly(value);
    }

    protected abstract OxBool GetReadOnly();
    protected abstract void SetReadOnly(OxBool value);
    public bool IsReadOnly => OxB.B(ReadOnly);

    private EventHandler? itemEdited;

    public EventHandler? ItemEdited
    {
        get => itemEdited;
        set => SetItemEdited(value);
    }

    protected virtual void SetItemEdited(EventHandler? value) =>
        itemEdited = value;

    public virtual object? PrepareValueToReadOnly(TItem? value) =>
        value;

    public TDAO? OwnerDAO { get; set; }

    public virtual ReadonlyMode ReadonlyMode => ReadonlyMode.ViewAsReadonly;
}