using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Fields;

namespace OxDAOEngine.Summary
{
    public delegate void SummaryPanelHandler(ISummaryPanel panel);

    public class SummaryPanel<TField, TDAO> : OxCard, ISummaryPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public readonly TField Field;
        public readonly AccessorDictionary<TField, TDAO> ValueAccessors = new();

        public SummaryPanel(TField field) : base()
        {
            Field = field;
            Text = IsGeneralSummaryPanel
                ? "General"
                : $"by {TypeHelper.Name(field)}";
            Paddings.Horizontal = 24;
        }

        private readonly FieldHelper<TField> FieldHelper = 
            DataManager.FieldHelper<TField>();

        public bool IsGeneralSummaryPanel =>
            FieldHelper.FieldMetaData.Equals(Field);

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
                Math.Max(SummaryConsts.CardHeight, maxBottom + SummaryConsts.CardHeaderHeight * 2)
            );
        }

        public void FillAccessors()
        {
            ClearAccessors();

            if (IsGeneralSummaryPanel)
            {
                int nextTop = CreateAccessors(
                    new Dictionary<object, int>()
                    {
                        [$"Total {DataManager.ListController<TField, TDAO>().ListName}"]
                        = DataManager.FullItemsList<TField, TDAO>().Count
                    },
                    Field
                );

                foreach (TField field in FieldHelper.GeneralSummaryFields)
                {
                    Dictionary<object, int> extract = new FieldExtractor<TField, TDAO>(
                        DataManager.FullItemsList<TField, TDAO>()).CountExtract(
                        field,
                        true,
                        ExtractCompareType.Count
                    );
                    FieldCountExtract trueExtract = new();

                    if (extract.ContainsKey(true))
                        trueExtract.Add(true, extract[true]);

                    if (trueExtract.Count == 0 && extract.ContainsKey(1))
                        trueExtract.Add(true, extract[1]);

                    if (trueExtract.Count == 0)
                        trueExtract.Add(true, 0);

                    nextTop = CreateAccessors(trueExtract, field, nextTop, 4);
                }
                    
            }
            else
                CreateAccessors(
                    new FieldExtractor<TField, TDAO>(
                        DataManager.FullItemsList<TField, TDAO>()).CountExtract(
                        Field,
                        true,
                        ExtractCompareType.Count
                    ),
                    Field
                );
        }

        public override Color DefaultColor => EngineStyles.SummaryColor;

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Dock = DockStyle.Top;
        }

        private int CreateAccessors(Dictionary<object, int> extraction, TField field, int newTop = SummaryConsts.VerticalSpace, int indent = 1)
        {
            Point nextLocation = new(
                SummaryConsts.HorizontalSpace * indent, 
                newTop
            );

            foreach (var extractItem in extraction)
            {
                IControlAccessor accessor = ValueAccessors.CreateAccessor(
                    field,
                    extractItem.Key,
                    ContentContainer,
                    ExtractItemCaptionHandler(field, extractItem.Key),
                    extractItem.Value,
                    nextLocation
                );

                nextLocation.Y = accessor.Bottom + SummaryConsts.VerticalSpace / 3;
            }

            return nextLocation.Y;
        }

        private string ExtractItemCaptionHandler(TField field, object? value)
        {
            if (GetExtractItemCaption is not null)
                return GetExtractItemCaption(field, value);

            return 
                value is bool boolValue
                    ? (boolValue ? "Yes" : "No")
                    : TypeHelper.Name(value);
        }

        public GetExtractItemCaption<TField>? GetExtractItemCaption;
    }

    public delegate string GetExtractItemCaption<TField>(TField field, object? value)
        where TField : notnull, Enum;
}