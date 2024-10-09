using OxLibrary.Dialogs;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;
using OxLibrary;

namespace OxDAOEngine.Editor
{
    public abstract class DAOWorker<TField, TDAO, TFieldGroup>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TFieldGroup : notnull, Enum
    {
        public bool Modified => !InitialItem.Equals(CurrentItem);


        protected readonly TDAO CurrentItem = new();
        public DAOEditor<TField, TDAO, TFieldGroup> Editor => DataManager.Editor<TField, TDAO, TFieldGroup>();

        public TDAO? Item
        {
            get => item;
            set
            {
                item = value;
                InitialItem.CopyFrom(item);
                CurrentItem.CopyFrom(item);
                LayoutControls();
                FillControls();
                SyncFieldsAndRecalcGroups();
            }
        }

        private void SyncFieldsAndRecalcGroups()
        {
            FillFormCaption();

            if (SyncAllFields())
                RecalcGroupsAvailability();
        }

        protected virtual FieldGroupFrames<TField, TFieldGroup> GetFieldGroupFrames() => 
            Editor.Groups;

        private readonly FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

        protected List<TField> EditingFields => fieldHelper.EditingFields;

        protected abstract EditorLayoutsGenerator<TField, TDAO, TFieldGroup> CreateLayoutsGenerator(
            FieldGroupFrames<TField, TFieldGroup> frames, ControlLayouter<TField, TDAO> layouter);

        protected EditorLayoutsGenerator<TField, TDAO, TFieldGroup> Generator { get; private set; }

        private void GenerateLayouts()
        {
            BeforeGenerateLayouts();
            Generator!.GenerateLayouts();
            AfterGenerateLayouts();
        }

        protected virtual void AfterGenerateLayouts() { }

        protected virtual void BeforeGenerateLayouts() { }

        private void AlignLabels()
        {
            foreach (List<TField> list in LabelGroups)
                Layouter.AlignLabels(list);

            AfterAlignLabels();
        }

        protected virtual void AfterAlignLabels() { }
        protected virtual void AfterLayoutControls() { }

        protected virtual List<List<TField>> LabelGroups => new();

        public void LayoutControls()
        {
            Layouter.Clear();
            GenerateLayouts();
            Layouter.LayoutControls();
            AfterLayoutControls();
            AlignLabels();
        }

        protected void FillControls()
        {
            UnSetHandlers();
            BeforeFillControls();
            Builder.FillControls(CurrentItem);
            AfterFillControls();
            SetHandlers();
            AfterFillControlsAndSetHandlers();
            SetGroupsAvailability();
        }

        protected virtual void BeforeFillControls() { }

        protected virtual void AfterFillControls() { }

        protected virtual void AfterFillControlsAndSetHandlers() { }

        private readonly ControlFactory<TField, TDAO> controlFactory = 
            DataManager.ControlFactory<TField, TDAO>();

        public bool CheckMandatoryFields()
        {
            foreach (TField field in fieldHelper.MandatoryFields)
            {
                if (controlFactory.GetFieldControlType(field) == FieldType.Image)
                    continue;

                IControlAccessor accessor = Builder[field];

                if (accessor == null)
                    return true;

                if (accessor.IsEmpty)
                {
                    OxMessage.ShowError($"{TypeHelper.Name(field)} is a required field", Editor);
                    accessor.Control.Focus();
                    return false;
                }
            }

            return true;
        }

        protected virtual void PrepareStyles() { }

        private void PrepareStylesInternal()
        {
            SetMainPanelColor();
            PrepareStyles();
            ColorizeControls();
            AfterColorizeControls();
        }

        private void SetMainPanelColor()
        {
            if (!Generator.BackColorField.Equals(default!))
                Editor.MainPanel.BaseColor = new OxColorHelper(
                    TypeHelper.BackColor(Builder[Generator.BackColorField].Value)
                ).Darker(7);
        }

        protected virtual void AfterColorizeControls() { }

        private void ColorizeControls()
        {
            foreach (TField field in EditingFields)
                ControlPainter.ColorizeControl(
                    Builder.Control(field),
                    Editor.MainPanel.BaseColor
                );
        }

        private void FillFormCaption()
        {
            TDAO itemForCaption = new();

            foreach (TField field in Generator!.TitleAccordionFields())
                itemForCaption[field] = Builder[field].Value;

            Editor.Text = itemForCaption.FullTitle();
        }

        private TDAO? item;
        private readonly TDAO InitialItem = new();

        public DAOWorker()
        {
            Layouter = Builder.Layouter;
            Generator = CreateLayoutsGenerator(GetFieldGroupFrames(), Layouter);
        }

        private ControlBuilder<TField, TDAO> builder = default!;
        protected ControlBuilder<TField, TDAO> Builder
        {
            get
            { 
                builder ??= controlFactory.Builder(ControlScope.Editor);
                return builder;
            }
        }

        protected readonly ControlLayouter<TField, TDAO> Layouter;

        public void GrabControls()
        {
            if (item != null)
            {
                BeforeGrabControls();
                Builder.GrabControls(item);
                AfterGrabControls();
            }

            InitialItem.CopyFrom(item);
        }

        protected virtual void AfterGrabControls() { }
        protected virtual void BeforeGrabControls() { }

        public void UnSetHandlers()
        {
            foreach (TField field in EditingFields)
                Builder[field].ValueChangeHandler -= ValueChangeHandler;
        }

        public void SetHandlers()
        {
            foreach (TField field in EditingFields)
                Builder[field].ValueChangeHandler += ValueChangeHandler;
        }

        private bool SyncAllFields()
        {
            bool needRecalcGroupsAvailability = false;

            foreach (TField field in EditingFields)
                needRecalcGroupsAvailability |= 
                    SyncFieldValue(field, false);

            Builder.ApplyDependencies();
            PrepareStylesInternal();
            return needRecalcGroupsAvailability;
        }

        private void ValueChangeHandler(object? sender, EventArgs e) 
        {
            Builder.ApplyDependencies();

            bool needRecalcGroupsAvailability = false;

            foreach (TField field in EditingFields)
                if (Builder.Control(field) == sender)
                {
                    if (Generator!.TitleAccordionFields().Contains(field))
                        FillFormCaption();

                    needRecalcGroupsAvailability |=
                        SyncFieldValue(field, true);
                }

            if (needRecalcGroupsAvailability)
                RecalcGroupsAvailability();
            else PrepareStylesInternal();

            Builder.GrabControls(CurrentItem);
        }

        private void RecalcGroupsAvailability()
        {
            Editor.SuspendLayout();
            bool parentsVisibleChanged = Editor.SetParentsVisible(true);
            bool groupsAvailabilityChanged = SetGroupsAvailability(true);
            Editor.Groups.SetGroupsSize();

            if (Editor.SetParentsVisible(false) || parentsVisibleChanged || groupsAvailabilityChanged)
                Editor.InvalidateSize();

            PrepareStylesInternal();
            Editor.ResumeLayout();
        }

        protected virtual bool SyncFieldValue(TField field, bool byUser) => false;
        protected virtual bool SetGroupsAvailability(bool afterSyncValues = false) => false;
    }
}