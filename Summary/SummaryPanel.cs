using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Fields;
using OxLibrary;

namespace OxDAOEngine.Summary
{
    public delegate void SummaryPanelHandler<TField, TDAO>(SummaryPanel<TField, TDAO> panel)
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new();

    public class SummaryPanel<TField, TDAO> : OxCard
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
            Padding.HorizontalInt = 24;
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
            OxWidth maxBottom = OxWh.W0;

            foreach (IControlAccessor accessor in ValueAccessors.Values)
                maxBottom = OxWh.Max(accessor.Bottom, maxBottom);

            Header.Size = new(SummaryConsts.CardWidth, SummaryConsts.CardHeaderHeight);
            Size = new(
                SummaryConsts.CardWidth,
                OxWh.Max(
                    SummaryConsts.CardHeight, 
                    maxBottom | OxWh.Mul(SummaryConsts.CardHeaderHeight, 2)
                )
            );
        }

        public void FillAccessors()
        {
            ClearAccessors();

            if (IsGeneralSummaryPanel)
            {
                OxWidth nextTop = CreateAccessors(
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

                    if (trueExtract.Count is 0 
                        && extract.ContainsKey(1))
                        trueExtract.Add(true, extract[1]);

                    if (trueExtract.Count is 0)
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
            Dock = OxDock.Top;
        }

        private OxWidth CreateAccessors(Dictionary<object, int> extraction, TField field, 
            OxWidth newTop = OxWidth.M | OxWidth.S, int indent = 1)
        {
            OxSize nextLocation = new(
                OxWh.Mul(SummaryConsts.HorizontalSpace, indent), 
                newTop
            );

            foreach (var extractItem in extraction)
            {
                IControlAccessor accessor = ValueAccessors.CreateAccessor(
                    field,
                    extractItem.Key,
                    this,
                    ExtractItemCaptionHandler(field, extractItem.Key),
                    extractItem.Value,
                    nextLocation
                );
                nextLocation.Height = OxWh.Sum(accessor.Bottom, OxWh.Div(SummaryConsts.VerticalSpace, 3));
            }

            return nextLocation.Height;
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