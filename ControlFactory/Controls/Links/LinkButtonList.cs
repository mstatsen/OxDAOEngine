using OxLibrary;
using OxLibrary.Panels;

namespace OxDAOEngine.ControlFactory.Controls.Links
{
    public class LinkButtonList : OxPanel
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
                        Dock = DockStyle.None;
                        break;
                    case ButtonListDirection.Horizontal:
                        Dock = DockStyle.Top;
                        Paddings.TopOx = OxSize.Extra;
                        break;
                }
            }
        }

        public LinkButtonList() : base() { }

        public readonly List<LinkButton> Buttons = new();
        private const int ButtonSpace = 2;
        private const int ButtonHeight = 22;

        private int LastBottom =>
            Buttons.Count > 0 ? Buttons[^1].Bottom : 0;

        private int LastRight =>
            Buttons.Count > 0 ? Buttons[^1].Right : 0;

        private int ButtonLeft() =>
            Direction == ButtonListDirection.Horizontal
                ? LastRight + ButtonSpace
                : 0;

        private int ButtonTop() =>
            Direction == ButtonListDirection.Horizontal
                ? 0
                : LastBottom + ButtonSpace + 1;

        public void RecalcButtonsSizeAndPositions()
        {
            RecalcButtonsSize();
            RecalcButtonsPositions();
        }

        private void RecalcButtonsPositions()
        {
            if (Direction == ButtonListDirection.Horizontal)
                foreach (LinkButton button in Buttons)
                    button.Left = Buttons.IndexOf(button) * (button.Width + ButtonSpace);
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
            int calcedWidth =
                Direction == ButtonListDirection.Vertical
                    ? Width - button.BorderWidth * 2
                    : (Buttons.Count == 0
                        ? 120
                        : Math.Min(Width / Buttons.Count, 120)
                            - ButtonSpace * (Buttons.Count - 1))
                    - button.BorderWidth * 2;

            button.SetContentSize(
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
            MinimumSize = new Size(0, LastBottom + 3);

        private void RecalcWidth()
        {
            MinimumSize = new Size(
                OxDockHelper.IsVertical(OxDockHelper.Dock(Dock))
                    ? Width
                    : LastRight,
                40
            );
        }
    }
}