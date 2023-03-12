using OxLibrary;

namespace OxXMLEngine
{
    public static class EngineStyles
    {
        public static readonly Color ElementControlColor = Color.FromArgb(235, 241, 241);
        public static readonly Color DefaultGridRowColor = Color.FromArgb(254, 254, 255);
        public static readonly Color DefaultGridFontColor = Color.Black;
        public static DataGridViewCellStyle Cell_Default { get; private set; }
        public static DataGridViewCellStyle Cell_LeftAlignment { get; private set; }
        public static Font DefaultFont { get; private set; }
        public const int DefaultControlHeight = 24;

        public static readonly Color QuickFilterColor = Color.FromArgb(135, 165, 195);
        public static readonly Color QuickFilterTextColor = Color.FromArgb(145, 175, 205);
        public static readonly Color SortingColor = Color.FromArgb(135, 195, 135);
        public static readonly Color GroupByColor = Color.FromArgb(140, 185, 140);
        public static readonly Color BatchUpdateColor = Color.FromArgb(205, 140, 140);
        public static readonly Color FieldsColor = Color.FromArgb(195, 145, 195);
        public static readonly Color CategoryColor = Color.FromArgb(180, 140, 120);
        public static readonly Color SummaryColor = Color.FromArgb(150, 170, 120);
        public static readonly Color InlineColor = Color.FromArgb(185, 155, 185);
        public static readonly Color SettingsFormColor = Color.FromArgb(150, 150, 130);
        public static readonly Color CardColor = DefaultGridRowColor;

        static EngineStyles()
        {
            Cell_Default = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                WrapMode = DataGridViewTriState.True,
                Font = new Font(Styles.FontFamily, 10),
            };

            Cell_LeftAlignment = Cell_Default.Clone();
            Cell_LeftAlignment.Alignment = DataGridViewContentAlignment.MiddleLeft;
            DefaultFont = new Font(Styles.FontFamily, Styles.DefaultFontSize, FontStyle.Regular);
        }
    }
}