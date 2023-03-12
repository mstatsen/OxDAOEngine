using OxLibrary.Panels;
using OxXMLEngine.ControlFactory;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Editor
{
    public abstract class EditorLayoutsGenerator<TField, TDAO, TFieldGroup>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TFieldGroup : notnull, Enum
    {
        public EditorLayoutsGenerator(FieldGroupFrames<TField, TFieldGroup> groupFrames, 
            ControlLayouter<TField, TDAO> layouter)
        {
            GroupFrames = groupFrames;
            Layouter = layouter;
        }

        protected virtual void BeforeGenerateLayouts() { }
        protected virtual void AfterGenerateLayouts() { }

        public void GenerateLayouts()
        {
            Layouter.Layouts.Clear();
            BeforeGenerateLayouts();

            foreach (TField field in fieldHelper.EditingFields)
                GenerateLayout(field);

            AfterGenerateLayouts();
        }

        protected virtual List<TField>? ControlsWithoutLabel() => null;

        protected virtual List<TField>? AutoSizeFields() => null;

        protected virtual List<TField>? FillDockFields() => null;

        protected virtual List<TField>? OffsettingFields() => null;

        private bool IsWithoutLabelField(TField field)
        {
            List<TField>? fields = ControlsWithoutLabel();
            return fields != null && fields.Contains(field);
        }

        private ControlCaptionVariant CaptionVariant(TField field) => 
            IsWithoutLabelField(field)
                ? ControlCaptionVariant.None
                : ControlCaptionVariant.Left;

        protected virtual AnchorStyles Anchors(TField field) =>
            AnchorStyles.Left | AnchorStyles.Top;

        protected virtual Color BackColor(TField field) => 
            Color.FromArgb(250, 250, 250);

        private bool AutoSize(TField field)
        {
            List<TField>? fields = AutoSizeFields();
            return fields != null && fields.Contains(field);
        }
        private DockStyle Dock(TField field)
        {
            List<TField>? fillDockFields = FillDockFields();
            return fillDockFields != null 
                && fillDockFields.Contains(field)
                ? DockStyle.Fill
                : DockStyle.None;
        }
        protected abstract int Top(TField field);
        protected abstract int Left(TField field);
        protected virtual int MaximumLabelWidth(TField field) =>
            64;
        protected virtual bool WrapLabel(TField field) => false;
        protected virtual int Height(TField field) => 28;
        protected abstract int Width(TField field);

        private void GenerateLayout(TField field)
        {
            ControlLayout<TField> layout = Layouter.Layouts.AddFromTemplate(field, Offset(field));
            layout.Parent = Parent(field);
            layout.CaptionVariant = CaptionVariant(field);
            layout.Dock = Dock(field);
            layout.Left = Left(field);

            if (!IsOffsettingField(field))
                layout.Top = Top(field);

            layout.Width = Width(field);
            layout.Height = Height(field);
            layout.AutoSize = AutoSize(field);
            layout.BackColor = BackColor(field);
            layout.Anchors = Anchors(field);
            layout.MaximumLabelWidth = MaximumLabelWidth(field);
            layout.WrapLabel = WrapLabel(field);

            if (fieldHelper.MandatoryFields.Contains(field))
            {
                layout.LabelStyle = FontStyle.Bold;

                if (IsWithoutLabelField(field))
                    layout.FontStyle = FontStyle.Bold;
            }
        }

        protected virtual bool IsCheckBoxField(TField field) => false;

        private bool IsOffsettingField(TField field)
        {
            List<TField>? fields = OffsettingFields();
            return fields != null && fields.Contains(field);
        }

        protected virtual int Offset(TField field) => 2;

        protected OxPane Parent(TField field) =>
            GroupFrames[fieldGroupHelper.EditedGroup(field)];

        private readonly FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();
        private readonly FieldGroupHelper<TField, TFieldGroup> fieldGroupHelper = 
            TypeHelper.FieldGroupHelper<TField, TFieldGroup>();
        public FieldGroupFrames<TField, TFieldGroup> GroupFrames;
        public ControlLayouter<TField, TDAO> Layouter;
    }
}