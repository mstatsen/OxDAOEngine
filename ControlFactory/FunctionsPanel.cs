using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Observers;
using OxDAOEngine.Settings.Part;

namespace OxDAOEngine.ControlFactory
{
    public class PinnedChangedEventArgs : EventArgs
    {
        public readonly bool OldPinned;
        public readonly bool NewPinned;

        public PinnedChangedEventArgs(bool oldPinned, bool newPinned)
        {
            OldPinned = oldPinned;
            NewPinned = newPinned;
        }
    }

    public class ExpandedChangedEventArgs : EventArgs
    {
        public readonly bool OldExpanded;
        public readonly bool NewExpanded;

        public ExpandedChangedEventArgs(bool oldExpanded, bool newExpanded)
        {
            OldExpanded = oldExpanded;
            NewExpanded = newExpanded;
        }
    }

    public delegate void FunctionsPanelPinnedChangeHandler<TSettings>(FunctionsPanel<TSettings> sender, PinnedChangedEventArgs e)
        where TSettings : ISettingsController;

    public delegate void FunctionsPanelExpandedChangeHandler<TSettings>(FunctionsPanel<TSettings> sender, ExpandedChangedEventArgs e)
        where TSettings : ISettingsController;

    public abstract class FunctionsPanel<TSettings> : OxFunctionsPanel
        where TSettings : ISettingsController
    {
        public FunctionsPanel() : base()
        {
            ShowSettingsButton = true;
            BaseColor = FunctionColor;

            waiter = new OxWaiter(StopWaiter);
            waiter.Start();
        }

        protected bool SettingsAvailable { get; set; } = true;

        private bool ShowSettingsButton
        {
            get => CustomizeButton.Visible;
            set => CustomizeButton.Visible = SettingsAvailable && value;
        }

        public sealed override Color DefaultColor => base.DefaultColor;
        protected abstract Color FunctionColor { get; }

        protected override void PrepareInnerControls()
        {
            CustomizeButton.Size = new(28, 23);
            CustomizeButton.Click += (s, e) => SettingsForm.ShowSettings(Settings, SettingsPart);
            Header.AddToolButton(CustomizeButton);

            base.PrepareInnerControls();
            PrepareLoadingPanel();

            Sider.Parent = this;
            ExpandButton.Parent = Sider;
            ExpandButton.Borders.Size = OxSize.None;
            PreparePinButton(PinButton);
            PreparePinButton(PinButton2);
            PinButton2.VisibleChanged += PinButton2VisibleChanged;
        }

        private void PrepareLoadingPanel()
        {
            loadingPanel.Parent = this;
            loadingPanel.FontSize = 18;
            loadingPanel.Margin.Size = OxSize.None;
            loadingPanel.Borders.Size = OxSize.None;
            loadingPanel.BringToFront();
        }

        private void PinButton2VisibleChanged(object? sender, EventArgs e) =>
            SetExpandButtonLastBorder();

        private void PreparePinButton(OxIconButton button)
        {
            button.Parent = Sider;
            button.Borders.Size = OxSize.XXS;
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

        public OxPane Sider { get; } = new(new(16, 1));
        private readonly OxIconButton ExpandButton = new(OxIcons.Left, 16)
        {
            Dock = DockStyle.Fill,
            HiddenBorder = false
        };

        private readonly OxIconButton PinButton = new(OxIcons.Unpin, 16)
        {
            Dock = DockStyle.Left,
            HiddenBorder = false
        };

        private readonly OxIconButton PinButton2 = new(OxIcons.Unpin, 16)
        {
            Dock = DockStyle.Right,
            HiddenBorder = false
        };

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            RecalcPinned();
        }

        public bool SiderEnabled
        {
            get => waiter.Enabled;
            set => waiter.Enabled = value && !Pinned;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (ShowSettingsButton)
            {
                Control? upParent = Parent;

                while (upParent is not null)
                {
                    if (upParent is SettingsForm)
                    {
                        ShowSettingsButton = false;
                        break;
                    }

                    upParent = upParent.Parent;
                }
            }

            RecalcPinned();
        }

        protected override void PrepareColors()
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
            Font = Styles.Font(-1, FontStyle.Bold),
            HiddenBorder = false
        };

