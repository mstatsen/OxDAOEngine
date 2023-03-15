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
        }

        public bool ShowSettingsButton
        {
            get => CustomizeButton.Visible;
            set => CustomizeButton.Visible = value;
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

            //BorderVisible = false;
            Sider.Parent = this;
            SiderButton.Parent = Sider;
        }

        private readonly OxPanel Sider = new(new Size(16, 1));
        private readonly OxIconButton SiderButton = new(OxIcons.left, 16)
        {
            Dock = DockStyle.Fill,
            HiddenBorder = false
        };

        protected override void ParentChangedHandler(object? sender, EventArgs e)
        {
            base.ParentChangedHandler(sender, e);

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
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();

            loadingPanel.BaseColor = BaseColor;

            if (GeneralSettings.DarkerHeaders)
                Header.BaseColor = Colors.Darker(1);

            SiderButton.BaseColor = Colors.Darker(GeneralSettings.DarkerHeaders ? 1 : 0);
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

            if (OxDockHelper.IsVertical(oxDock))
                Sider.SetContentSize(1, 16);
            else Sider.SetContentSize(16, 1);

            Sider.Visible = Dock != DockStyle.Fill && Dock != DockStyle.None;
            SiderButton.Icon = SiderButtonIcon;


            if (OxDockHelper.IsVertical(OxDockHelper.Dock(Dock)))
            {
                SiderButton.Borders.VerticalOx = OxSize.Small;
                SiderButton.Borders.HorizontalOx = OxSize.None;
            }
            else
            {
                SiderButton.Borders.VerticalOx = OxSize.None;
                SiderButton.Borders.HorizontalOx = OxSize.Small;
            }

            base.OnDockChanged(e);
        }

        private bool expanded = true;

        private void ExpandHandler(object? sender, EventArgs e) =>
            Expanded = !Expanded;

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
            SetMouseHandler(Margins);
            SetMouseHandler(Borders);
            SetMouseHandler(Paddings);
        }

        protected override void SetHandlers()
        {
            base.SetHandlers();
            ApplyRecalcSizeHandler(ContentContainer, false, true);
            SiderButton.Click += ExpandHandler;
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

        public EventHandler? OnExpandedChanged { get; set; }
        public EventHandler? OnAfterExpand { get; set; }
        public EventHandler? OnAfterCollapse { get; set; }

        private bool pinned = false;
        public bool Pinnded
        {
            get => pinned;
            set
            {
                pinned = value;
                SetMouseHandlers();
            }
        }

        private void MouseEnterHandler(object? sender, EventArgs e)
        {
            if (Expanded)
                return;

            RestartWaiter();
        }

        private OxWaiter? waiter;

        private void MouseLeaveHandler(object? sender, EventArgs e)
        {
            if (!Expanded)
                return;

            RestartWaiter();
        }

        private void RestartWaiter()
        {
            if (waiter != null)
                waiter.Stop();

            waiter = new OxWaiter(StopWiater);
            waiter.Start();
        }

        private void CheckExpandedState() =>
            Expanded = ClientRectangle.Contains(PointToClient(MousePosition));

        private int StopWiater()
        {
            if (waiter != null)
                waiter.Stop();

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