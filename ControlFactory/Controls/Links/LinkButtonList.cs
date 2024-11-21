using OxLibrary;
using OxLibrary.Panels;

namespace OxDAOEngine.ControlFactory.Controls.Links
{
    public class LinkButtonList : OxPane
    {
        private ButtonListDirection direction = ButtonListDirection.Vertical;
        public ButtonListDirection Direction
        {
            get => direction;
            set
            {
                direction = value;

                switch (direction)
                {
                    case ButtonListDirection.Vertical:
                        Dock = OxDock.None;
                        break;
                    case ButtonListDirection.Horizontal:
                        Dock = OxDock.Top;
                        Padding.Top = OxWh.W8;
                        break;
                }
            }
        }

        public LinkButtonList() : base() { }

        public readonly List<LinkButton> Buttons = new();
        private readonly OxWidth ButtonSpace = OxWh.W2;
        private readonly OxWidth ButtonHeight = OxWh.W22;

        private OxWidth LastBottom =>
            Buttons.Count > 0 
                ? Buttons[^1].Bottom
                : OxWh.W0;

        private OxWidth LastRight =>
            Buttons.Count > 0
                ? Buttons[^1].Right
                : OxWh.W0;

        private OxWidth ButtonLeft() =>
            Direction is ButtonListDirection.Horizontal
                ? LastRight | ButtonSpace
                : OxWh.W0;

        private OxWidth ButtonTop() =>
            Direction is ButtonListDirection.Horizontal
                ? OxWh.W0
                : LastBottom | ButtonSpace | OxWh.W1;

        public void RecalcButtonsSizeAndPositions()
        {
            RecalcButtonsSize();
            RecalcButtonsPositions();
        }

        private void RecalcButtonsPositions()
        {
            if (Direction is ButtonListDirection.Horizontal)
                foreach (LinkButton button in Buttons)
                    button.Left = 
                        OxWh.Mul(
                            Buttons.IndexOf(button), 
                            button.Width | ButtonSpace
                        );
        }

        private void RecalcButtonsSize()
        {
            switch (Direction)
            {
                case ButtonListDirection.Vertical:
                    RecalcHeight();
                    break;
                case ButtonListDirection.Horizontal:
                    RecalcWidth();
                    break;
            }
        }

        public void AddButton(LinkButton button)
        {
            if (button is null)
                return;

            button.Parent = this;
            button.Left = ButtonLeft();
            button.Top = ButtonTop();
            Buttons.Add(button);
            SetButtonsSize();
            RecalcButtonsSizeAndPositions();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RecalcButtonsSize();
            SetButtonsSize();
            RecalcButtonsPositions();
        }

        private void SetButtonsSize()
        {
            foreach (LinkButton button in Buttons)
                SetButtonSize(button);
        }

        private void SetButtonSize(LinkButton button)
        {
            OxWidth calcedWidth =
                Direction is ButtonListDirection.Vertical
                    ? OxWh.Sub(Width, OxWh.Div(button.BorderWidth, 2))
                    : (Buttons.Count is 0
                        ? OxWh.W120
                        : OxWh.Sub(
                            OxWh.Sub(
                                OxWh.Min(OxWh.Div(Width, Buttons.Count), OxWh.W120),
                                OxWh.Mul(ButtonSpace, Buttons.Count - 1)
                            ),
                            OxWh.Mul(button.BorderWidth, 2)
                          )
                      );

            button.Size = new
            (
                calcedWidth,
                ButtonHeight
            );
        }

        public void Clear()
        {
            foreach (LinkButton button in Buttons)
                button.Parent = null;

            Buttons.Clear();
            RecalcButtonsSize();
        }

        private void RecalcHeight() =>
            MinimumSize = new(OxWh.W0, LastBottom | OxWh.W3);

        private void RecalcWidth() => 
            MinimumSize = new(
                OxDockHelper.IsVertical(Dock)
                    ? Width
                    : LastRight,
                OxWh.W40
            );
    }
}