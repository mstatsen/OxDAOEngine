﻿using OxLibrary.Panels;
using OxLibrary;
using OxLibrary.Handlers;

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
            (short)(Buttons.Count > 0
                ? Buttons[^1].Bottom
                : 0);

        private short LastRight =>
            (short)(Buttons.Count > 0
                ? Buttons[^1].Right
                : 0);

        private short ButtonLeft() =>
            (short)(Direction is ButtonListDirection.Horizontal
                ? LastRight + ButtonSpace
                : 0);

        private short ButtonTop() =>
            (short)(Direction is ButtonListDirection.Horizontal
                ? 0
                : LastBottom + ButtonSpace | 1);

        public void RecalcButtonsSizeAndPositions()
        {
            RecalcButtonsSize();
            RecalcButtonsPositions();
        }

        private void RecalcButtonsPositions()
        {
            if (Direction is ButtonListDirection.Horizontal)
                foreach (LinkButton button in Buttons)
                    button.Left = (short)(Buttons.IndexOf(button) * (button.Width + ButtonSpace));
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

        private void SetButtonSize(LinkButton button)
        {
            short calcedWidth =
                (short)(Direction is ButtonListDirection.Vertical
                    ? Width - button.Borders.Left
                    : (Buttons.Count is 0
                        ? 120
                        : Math.Min(Width / Buttons.Count, 120)
                            - ButtonSpace * (Buttons.Count - 1)
                            ) 
                        - (button.Borders.Left + button.Borders.Right));

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
            MinimumSize = new(0, (short)(LastBottom + 3));

        private void RecalcWidth() => 
            MinimumSize = new(
                OxDockHelper.IsVertical(Dock)
                    ? Width
                    : LastRight,
                40
            );
    }
}