        public void ApplySettings() 
        {
            if (GeneralObserver[GeneralSetting.ShowCustomizeButtons])
                ShowSettingsButton = GeneralSettings.ShowCustomizeButtons;

            if (GeneralObserver[GeneralSetting.DoublePinButtons])
                PinButton2.Visible = GeneralSettings.DoublePinButtons;

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
            HeaderVisible = false;
            Header.ToolBar.Enabled = false;

            if (GeneralSettings.DarkerHeaders)
                BaseColor = Colors.Darker();

            loadingPanel.StartLoading();
        }

        protected void EndLoading()
        {
            loadingPanel.EndLoading();
            BaseColor = Colors.Lighter();
            Header.ToolBar.Enabled = true;
            HeaderVisible = true;
        }

        protected override int GetCalcedWidth() => 
            !OxDockHelper.IsVertical(OxDockHelper.Dock(Dock))
                ? (isFixedPanel ? 0 : Sider.CalcedWidth)
                    + (Expanded
                        ? base.GetCalcedWidth()
                        : Margin.LeftInt + Margin.RightInt
                    )
                : base.GetCalcedWidth();

        protected override int GetCalcedHeight() => 
            OxDockHelper.IsVertical(OxDockHelper.Dock(Dock))
                ? (isFixedPanel ? 0 : Sider.CalcedHeight) 
                    + (Expanded
                        ? base.GetCalcedHeight()
                        : Margin.TopInt + Margin.BottomInt
                    )
                : base.GetCalcedHeight();

        public override void ReAlignControls()
        {
            base.ReAlignControls();
            Header.ReAlign();
            Sider.ReAlign();
            SendToBack();
        }

        protected override void OnDockChanged(EventArgs e)
        {
            Borders.Size = OxSize.XXS;
            Padding.SetVisible(OxDockHelper.Dock(Dock), true);
            Padding[OxDockHelper.Dock(Sider.Dock)].Visible = true;

            OxDock oxDock = OxDockHelper.Dock(Dock);
            Sider.Dock = OxDockHelper.Dock(OxDockHelper.Opposite(oxDock));
            Borders[OxDockHelper.Dock(Sider.Dock)].Size =
                isFixedPanel
                    ? OxSize.XXS
                    : OxSize.None;
            Sider.Visible = 
                !isFixedPanel
                && Dock is not DockStyle.Fill
                       and not DockStyle.None;

            if (OxDockHelper.IsVertical(oxDock))
            {
                Sider.Size = new(1, 16);
                ExpandButton.Borders.Vertical = OxSize.XXS;
                ExpandButton.Borders.Horizontal = OxSize.None;
                PinButton.Dock = DockStyle.Left;
                PinButton.Width = 24;
                PinButton2.Dock = DockStyle.Right;
                PinButton2.Width = 24;
            }
            else
            {
                Sider.Size = new(16, 1);
                ExpandButton.Borders.Vertical = OxSize.None;
                ExpandButton.Borders.Horizontal = OxSize.XXS;
                PinButton.Borders.Size = OxSize.XXS;
                PinButton.Dock = DockStyle.Top;
                PinButton.Height = 24;
                PinButton2.Dock = DockStyle.Bottom;
                PinButton2.Height = 24;
            }

            SetExpandButtonLastBorder();
            ExpandButton.Icon = ExpandButtonIcon;
            RecalcPinned();
            RecalcSize();
            BringToFront();
            base.OnDockChanged(e);
            SetExpanded(Expanded);
        }

        private void SetExpandButtonLastBorder() =>
            ExpandButton.Borders[OxDockHelper.Dock(PinButton2.Dock)].Size = 
                PinButton2.Visible ? OxSize.None : OxSize.XXS;

        private bool expanded = false;

