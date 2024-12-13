using OxLibrary;
using OxLibrary.Geometry;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Fields;

namespace OxDAOEngine.Summary;

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
        Padding.Horizontal = 24;
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
        short maxBottom = 0;

        foreach (IControlAccessor accessor in ValueAccessors.Values)
            maxBottom = OxSh.Max(accessor.Bottom, maxBottom);

        Header.Size = new(SummaryConsts.CardWidth, SummaryConsts.CardHeaderHeight);
        Size = new(
            SummaryConsts.CardWidth,
            OxSh.Max(
                SummaryConsts.CardHeight,
                OxSh.Half(maxBottom + SummaryConsts.CardHeaderHeight)
            )
        );
    }

    public void FillAccessors()
    {
        ClearAccessors();

        if (IsGeneralSummaryPanel)
        {
            short nextTop = CreateAccessors(
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

    private short CreateAccessors(Dictionary<object, int> extraction, TField field, 
        short newTop = 12, int indent = 1)
    {
        OxSize nextLocation = new(
            SummaryConsts.HorizontalSpace * indent,
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
            nextLocation.Height = OxSh.Third(OxSh.Add(accessor.Bottom, SummaryConsts.VerticalSpace));
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