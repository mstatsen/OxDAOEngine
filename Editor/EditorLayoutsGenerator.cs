using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;
using OxLibrary;

namespace OxDAOEngine.Editor
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

        public virtual List<TField>? ControlsWithoutLabel() => null;

        public virtual List<TField>? AutoSizeFields() => null;

        public virtual List<TField>? FillDockFields() => null;

        public virtual List<TField>? OffsettingFields() => null;

        public virtual List<TField> TitleAccordionFields() => default!;

        public virtual TField BackColorField => default!;

        public bool IsWithoutLabelField(TField field)
        {
            List<TField>? fields = ControlsWithoutLabel();
            return 
                fields is not null 
                && fields.Contains(field);
        }

        public ControlCaptionVariant CaptionVariant(TField field) => 
            IsWithoutLabelField(field)
                ? ControlCaptionVariant.None
                : ControlCaptionVariant.Left;

        public virtual AnchorStyles Anchors(TField field) =>
            AnchorStyles.Left | AnchorStyles.Top;

        public virtual Color BackColor(TField field) => 
            Color.FromArgb(250, 250, 250);

        public bool AutoSize(TField field)
        {
            List<TField>? fields = AutoSizeFields();
            return 
                fields is not null 
                && fields.Contains(field);
        }
        private OxDock Dock(TField field)
        {
            List<TField>? fillDockFields = FillDockFields();
            return 
                fillDockFields is not null
                && fillDockFields.Contains(field)
                    ? OxDock.Fill
                    : OxDock.None;
        }
        public abstract OxWidth Top(TField field);
        public abstract OxWidth Left(TField field);
        public virtual OxWidth MaximumLabelWidth(TField field) =>
            OxWh.W64;
        public virtual bool WrapLabel(TField field) => false;
        public virtual OxWidth Height(TField field) => OxWh.W28;
        public abstract OxWidth Width(TField field);

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

        public virtual bool IsCheckBoxField(TField field) => false;

        public bool IsOffsettingField(TField field)
        {
            List<TField>? fields = OffsettingFields();
            return 
                fields is not null 
                && fields.Contains(field);
        }

        public virtual int Offset(TField field) => 2;

        public OxPanel Parent(TField field) =>
            GroupFrames[fieldGroupHelper.EditedGroup(field)];

        private readonly FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();
        private readonly FieldGroupHelper<TField, TFieldGroup> fieldGroupHelper = 
            TypeHelper.FieldGroupHelper<TField, TFieldGroup>();
        public FieldGroupFrames<TField, TFieldGroup> GroupFrames;
        public ControlLayouter<TField, TDAO> Layouter;
    }
}