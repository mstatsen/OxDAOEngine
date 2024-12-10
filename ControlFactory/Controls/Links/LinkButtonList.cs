using OxLibrary.Panels;
using OxLibrary;
using OxLibrary.Handlers;
using OxLibrary.Geometry;

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
                        Dock = OxDock.None;
                        break;
                    case ButtonListDirection.Horizontal:
                        Dock = OxDock.Top;
                        Padding.Top = 8;
                        break;
                }
            }
        }

        public LinkButtonList() : base() { }

        public readonly List<LinkButton> Buttons = new();
        private readonly short ButtonSpace = 2;
        private readonly short ButtonHeight = 22;

        private short LastBottom =>
            OxSH.Short(Buttons.Count > 0 ? Buttons[^1].Bottom : 0);

        private short LastRight =>
            OxSH.Short(Buttons.Count > 0 ? Buttons[^1].Right : 0);

        private short ButtonLeft() =>
            OxSH.Short(
                Direction is ButtonListDirection.Horizontal
                    ? LastRight + ButtonSpace
                    : 0
            );

        private short ButtonTop() =>
            OxSH.Short(
                Direction is ButtonListDirection.Horizontal
                    ? 0
                    : LastBottom + ButtonSpace + 1
            );

        public void RecalcButtonsSizeAndPositions()
        {
            RecalcButtonsSize();
            RecalcButtonsPositions();
        }

        private void RecalcButtonsPositions()
        {
            if (Direction is not ButtonListDirection.Horizontal)
                return;

            foreach (LinkButton button in Buttons)
                button.Left = OxSH.Mul(
                    Buttons.IndexOf(button), 
                    button.Width + ButtonSpace
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

        public override void OnSizeChanged(OxSizeChangedEventArgs e)
        {
            if (!e.Changed)
                return;

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

        private void SetButtonSize(LinkButton button) => 
            button.Size = new
            (
                    Direction is ButtonListDirection.Vertical
                        ? Width - button.Borders.Left
                        : Buttons.Count is 0
                            ? 120
                            : OxSH.Sub(
                                    OxSH.Min(Width / Buttons.Count, 120),
                                    ButtonSpace * (Buttons.Count - 1),
                                    button.Borders.Left + button.Borders.Right
                              ),
                ButtonHeight
            );

        public void Clear()
        {
            foreach (LinkButton button in Buttons)
                button.Parent = null;

            Buttons.Clear();
            RecalcButtonsSize();
        }

        private void RecalcHeight() =>
            MinimumSize = new(0, LastBottom + 3);

        private void RecalcWidth() => 
            MinimumSize = new(
                OxDockHelper.IsVertical(Dock)
                    ? Width
                    : LastRight,
                40
            );
    }
}