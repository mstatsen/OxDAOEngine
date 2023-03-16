using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxXMLEngine.Data;
using OxXMLEngine.Settings;

namespace OxXMLEngine.ControlFactory
{
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
            CustomizeButton.Click += CustomizeButtonClick;
            Header.AddToolButton(CustomizeButton);

            base.PrepareInnerControls();
            loadingPanel.Parent = ContentContainer;
            loadingPanel.FontSize = 18;
            loadingPanel.Margins.SetSize(OxSize.None);
            loadingPanel.Borders.SetSize(OxSize.None);
            loadingPanel.Borders.LeftOx = OxSize.None;
            loadingPanel.BringToFront();

            Sider.Parent = this;
            SiderButton.Parent = Sider;
            PinButton.Parent = Sider;
            PinButton2.Parent = Sider;
        }

        public OxPanel Sider { get; } = new(new Size(16, 1));
        private readonly OxIconButton SiderButton = new(OxIcons.left, 16)
        {
            Dock = DockStyle.Fill,
            HiddenBorder = false
        };

        private readonly OxIconButton PinButton = new(OxIcons.unpin, 16)
        {
            Dock = DockStyle.Left,
            HiddenBorder = false
        };

        private readonly OxIconButton PinButton2 = new(OxIcons.unpin, 16)
        {
            Dock = DockStyle.Right,
            HiddenBorder = false
        };

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            RecalcPinned();
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
                Header.BaseColor = Colors.Darker(1);

            SiderButton.BaseColor = Colors.Darker(GeneralSettings.DarkerHeaders ? 1 : 0);
            PinButton.BaseColor = SiderButton.BaseColor;
            PinButton2.BaseColor = SiderButton.BaseColor;
        }

        protected virtual TSettings Settings => SettingsManager.Settings<TSettings>();
        protected GeneralSettings GeneralSettings => SettingsManager.Settings<GeneralSettings>();
        protected ISettingsObserver GeneralObserver => GeneralSettings.Observer;

        protected virtual SettingsPart SettingsPart => default;

        private void CustomizeButtonClick(object? sender, EventArgs e) =>
            SettingsForm.ShowSettings(Settings, SettingsPart);

        private readonly OxIconButton CustomizeButton = new(OxIcons.settings, 23)
        {
            Font = new Font(Styles.FontFamily, Styles.DefaultFontSize - 1, FontStyle.Bold),
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

        protected override int GetCalcedWidth()
        {
            int calcedWidth = base.GetCalcedWidth();

            if (!OxDockHelper.IsVertical(OxDockHelper.Dock(Dock)))
            {
                calcedWidth += Sider.CalcedWidth;

                if (!Expanded)
                    calcedWidth -= SavedWidth + 1;
            }

            return calcedWidth;
        }

        protected override int GetCalcedHeight()
        {
            int calcedHeight = base.GetCalcedHeight();

            if (OxDockHelper.IsVertical(OxDockHelper.Dock(Dock)))
            {
                calcedHeight += Sider.CalcedHeight;

                if (!Expanded)
                    calcedHeight -= SavedHeight + Header.Height + 1;
            }

            return calcedHeight;
        }

        public override void ReAlignControls()
        {
            ContentContainer.ReAlign();
            Paddings.ReAlign();
            Header.ReAlign();
            Sider.ReAlign();
            Borders.ReAlign();
            Margins.ReAlign();
            SendToBack();
        }

        protected override void OnDockChanged(EventArgs e)
        {
            OxDock oxDock = OxDockHelper.Dock(Dock);
            Sider.Dock = OxDockHelper.Dock(OxDockHelper.Opposite(oxDock));
            Borders[OxDockHelper.Dock(Sider.Dock)].Visible = false;

            Sider.Visible = Dock != DockStyle.Fill && Dock != DockStyle.None;

            if (OxDockHelper.IsVertical(oxDock))
            {
                Sider.SetContentSize(1, 16);
                SiderButton.Borders.VerticalOx = OxSize.Small;
                SiderButton.Borders.HorizontalOx = OxSize.None;
                PinButton.Dock = DockStyle.Left;
                PinButton.Borders.VerticalOx = OxSize.Small;
                PinButton.Borders.LeftOx = OxSize.None;
                PinButton.Borders.RightOx = OxSize.Small;
                PinButton.Width = 24;

                PinButton2.Dock = DockStyle.Right;
                PinButton2.Borders.VerticalOx = OxSize.Small;
                PinButton2.Borders.LeftOx = OxSize.Small;
                PinButton2.Borders.RightOx = OxSize.None;
                PinButton2.Width = 24;
            }
            else
            {
                Sider.SetContentSize(16, 1);
                SiderButton.Borders.VerticalOx = OxSize.None;
                SiderButton.Borders.HorizontalOx = OxSize.Small;
                PinButton.Dock = DockStyle.Top;
                PinButton.Borders.HorizontalOx = OxSize.Small;
                PinButton.Borders.TopOx = OxSize.None;
                PinButton.Borders.BottomOx = OxSize.Small;
                PinButton.Height = 24;
                PinButton2.Dock = DockStyle.Bottom;
                PinButton2.Borders.HorizontalOx = OxSize.Small;
                PinButton2.Borders.TopOx = OxSize.Small;
                PinButton2.Borders.BottomOx = OxSize.None;
                PinButton2.Height = 24;
            }

            SiderButton.Icon = SiderButtonIcon;
            RecalcPinned();
            base.OnDockChanged(e);
        }

        private bool expanded = true;

        private void SetExpanded(bool value)
        {
            expanded = value;

            if (!Expandable)
                return;

            OnExpandedChanged?.Invoke(this, EventArgs.Empty);

            ContentContainer.StartSizeRecalcing();
            try
            {
                Paddings[OxDockHelper.Dock(Dock)].Visible = value;
                Paddings[OxDockHelper.Dock(Sider.Dock)].Visible = value;
                ContentContainer.Visible = value;
                Header.Visible = value || !OxDockHelper.IsVertical(OxDockHelper.Dock(Dock));
                SiderButton.Icon = SiderButtonIcon;
                Borders[OxDockHelper.Dock(Dock)].Visible = value;
            }
            finally
            {
                Update();
                ContentContainer.EndSizeRecalcing();
            }

            if (expanded)
                OnAfterExpand?.Invoke(this, EventArgs.Empty);
            else OnAfterCollapse?.Invoke(this, EventArgs.Empty);
        }

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

        private void SetMouseHandlers()
        {
            SetMouseHandler(this);
            SetMouseHandler(ContentContainer);
            SetMouseHandler(SiderButton);
            SetMouseHandler(SiderButton.Picture);
            SetMouseHandler(SiderButton.Margins);
            SetMouseHandler(SiderButton.Borders);
            SetMouseHandler(SiderButton.Paddings);
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
        }

        protected override void SetHandlers()
        {
            base.SetHandlers();
            ApplyRecalcSizeHandler(ContentContainer, false, true);
            SiderButton.Click += (s, e) => Expanded = !Expanded;
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

        public Bitmap? SiderButtonIcon =>
            Dock switch
            {
                DockStyle.Left => expanded ? OxIcons.left : OxIcons.right,
                DockStyle.Right => expanded ? OxIcons.right : OxIcons.left,
                DockStyle.Top => expanded ? OxIcons.up : OxIcons.down,
                DockStyle.Bottom => expanded ? OxIcons.down : OxIcons.up,
                _ => null,
            };

        public void Expand() => Expanded = true;
        public void Collapse() => Expanded = false;

        public EventHandler? OnPinnedChanged { get; set; }
        public EventHandler? OnExpandedChanged { get; set; }
        public EventHandler? OnAfterExpand { get; set; }
        public EventHandler? OnAfterCollapse { get; set; }


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

        public void RecalcPinned()
        {
            PinButton.FreezeHovered = pinned;
            PinButton.Icon = pinned ? OxIcons.pin : OxIcons.unpin;

            PinButton2.FreezeHovered = pinned;
            PinButton2.Icon = pinned ? OxIcons.pin : OxIcons.unpin;

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
            OnPinnedChanged?.Invoke(this, EventArgs.Empty);
            SetMouseHandlers();
        }

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
                ? Sider.Height + Margins.Top + Margins.Bottom
                : Sider.Width + Margins.Left + Margins.Right;

            if (fakePadding != null)
            {
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

        private void MouseEnterHandler(object? sender, EventArgs e) => 
            waiter.Ready = !Expanded && !pinned;

        private readonly OxWaiter waiter;

        private void MouseLeaveHandler(object? sender, EventArgs e) => 
            waiter.Ready = Expanded && !pinned;

        private void CheckExpandedState() =>
            Expanded = ClientRectangle.Contains(PointToClient(MousePosition));

        private int StopWaiter()
        {
            waiter.Ready = false;

            if (Pinned)
                return 1;

            if (Parent == null || !Created || Disposing)
                return 2;

            BeginInvoke(CheckExpandedState);
            return 0;
        }
    }

    public abstract class FunctionsPanel<TField, TDAO> : FunctionsPanel<DAOSettings<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, IFieldMapping<TField>, new()
    {
        protected DAOObserver<TField, TDAO> Observer => Settings.Observer;
    }
}