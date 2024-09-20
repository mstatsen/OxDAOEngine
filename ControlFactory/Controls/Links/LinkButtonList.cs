using OxLibrary.Panels;

namespace OxDAOEngine.ControlFactory.Controls
{
    public class LinkButtonList : OxPanel
    {
        public ButtonListDirection Direction = ButtonListDirection.Vertical;

        public LinkButtonList() : base() { }

        public readonly List<LinkButton> Buttons = new();
        private const int ButtonSpace = 3;
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
                : LastBottom + ButtonSpace;

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
            SetButtonSize(button);
            Buttons.Add(button);
            RecalcButtonsSize();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            foreach (LinkButton button in Buttons)
                SetButtonSize(button);
        }

        private void SetButtonSize(LinkButton button) =>
            button.SetContentSize(
                (Direction == ButtonListDirection.Vertical
                    ? Width
                    : 120)
                    - button.BorderWidth * 2,
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
            MinimumSize = new Size(0, LastBottom + 2);

        private void RecalcWidth() =>
            MinimumSize = new Size(LastRight, ButtonHeight);
    }
}