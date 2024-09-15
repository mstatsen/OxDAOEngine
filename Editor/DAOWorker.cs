using OxLibrary.Dialogs;
using OxXMLEngine.ControlFactory;
using OxXMLEngine.ControlFactory.Accessors;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Editor
{
    public abstract class DAOWorker<TField, TDAO, TFieldGroup>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TFieldGroup : notnull, Enum
    {
        public bool Modified =>
            !initialItem.Equals(Item);

        public DAOEditor<TField, TDAO, TFieldGroup> Editor =>
            DataManager.Editor<TField, TDAO, TFieldGroup>();

        public TDAO? Item
        {
            get => item;
            set
            {
                if (initialItem.Equals(value))
                    return;

                item = value;
                initialItem.CopyFrom(item);
                FillFormCaption(item);
                LayoutControls();
                FillControls();

                if (SyncAllFields())
                    RecalcGroupsAvailability();
            }
        }

        protected virtual FieldGroupFrames<TField, TFieldGroup> GetFieldGroupFrames() => 
            Editor.Groups;

        private readonly FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

        protected List<TField> EditingFields => fieldHelper.EditingFields;

        protected abstract EditorLayoutsGenerator<TField, TDAO, TFieldGroup> CreateLayoutsGenerator(
            FieldGroupFrames<TField, TFieldGroup> frames, ControlLayouter<TField, TDAO> layouter);

        private EditorLayoutsGenerator<TField, TDAO, TFieldGroup>? generator;

        private void GenerateLayouts()
        {
            BeforeGenerateLayouts();
            generator ??= CreateLayoutsGenerator(GetFieldGroupFrames(), Layouter);
            generator.GenerateLayouts();
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

        private void FillControls()
        {
            UnSetHandlers();
            BeforeFillControls();

            if (Item != null)
                Builder.FillControls(Item);

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
                    OxMessage.ShowError($"{TypeHelper.Name(field)} is mandatory");
                    accessor.Control.Focus();
                    return false;
                }
            }

            return true;
        }

        protected abstract void PrepareStyles();
        private void PrepareStylesInternal()
        {
            PrepareStyles();
            ColorizeControls();
            AfterColorizeControls();
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

        protected void FillFormCaption(TDAO? itemForCaption) =>
            Editor.Text = itemForCaption != null ? itemForCaption.FullTitle() : "Unknown DAO";

        private TDAO? item;
        private readonly TDAO initialItem = new();
        public DAOWorker()
        {
            Builder = controlFactory.Builder(ControlScope.Editor);
            Layouter = Builder.Layouter;
        }

        protected readonly ControlBuilder<TField, TDAO> Builder;

        protected readonly ControlLayouter<TField, TDAO> Layouter;

        public void GrabControls()
        {
            if (Item != null)
            {
                BeforeGrabControls();
                Builder.GrabControls(Item);
                AfterGrabControls();
            }

            initialItem.CopyFrom(Item);
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
                    SyncFieldValues(field, false);

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
                    needRecalcGroupsAvailability |= 
                        SyncFieldValues(field, true);

            if (needRecalcGroupsAvailability)
                RecalcGroupsAvailability();
            else PrepareStylesInternal();
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

        protected virtual bool SyncFieldValues(TField field, bool byUser) => false;
        protected virtual bool SetGroupsAvailability(bool afterSyncValues = false) => false;
    }
}