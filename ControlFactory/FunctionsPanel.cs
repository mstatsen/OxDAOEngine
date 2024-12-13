using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxLibrary.Waiter;
using OxDAOEngine.Data;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Observers;
using OxDAOEngine.Settings.Part;
using OxLibrary.Handlers;
using OxLibrary.Interfaces;
using OxLibrary.Geometry;

namespace OxDAOEngine.ControlFactory
{
    public delegate void BoolChandedEvent<TSettings>(FunctionsPanel<TSettings> sender, OxBoolChangedEventArgs e)
        where TSettings : ISettingsController;

    public abstract class FunctionsPanel<TSettings> : OxFunctionsPanel
        where TSettings : ISettingsController
    {
        public FunctionsPanel() : base()
        {
            ShowSettingsButton = OxB.T;
            BaseColor = FunctionColor;

            waiter = new OxWaiter(StopWaiter);
            waiter.Start();
        }

        protected OxBool SettingsAvailable { get; set; } = OxB.T;
        protected bool IsSettingsAvailable =>
            OxB.B(SettingsAvailable);
        protected void SetSettingsAvailable(bool value) =>
            SettingsAvailable = OxB.B(value);

        private OxBool ShowSettingsButton
        {
            get => CustomizeButton.Visible;
            set => CustomizeButton.Visible = OxB.And(SettingsAvailable, value);
        }

        private bool IsShowSettingsButton =>
            OxB.B(ShowSettingsButton);

        private void SetShowSettingsButton(bool value) =>
            ShowSettingsButton = OxB.B(value);

        public sealed override Color DefaultColor => base.DefaultColor;
        protected abstract Color FunctionColor { get; }

        protected override void PrepareInnerComponents()
        {
            CustomizeButton.Size = new(28, 23);
            CustomizeButton.Click += (s, e) => SettingsForm.ShowSettings(Settings, SettingsPart);
            Header.AddButton(CustomizeButton);

            base.PrepareInnerComponents();
            PrepareLoadingPanel();

            Sider.Parent = this;
            ExpandButton.Parent = Sider;
            ExpandButton.Borders.Size = 0;
            PreparePinButton(PinButton);
            PreparePinButton(PinButton2);
            PinButton2.VisibleChanged += PinButton2VisibleChanged;
        }

        private void PrepareLoadingPanel()
        {
            loadingPanel.Parent = this;
            loadingPanel.FontSize = 18;
            loadingPanel.Margin.Size = 0;
            loadingPanel.Borders.Size = 0;
            loadingPanel.BringToFront();
        }

        private void PinButton2VisibleChanged(object? sender, EventArgs e) =>
            SetExpandButtonLastBorder();

        private void PreparePinButton(OxIconButton button)
        {
            button.Parent = Sider;
            button.Borders.Size = 1;
            button.HoveredChanged = PinButtonHoveredChanged;
        }

        private void PinButtonHoveredChanged(object? sender, EventArgs e)
        {
            if (PinButton.Equals(sender))
            {
                if (!PinButton.Hovered.Equals(PinButton2.Hovered))
                    PinButton2.Hovered = PinButton.Hovered;
            }
            else
            if (!PinButton.Hovered.Equals(PinButton2.Hovered))
                PinButton.Hovered = PinButton2.Hovered;
        }

        public OxPanel Sider { get; } = new(new(16, 1));
        private readonly OxIconButton ExpandButton = new(OxIcons.Left, 16)
        {
            Dock = OxDock.Fill,
            HiddenBorder = false
        };

        private readonly OxIconButton PinButton = new(OxIcons.Unpin, 16)
        {
            Dock = OxDock.Left,
            HiddenBorder = false
        };

        private readonly OxIconButton PinButton2 = new(OxIcons.Unpin, 16)
        {
            Dock = OxDock.Right,
            HiddenBorder = false
        };

        public override void OnVisibleChanged(OxBoolChangedEventArgs e)
        {
            base.OnVisibleChanged(e);

            if (e.IsChanged)
                RecalcPinned();
        }

        public OxBool SiderEnabled
        {
            get => OxB.B(IsSiderEnabled);
            set => SetSiderEnabled(OxB.B(value));
        }

        public bool IsSiderEnabled =>
            waiter.Enabled;

        public void SetSiderEnabled(bool value) =>
            waiter.Enabled = value && !IsPinned;

        public override void OnParentChanged(OxParentChangedEventArgs e)
        {
            base.OnParentChanged(e);

            if (IsShowSettingsButton)
            {
                IOxBox? upParent = Parent;

                while (upParent is not null)
                {
                    if (upParent is SettingsForm)
                    {
                        ShowSettingsButton = OxB.F;
                        break;
                    }

                    upParent = upParent.Parent;
                }
            }

            RecalcPinned();
        }

        public override void PrepareColors()
        {
            base.PrepareColors();

            loadingPanel.BaseColor = BaseColor;

            if (GeneralSettings.DarkerHeaders)
                Header.BaseColor = Colors.Darker();

            ExpandButton.BaseColor = Colors.Darker(GeneralSettings.DarkerHeaders ? 1 : 0);
            PinButton.BaseColor = ExpandButton.BaseColor;
            PinButton2.BaseColor = ExpandButton.BaseColor;
        }

        protected virtual TSettings Settings => SettingsManager.Settings<TSettings>();
        protected GeneralSettings GeneralSettings => SettingsManager.Settings<GeneralSettings>();
        protected ISettingsObserver GeneralObserver => GeneralSettings.Observer;

        protected virtual SettingsPart SettingsPart => default;

        private readonly OxIconButton CustomizeButton = new(OxIcons.Settings, 23)
        {
            Font = OxStyles.Font(-1, FontStyle.Bold),
            HiddenBorder = false
        };

        public void ApplySettings()
        {
            if (GeneralObserver[GeneralSetting.ShowCustomizeButtons])
                SetShowSettingsButton(GeneralSettings.ShowCustomizeButtons);

            if (GeneralObserver[GeneralSetting.DoublePinButtons])
                PinButton2.SetVisible(GeneralSettings.DoublePinButtons);

            if (GeneralObserver[GeneralSetting.ColorizePanels])
                BaseColor = GeneralSettings.ColorizePanels ? FunctionColor : DefaultColor;
            else
                if (GeneralObserver[GeneralSetting.DarkerHeaders])
                PrepareColors();

            ApplySettingsInternal();
        }

        protected abstract void ApplySettingsInternal();

        public virtual void SaveSettings() { }

        private readonly OxLoadingPanel loadingPanel = new();

        protected void StartLoading()
        {
            HeaderVisible = OxB.F;
            Header.ToolBar.Enabled = OxB.F;

            if (GeneralSettings.DarkerHeaders)
                BaseColor = Colors.Darker();

            loadingPanel.StartLoading();
        }

        protected void EndLoading()
        {
            loadingPanel.EndLoading();
            BaseColor = Colors.Lighter();
            Header.ToolBar.Enabled = OxB.T;
            HeaderVisible = OxB.T;
        }

        /*
        protected override short GetCalcedWidth() =>
            !OxDockHelper.IsVertical(Dock)
                ? (isFixedPanel ? 0 : Sider.CalcedWidth)
                    | (Expanded
                        ? base.GetCalcedWidth()
                        : Margin.Left | Margin.Right
                    )
                : base.GetCalcedWidth();

        protected override short GetCalcedHeight() => 
            OxDockHelper.IsVertical(Dock)
                ? (isFixedPanel ? 0 : Sider.CalcedHeight) 
                    | (Expanded
                        ? base.GetCalcedHeight()
                        : Margin.Top | Margin.Bottom
                    )
                : base.GetCalcedHeight();
        */

        public override void OnDockChanged(OxDockChangedEventArgs e)
        {
            if (!OxDockHelper.IsSingleDirectionDock(Dock))
                return;

            Borders.Size = 1;
            Padding.SetVisible(Dock, OxB.T);
            Sider.Dock = OxDockHelper.Opposite(Dock);
            Borders[Sider.Dock].Size = OxSh.Short(isFixedPanel ? 1 : 0);
            Sider.SetVisible(
                !isFixedPanel
                && Dock is not OxDock.Fill
                       and not OxDock.None
            );

            if (OxDockHelper.IsVertical(Dock))
            {
                Sider.Size = new(1, 16);
                ExpandButton.Borders.Vertical = 1;
                ExpandButton.Borders.Horizontal = 0;
                PinButton.Dock = OxDock.Left;
                PinButton.Width = 24;
                PinButton2.Dock = OxDock.Right;
                PinButton2.Width = 24;
            }
            else
            {
                Sider.Size = new(16, 1);
                ExpandButton.Borders.Vertical = 0;
                ExpandButton.Borders.Horizontal = 1;
                PinButton.Borders.Size = 1;
                PinButton.Dock = OxDock.Top;
                PinButton.Height = 24;
                PinButton2.Dock = OxDock.Bottom;
                PinButton2.Height = 24;
            }

            SetExpandButtonLastBorder();
            ExpandButton.Icon = ExpandButtonIcon;
            RecalcPinned();
            //RecalcSize();
            BringToFront();
            base.OnDockChanged(e);
            SetExpanded(Expanded);
        }

        private void SetExpandButtonLastBorder() =>
            ExpandButton.Borders[PinButton2.Dock].Size = 
                OxSh.Short(!PinButton2.IsVisible ? 1 : 0);

        private OxBool expanded = OxB.F;

        private void SetExpanded(OxBool value)
        {
            if (!OxDockHelper.IsSingleDirectionDock(Sider.Dock)
                || !Expandable)
                return;

            value = OxB.Or(isFixedPanel, value);
            OnExpandedChanging(new OxBoolChangedEventArgs(expanded, value));
            expanded = value;

            WithSuspendedLayout(
                () =>
                {
                    Padding[Dock].Visible = value;
                    Padding[Sider.Dock].Visible = value;
                    HeaderVisible = value;
                    ExpandButton.Icon = ExpandButtonIcon;
                    Borders[Dock].Size = OxSh.Short(IsExpanded ? 1 : 0);
                }
            );

            Update();
            RecalcPinned();
//          RecalcSize();
            OnExpandedChanged(new OxBoolChangedEventArgs(IsExpanded));
        }

        protected virtual void OnExpandedChanging(OxBoolChangedEventArgs e) => 
            ExpandedChanging?.Invoke(this, e);

        protected virtual void OnExpandedChanged(OxBoolChangedEventArgs e) =>
            ExpandedChanged?.Invoke(this, e);

        private void SetMouseHandler(Control control)
        {
            if (IsPinned)
            {
                control.MouseEnter -= MouseEnterHandler;
                control.MouseLeave -= MouseLeaveHandler;
            }
            else
            {
                control.MouseEnter += MouseEnterHandler;
                control.MouseLeave += MouseLeaveHandler;
            }

        }

        private bool? mouseHandlersSetted = null;

        private void SetMouseHandlers()
        {
            if (mouseHandlersSetted is not null 
                && mouseHandlersSetted.Equals(IsPinned))
                return;

            SetMouseHandler(this);
            SetMouseHandler(ExpandButton);
            SetMouseHandler(ExpandButton.Picture);
            SetMouseHandler(PinButton);
            SetMouseHandler(PinButton.Picture);
            SetMouseHandler(PinButton2);
            SetMouseHandler(PinButton2.Picture);
            mouseHandlersSetted = IsPinned;
        }

        protected override void SetHandlers()
        {
            base.SetHandlers();
            //ApplyVisibleChangedHandler(ContentBox);
            ExpandButton.Click += (s, e) => Expanded = OxB.Not(Expanded);
            PinButton.Click += (s, e) => SetPinned(!IsPinned);
            PinButton2.Click += (s, e) => SetPinned(!IsPinned);
            SetMouseHandlers();
        }

        public OxBool Expanded
        {
            get => OxB.Or(isFixedPanel, expanded);
            set => SetExpanded(value);
        }

        public bool IsExpanded =>
            OxB.B(Expanded);

        public void SetExpanded(bool value) =>
            Expanded = OxB.B(value);

        private bool Expandable =>
            IsVariableWidth || IsVariableHeight;

        public Bitmap? ExpandButtonIcon =>
            Dock switch
            {
                OxDock.Left => IsExpanded ? OxIcons.Left : OxIcons.Right,
                OxDock.Right => IsExpanded ? OxIcons.Right : OxIcons.Left,
                OxDock.Top => IsExpanded ? OxIcons.Up : OxIcons.Down,
                OxDock.Bottom => IsExpanded ? OxIcons.Down : OxIcons.Up,
                _ => null,
            };

        public void Expand() => Expanded = OxB.T;
        public void Collapse() => Expanded = OxB.F;

        public BoolChandedEvent<TSettings>? PinnedChanged { get; set; }
        public BoolChandedEvent<TSettings>? ExpandedChanging { get; set; }
        public BoolChandedEvent<TSettings>? ExpandedChanged { get; set; }

        private OxBool pinned = OxB.F;
        public OxBool Pinned
        {
            get => OxB.Or(isFixedPanel, pinned);
            set
            {
                pinned = OxB.Or(isFixedPanel, value);
                waiter.Enabled = OxB.B(OxB.Not(pinned));

                if (OxB.B(pinned))
                {
                    StopWaiter();
                    waiter.Stop();
                }
                RecalcPinned();
            }
        }

        public bool IsPinned =>
            OxB.B(Pinned);

        public void SetPinned(bool value) =>
            Pinned = OxB.B(value);

        private void RecalcPinnedButton(OxIconButton button)
        {
            button.FreezeHovered = IsPinned;
            button.Icon = IsPinned ? OxIcons.Pin : OxIcons.Unpin;
        }

        public void RecalcPinned()
        {
            RecalcPinnedButton(PinButton);
            RecalcPinnedButton(PinButton2);

            OxPanel? parentFillControl = GetParentFillControl();

            parentFillControl?.WithSuspendedLayout(
                () =>
                {
                    if (IsPinned)
                        SendToBack();
                    else
                        BringToFront();

                    SetParentPaddings();
                }
            );
            OnPinnedChanged(new OxBoolChangedEventArgs(Pinned));
            SetMouseHandlers();
        }

        protected virtual void OnPinnedChanged(OxBoolChangedEventArgs e) =>
            PinnedChanged?.Invoke(this, e);

        private readonly Guid Id = Guid.NewGuid();

        private OxPanel? GetParentFillControl()
        {
            if (Parent is not null)
                foreach (Control control in Parent.Controls)
                    if (control is OxPanel pane
                        && Parent.Equals(pane.Parent)
                        && pane.Dock is OxDock.Fill)
                        return pane;

            return null;
        }

        public OxPanel? ParentPadding
        {
            get
            {
                OxPanel? parentFillControl = GetParentFillControl();

                if (parentFillControl is not null)
                    foreach (Control control in parentFillControl.Controls)
                        if (control is OxPanel pane
                            && pane.Name.Equals(FakePaddingName))
                            return pane;

                return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            waiter.Stop();
            base.Dispose(disposing);
        }

        private readonly string fakePaddingNamePrefix = "SidePanelPadding_";
        private string FakePaddingName => $"{fakePaddingNamePrefix}{Id}";

        private void SetParentPaddings()
        {
            OxPanel? parentFillControl = GetParentFillControl();

            if (parentFillControl is null)
                return;

            OxPanel? fakePadding = ParentPadding;

            short fakePaddingSize =
                OxSh.Short(
                    OxDockHelper.IsVertical(Dock)
                        ? Sider.Height + Margin.Top + Margin.Bottom
                        : Sider.Width + Margin.Left + Margin.Right
                );

            if (fakePadding is not null)
            {
                if (!IsVisible)
                {
                    fakePadding.Visible = OxB.F;
                    return;
                }

                if (fakePadding.Dock.Equals(Dock))
                {
                    if (Pinned.Equals(fakePadding.Visible))
                        fakePadding.Visible = OxB.Not(Pinned);
                }
                else
                {
                    parentFillControl.Controls.Remove(fakePadding);
                    fakePadding = null;
                }
            }

            fakePadding ??= 
                new OxPanel
                {
                    Name = FakePaddingName,
                    Parent = parentFillControl,
                    Dock = Dock,
                    BackColor = parentFillControl.BackColor,
                    Visible = OxB.Not(Pinned)
                };

            if (OxDockHelper.IsVertical(Dock))
                fakePadding.Height = fakePaddingSize;
            else fakePadding.Width = fakePaddingSize;

            fakePadding.SendToBack();
        }

        private void MouseEnterHandler(object? sender, EventArgs e)
        {
            waiter.Start();
            waiter.Ready = IsVisible && !IsExpanded && !IsPinned;
        }

        private readonly OxWaiter waiter;

        private void MouseLeaveHandler(object? sender, EventArgs e) => 
            waiter.Ready = IsVisible && IsExpanded && !IsPinned;

        private void CheckExpandedState()
        {
            if (IsPinned)
                return;

            bool onPanel = ClientRectangle.Contains(PointToClient(MousePosition));

            if (!IsExpanded.Equals(onPanel))
                Expanded = OxB.Not(Expanded);
        }

        private int StopWaiter()
        {
            waiter.Stop();

            if (IsPinned)
                return 1;

            if (Parent is null 
                || !Created 
                || Disposing)
                return 2;

            BeginInvoke(CheckExpandedState);
            return 0;
        }

        private bool isFixedPanel = false;
        private void SetAsFixedPanel(bool fixedPanel)
        {
            if (isFixedPanel.Equals(fixedPanel))
                return;

            isFixedPanel = fixedPanel;
            Sider.SetVisible(!isFixedPanel);
                
            if (isFixedPanel)
            {
                waiter.Stop();
                Pinned = OxB.T;
                Expanded = OxB.T;
            }

            OnDockChanged(EventArgs.Empty);
        }

        public new FunctionalPanelVisible Visible
        {
            get => !IsVisible 
                ? FunctionalPanelVisible.Hidden 
                : isFixedPanel 
                    ? FunctionalPanelVisible.Fixed 
                    : FunctionalPanelVisible.Float;
            set
            {
                SetVisible(value is not FunctionalPanelVisible.Hidden);
                SetAsFixedPanel(value is FunctionalPanelVisible.Fixed);

                if (value is FunctionalPanelVisible.Hidden)
                    HideParentPadding();
            }
        }

        private void HideParentPadding()
        {
            OxPanel? parentFillControl = GetParentFillControl();

            if (parentFillControl is null)
                return;

            OxPanel? parentPadding = ParentPadding;

            if (parentPadding is null)
                return;

            parentPadding.Visible = OxB.F;
        }
    }

    public abstract class FunctionsPanel<TField, TDAO> : FunctionsPanel<DAOSettings<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, IFieldMapping<TField>, new()
    {
        protected DAOObserver<TField, TDAO> Observer => Settings.Observer;
        protected abstract DAOSetting VisibleSetting { get; }
        protected abstract DAOSetting PinnedSetting { get; }
        protected abstract DAOSetting ExpandedSetting { get; }

        protected override void ApplySettingsInternal()
        {
            if (Observer[VisibleSetting])
            {
                FunctionalPanelVisible? settingVisible = (FunctionalPanelVisible?)Settings[VisibleSetting];
                Visible = (FunctionalPanelVisible)(
                    settingVisible is not null
                        ? settingVisible
                        : FunctionalPanelVisible.Float
                );
            }

            if (Observer[PinnedSetting])
            {
                bool? settingPinned = (bool?)Settings[PinnedSetting];
                SetPinned(
                    Visible is FunctionalPanelVisible.Fixed 
                    || (bool)(settingPinned is not null ? settingPinned : false)
                );
            }

            if (Observer[PinnedSetting]
                || Observer[ExpandedSetting])
            {
                bool? settingExpanded = (bool?)Settings[ExpandedSetting];
                SetExpanded(
                    IsPinned 
                    && (bool)(settingExpanded is not null ? settingExpanded : false)
                );
            }

            if (SettingsManager.Settings<GeneralSettings>().Observer[GeneralSetting.DarkerHeaders])
                PrepareColors();
        }
    }
}