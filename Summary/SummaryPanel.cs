using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings;

namespace OxDAOEngine.Summary
{
    public delegate void SummaryPanelHandler(ISummaryPanel panel);

    public class SummaryPanel<TField, TDAO> : OxCard, ISummaryPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TField Field { get; internal set; }
        public AccessorDictionary<TField, TDAO> ValueAccessors = new();

        public SummaryPanel(TField field) : base()
        {
            Field = field;
            Text = "by " + TypeHelper.Name(field);
        }

        public void AlignAccessors() =>
            ValueAccessors.AlignAccessors();

        public void ClearAccessors() =>
            ValueAccessors.Clear();

        public void CalcPanelSize()
        {
            int maxBottom = 0;

            foreach (IControlAccessor accessor in ValueAccessors.Values)
                maxBottom = Math.Max(accessor.Bottom, maxBottom);

            Header.SetContentSize(SummaryConsts.CardWidth, SummaryConsts.CardHeaderHeight);
            SetContentSize(
                SummaryConsts.CardWidth,
                Math.Max(SummaryConsts.CardHeight, maxBottom + SummaryConsts.CardHeaderHeight)
            );
        }

        public void FillAccessors()
        {
            ClearAccessors();
            CreateAccessors(
                new FieldExtractor<TField, TDAO>(
                    DataManager.VisibleItemsList<TField, TDAO>()).CountExtract(
                    Field, 
                    true,
                    SettingsManager.DAOSettings<TField>().SummarySorting
                )
            );
        }

        public override Color DefaultColor => EngineStyles.SummaryColor;

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Dock = DockStyle.Top;
        }

        private void CreateAccessors(Dictionary<object, int> extraction)
        {
            Point nextLocation = new(
                SummaryConsts.HorizontalSpace, 
                SummaryConsts.VerticalSpace
            );

            foreach (var extractItem in extraction)
            {
                IControlAccessor accessor = ValueAccessors.CreateAccessor(
                    extractItem.Key,
                    ContentContainer,
                    TypeHelper.Name(extractItem.Key),
                    extractItem.Value,
                    nextLocation
                );

                nextLocation.Y = accessor.Bottom + SummaryConsts.VerticalSpace / 3;
            }
        }
    }
}