        private void SetExpanded(bool value)
        {
            if (!Expandable)
                return;

            value = isFixedPanel || value;
            OnExpandedChanging(new ExpandedChangedEventArgs(expanded, value));
            expanded = value;

            StartSizeRecalcing();
            try
            {
                Padding[OxDockHelper.Dock(Dock)].Visible = value;
                Padding[OxDockHelper.Dock(Sider.Dock)].Visible = value;
                //ContentBox.Visible = value;
                HeaderVisible = value; 
                ExpandButton.Icon = ExpandButtonIcon;
                Borders[OxDockHelper.Dock(Dock)].Size = value ? OxSize.XXS : OxSize.None;
            }
            finally
            {
                Update();
                EndSizeRecalcing();
                RecalcPinned();
                RecalcSize();
            }

            OnExpandedChanged(new ExpandedChangedEventArgs(!Expanded, Expanded));
        }

        protected virtual void OnExpandedChanging(ExpandedChangedEventArgs e) => 
            ExpandedChanging?.Invoke(this, e);

        protected virtual void OnExpandedChanged(ExpandedChangedEventArgs e) =>
            ExpandedChanged?.Invoke(this, e);

        private void SetMouseHandler(Control control)
        {
            if (pinned)
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
                && mouseHandlersSetted.Equals(pinned))
                return;

