using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Observers;

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
            CustomizeButton.SetContentSize(28, 23);
            CustomizeButton.Click += (s, e) => SettingsForm.ShowSettings(Settings, SettingsPart);
            Header.AddToolButton(CustomizeButton);

            base.PrepareInnerControls();
            PrepareLoadingPanel();

            Sider.Parent = this;
            ExpandButton.Parent = Sider;
            ExpandButton.Borders.SetSize(OxSize.None);
            PreparePinButton(PinButton);
            PreparePinButton(PinButton2);
            PinButton2.VisibleChanged += PinButton2VisibleChanged;
        }

        private void PrepareLoadingPanel()
        {
            loadingPanel.Parent = ContentContainer;
            loadingPanel.FontSize = 18;
            loadingPanel.Margins.SetSize(OxSize.None);
            loadingPanel.Borders.SetSize(OxSize.None);
            loadingPanel.Borders.LeftOx = OxSize.None;
            loadingPanel.BringToFront();
        }

        private void PinButton2VisibleChanged(object? sender, EventArgs e) =>
            SetExpandButtonLastBorder();

        private void PreparePinButton(OxIconButton button)
        {
            button.Parent = Sider;
            button.Borders.SetSize(OxSize.Small);
            button.HoveredChanged = PinButtonHoveredChanged;
        }

        private void PinButtonHoveredChanged(object? sender, EventArgs e)
        {
            if (sender == PinButton)
            {
                if (PinButton.Hovered != PinButton2.Hovered)
                    PinButton2.Hovered = PinButton.Hovered;
            }
            else
            if (PinButton.Hovered != PinButton2.Hovered)
                PinButton.Hovered = PinButton2.Hovered;
        }

        public OxPanel Sider { get; } = new(new Size(16, 1));
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
                Control upParent = Parent;

                while (upParent != null)
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
            Header.Visible = false;
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
            Header.Visible = true;
        }

        protected override int GetCalcedWidth() => 
            !OxDockHelper.IsVertical(OxDockHelper.Dock(Dock))
                ? Sider.CalcedWidth + (Expanded
                    ? base.GetCalcedWidth()
                    : Margins[OxDock.Left].CalcedWidth + Margins[OxDock.Right].CalcedWidth)
                : base.GetCalcedWidth();

        protected override int GetCalcedHeight() => 
            OxDockHelper.IsVertical(OxDockHelper.Dock(Dock))
                ? Sider.CalcedHeight + (Expanded
                    ? base.GetCalcedHeight()
                    : Margins[OxDock.Top].CalcedHeight + Margins[OxDock.Bottom].CalcedHeight)
                : base.GetCalcedHeight();

        public override void ReAlignControls()
        {
            ContentContainer.ReAlign();
            Paddings.ReAlign();
            Header.ReAlign();
            Borders.ReAlign();
            Sider.ReAlign();
            Margins.ReAlign();
            SendToBack();
        }

        protected override void OnDockChanged(EventArgs e)
        {
            OxDock oxDock = OxDockHelper.Dock(Dock);
            Sider.Dock = OxDockHelper.Dock(OxDockHelper.Opposite(oxDock));
            Borders[OxDockHelper.Dock(Sider.Dock)].Visible = IsSimplePanel;
            Sider.Visible = !IsSimplePanel && Dock != DockStyle.Fill && Dock != DockStyle.None;

            if (OxDockHelper.IsVertical(oxDock))
            {
                Sider.SetContentSize(1, 16);
                ExpandButton.Borders.VerticalOx = OxSize.Small;
                ExpandButton.Borders.HorizontalOx = OxSize.None;
                PinButton.Dock = DockStyle.Left;
                PinButton.Width = 24;
                PinButton2.Dock = DockStyle.Right;
                PinButton2.Width = 24;
            }
            else
            {
                Sider.SetContentSize(16, 1);
                ExpandButton.Borders.VerticalOx = OxSize.None;
                ExpandButton.Borders.HorizontalOx = OxSize.Small;
                PinButton.Borders.SetSize(OxSize.Small);
                PinButton.Dock = DockStyle.Top;
                PinButton.Height = 24;
                PinButton2.Dock = DockStyle.Bottom;
                PinButton2.Height = 24;
            }

            SetExpandButtonLastBorder();

            ExpandButton.Icon = ExpandButtonIcon;
            RecalcPinned();
            base.OnDockChanged(e);
        }

        private void SetExpandButtonLastBorder() =>
            ExpandButton.Borders.Borders[OxDockHelper.Dock(PinButton2.Dock)].SetSize(
                PinButton2.Visible ? OxSize.None : OxSize.Small
            );

        private bool expanded = false;

        private void SetExpanded(bool value)
        {
            if (!Expandable)
                return;

            OnExpandedChanging(new ExpandedChangedEventArgs(expanded, value));
            expanded = value;

            StartSizeRecalcing();
            try
            {
                Paddings[OxDockHelper.Dock(Dock)].Visible = value;
                Paddings[OxDockHelper.Dock(Sider.Dock)].Visible = value;
                ContentContainer.Visible = value;
                Header.Visible = value; 
                ExpandButton.Icon = ExpandButtonIcon;
                Borders[OxDockHelper.Dock(Dock)].Visible = value;
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

        private void SetMouseHandler(OxBorders borders)
        {
            foreach (OxBorder border in borders.Borders.Values)
                SetMouseHandler(border);
        }

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
            if (mouseHandlersSetted != null && 
                mouseHandlersSetted == pinned)
                return;

            SetMouseHandler(this);
            SetMouseHandler(ContentContainer);
            SetMouseHandler(ExpandButton);
            SetMouseHandler(ExpandButton.Picture);
            SetMouseHandler(ExpandButton.Margins);
            SetMouseHandler(ExpandButton.Borders);
            SetMouseHandler(ExpandButton.Paddings);
            SetMouseHandler(PinButton);
            SetMouseHandler(PinButton.Picture);
            SetMouseHandler(PinButton.Margins);
            SetMouseHandler(PinButton.Borders);
            SetMouseHandler(PinButton.Paddings);
            SetMouseHandler(PinButton2);
            SetMouseHandler(PinButton2.Picture);
            SetMouseHandler(PinButton2.Margins);
            SetMouseHandler(PinButton2.Borders);
            SetMouseHandler(PinButton2.Paddings);
            SetMouseHandler(Margins);
            SetMouseHandler(Borders);
            SetMouseHandler(Paddings);
            mouseHandlersSetted = pinned;
        }

        protected override void SetHandlers()
        {
            base.SetHandlers();
            ApplyRecalcSizeHandler(ContentContainer, false, true);
            ExpandButton.Click += (s, e) => Expanded = !Expanded;
            PinButton.Click += (s, e) => Pinned = !Pinned;
            PinButton2.Click += (s, e) => Pinned = !Pinned;
            SetMouseHandlers();
        }

        public bool Expanded
        {
            get => expanded;
            set => SetExpanded(value);
        }

        private bool Expandable => IsVariableWidth || IsVariableHeight;

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
            get => pinned;
            set
            {
                pinned = value;
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

            Control? parentFillControl = GetParentFillControl();

            if (parentFillControl != null)
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

        private Control? GetParentFillControl()
        {
            if (Parent != null)
                foreach (Control control in Parent.Controls)
                    if (control.Parent == Parent && control.Visible && control.Dock == DockStyle.Fill)
                        return control;

            return null;
        }

        public OxBorder? ParentPadding
        {
            get
            {
                Control? parentFillControl = GetParentFillControl();

                if (parentFillControl != null)
                    foreach (Control control in parentFillControl.Controls)
                        if (control is OxBorder border && control.Name == FakePaddingName)
                            return border;

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
            Control? parentFillControl = GetParentFillControl();

            if (parentFillControl == null) 
                return;

            OxBorder? fakePadding = null;

            foreach (Control control in parentFillControl.Controls)
                if (control is OxBorder border && control.Name == FakePaddingName)
                {
                    fakePadding = border;
                    break;
                }

            int fakePaddingSize = OxDockHelper.IsVertical(OxDockHelper.Dock(Dock))
                ? Sider.CalcedHeight + Margins.CalcedSize(OxDock.Top) + Margins.CalcedSize(OxDock.Bottom)
                : Sider.CalcedWidth + Margins.CalcedSize(OxDock.Left) + Margins.CalcedSize(OxDock.Right);


            if (fakePadding != null)
            {
                if (!Visible)
                {
                    fakePadding.Visible = false;
                    return;
                }

                if (fakePadding.Dock == Dock)
                {
                    fakePadding.SetSize(fakePaddingSize);

                    if (Pinned == fakePadding.Visible)
                        fakePadding.Visible = !Pinned;
                }
                else
                {
                    parentFillControl.Controls.Remove(fakePadding);
                    fakePadding = null;
                }
            }

            if (fakePadding == null)
            {
                fakePadding = OxBorder.New(
                    parentFillControl,
                    Dock,
                    parentFillControl.BackColor,
                    fakePaddingSize,
                    !Pinned
                );
                fakePadding.Name = FakePaddingName;
            }

            if (fakePadding.Visible)
                fakePadding.SendToBack();
        }

        private void MouseEnterHandler(object? sender, EventArgs e)
        {
            waiter.Start();
            waiter.Ready = Visible && !Expanded && !pinned;
        }

        private readonly OxWaiter waiter;

        private void MouseLeaveHandler(object? sender, EventArgs e) => 
            waiter.Ready = Visible && Expanded && !pinned;

        private void CheckExpandedState()
        {
            bool onPanel = ClientRectangle.Contains(PointToClient(MousePosition));

            if (Expanded != onPanel)
                Expanded = onPanel;
        }

        private int StopWaiter()
        {
            waiter.Stop();

            if (Pinned)
                return 1;

            if (Parent == null || !Created || Disposing)
                return 2;

            BeginInvoke(CheckExpandedState);
            return 0;
        }

        private bool isSimplePanel = false;
        public bool IsSimplePanel
        {
            get => isSimplePanel;
            set
            {
                isSimplePanel = value;
                Sider.Visible = !isSimplePanel;

                if (isSimplePanel)
                {
                    waiter.Stop();
                    Pinned = true;
                    Expanded = true;
                    Borders[OxDockHelper.Dock(Sider.Dock)].Visible = IsSimplePanel;
                }
            }
        }
    }

    public abstract class FunctionsPanel<TField, TDAO> : FunctionsPanel<DAOSettings<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, IFieldMapping<TField>, new()
    {
        protected DAOObserver<TField, TDAO> Observer => Settings.Observer;
    }
}