            SetMouseHandler(this);
            SetMouseHandler(ExpandButton);
            SetMouseHandler(ExpandButton.Picture);
            SetMouseHandler(PinButton);
            SetMouseHandler(PinButton.Picture);
            SetMouseHandler(PinButton2);
            SetMouseHandler(PinButton2.Picture);
            mouseHandlersSetted = pinned;
        }

        protected override void SetHandlers()
        {
            base.SetHandlers();
            //ApplyVisibleChangedHandler(ContentBox);
            ExpandButton.Click += (s, e) => Expanded = !Expanded;
            PinButton.Click += (s, e) => Pinned = !Pinned;
            PinButton2.Click += (s, e) => Pinned = !Pinned;
            SetMouseHandlers();
        }

        public bool Expanded
        {
            get => isFixedPanel || expanded;
            set => SetExpanded(value);
        }

        private bool Expandable => (IsVariableWidth || IsVariableHeight);

        public Bitmap? ExpandButtonIcon =>
            Dock switch
            {
                DockStyle.Left => expanded ? OxIcons.Left : OxIcons.Right,
                DockStyle.Right => expanded ? OxIcons.Right : OxIcons.Left,
                DockStyle.Top => expanded ? OxIcons.Up : OxIcons.Down,
                DockStyle.Bottom => expanded ? OxIcons.Down : OxIcons.Up,
                _ => null,
            };

        public void Expand() => Expanded = true;
        public void Collapse() => Expanded = false;

        public FunctionsPanelPinnedChangeHandler<TSettings>? PinnedChanged { get; set; }
        public FunctionsPanelExpandedChangeHandler<TSettings>? ExpandedChanging { get; set; }
        public FunctionsPanelExpandedChangeHandler<TSettings>? ExpandedChanged { get; set; }

        private bool pinned = false;
        public bool Pinned
        {
            get => isFixedPanel || pinned;
            set
            {
                pinned = isFixedPanel || value;
                waiter.Enabled = !pinned;

                if (pinned)
                {
                    StopWaiter();
                    waiter.Stop();
                }
                RecalcPinned();
            }
        }

        private void RecalcPinnedButton(OxIconButton button)
        {
            button.FreezeHovered = pinned;
            button.Icon = pinned ? OxIcons.Pin : OxIcons.Unpin;
        }

        public void RecalcPinned()
        {
            RecalcPinnedButton(PinButton);
            RecalcPinnedButton(PinButton2);

            OxPane? parentFillControl = GetParentFillControl();

            if (parentFillControl is not null)
            {
                parentFillControl.SuspendLayout();
                parentFillControl.Parent?.SuspendLayout();

                try
                {
                    if (Pinned)
                        SendToBack();
                    else
                        BringToFront();

                    SetParentPaddings();
                }
                finally
                {
                    parentFillControl?.Parent?.ResumeLayout();
                    parentFillControl?.ResumeLayout();
                }
            }
            OnPinnedChanged(new PinnedChangedEventArgs(!Pinned, Pinned));
            SetMouseHandlers();
        }

        protected virtual void OnPinnedChanged(PinnedChangedEventArgs e) =>
            PinnedChanged?.Invoke(this, e);

        private readonly Guid Id = Guid.NewGuid();

        private OxPane? GetParentFillControl()
        {
            if (Parent is not null)
                foreach (Control control in Parent.Controls)
                    if (control is OxPane pane
                        && Parent.Equals(pane.Parent)
                        && pane.Dock is DockStyle.Fill)
                        return pane;

            return null;
        }

        public OxPane? ParentPadding
        {
            get
            {
                OxPane? parentFillControl = GetParentFillControl();

                if (parentFillControl is not null)
                    foreach (Control control in parentFillControl.Controls)
                        if (control is OxPane pane
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
            OxPane? parentFillControl = GetParentFillControl();

            if (parentFillControl is null)
                return;

            OxPane? fakePadding = ParentPadding;

            int fakePaddingSize = OxDockHelper.IsVertical(OxDockHelper.Dock(Dock))
                ? Sider.CalcedHeight + Margin.TopInt + Margin.BottomInt
                : Sider.CalcedWidth + Margin.LeftInt + Margin.RightInt;

            if (fakePadding is not null)
            {
                if (!base.Visible)
                {
                    fakePadding.Visible = false;
                    return;
                }

                if (fakePadding.Dock.Equals(Dock))
                {
                    if (Pinned.Equals(fakePadding.Visible))
                        fakePadding.Visible = !Pinned;
                }
                else
                {
                    parentFillControl.Controls.Remove(fakePadding);
                    fakePadding = null;
                }
            }

            fakePadding ??= 
                new OxPane
                {
                    Name = FakePaddingName,
                    Parent = parentFillControl,
                    Dock = Dock,
                    BackColor = parentFillControl.BackColor,
                    Visible = !Pinned
                };

            if (OxDockHelper.IsVertical(Dock))
                fakePadding.Height = fakePaddingSize;
            else fakePadding.Width = fakePaddingSize;

            fakePadding.SendToBack();
        }

        private void MouseEnterHandler(object? sender, EventArgs e)
        {
            waiter.Start();
            waiter.Ready = base.Visible && !Expanded && !pinned;
        }

        private readonly OxWaiter waiter;

        private void MouseLeaveHandler(object? sender, EventArgs e) => 
            waiter.Ready = base.Visible && Expanded && !pinned;

        private void CheckExpandedState()
        {
            if (pinned)
                return;

            bool onPanel = ClientRectangle.Contains(PointToClient(MousePosition));

            if (!Expanded.Equals(onPanel))
                Expanded = onPanel;
        }

        private int StopWaiter()
        {
            waiter.Stop();

            if (Pinned)
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
            Sider.Visible = !isFixedPanel;
                
            if (isFixedPanel)
            {
                waiter.Stop();
                Pinned = true;
                Expanded = true;
            }

            OnDockChanged(EventArgs.Empty);
        }

        public new FunctionalPanelVisible Visible
        {
            get => !base.Visible 
                ? FunctionalPanelVisible.Hidden 
                : isFixedPanel 
                    ? FunctionalPanelVisible.Fixed 
                    : FunctionalPanelVisible.Float;
            set
            {
                base.Visible = value is not FunctionalPanelVisible.Hidden;
                SetAsFixedPanel(value is FunctionalPanelVisible.Fixed);

                if (value is FunctionalPanelVisible.Hidden)
                    HideParentPadding();
            }
        }

        private void HideParentPadding()
        {
            OxPane? parentFillControl = GetParentFillControl();

            if (parentFillControl is null)
                return;

            OxPane? parentPadding = ParentPadding;

            if (parentPadding is null)
                return;

            parentPadding.Visible = false;
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
                Pinned = 
                    Visible is FunctionalPanelVisible.Fixed 
                    || (bool)(settingPinned is not null ? settingPinned : false);
            }

            if (Observer[PinnedSetting]
                || Observer[ExpandedSetting])
            {
                bool? settingExpanded = (bool?)Settings[ExpandedSetting];
                Expanded = 
                    Pinned 
                    && (bool)(settingExpanded is not null ? settingExpanded : false);
            }

            if (SettingsManager.Settings<GeneralSettings>().Observer[GeneralSetting.DarkerHeaders])
                PrepareColors();
        }
    }
}