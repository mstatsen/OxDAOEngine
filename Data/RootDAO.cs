using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxDAOEngine.XML;
using OxLibrary;
using System.Xml;

namespace OxDAOEngine.Data;

public class FieldModifiedEventArgs<TField> : EventArgs
    where TField : notnull, Enum
{ 
    public RootDAO<TField> DAO { get; }
    public TField Field { get; }
    public object? OldValue { get; }

    public FieldModifiedEventArgs(RootDAO<TField> dao, TField field, object? oldValue)
    {
        DAO = dao;
        Field = field;
        OldValue = oldValue;
    }
}

public delegate void FieldModified<TField>(FieldModifiedEventArgs<TField> e)
    where TField : notnull, Enum;

public abstract class RootDAO<TField> : DAO, IDAO, IFieldMapping<TField>
    where TField : notnull, Enum
{
    public FieldModified<TField>? FieldModified;

    public RootDAO() : base() { }

    public object? this[TField field]
    {
        get => GetFieldValue(field);
        set => SetFieldValue(field, value);
    }

    protected Dictionary<TField, DAO> FieldMembers = new();

    protected void AddMember(TField field, DAO member)
    {
        AddMember(member);
        FieldMembers.Add(field, member);
    }

    protected void AddListMember<T>(TField field, ListDAO<T> listDao)
        where T : DAO, new()
    {
        AddMember(field, listDao);
        listDao.ItemRemoveHandler += (d, e) => ModifiedChangeHandler?.Invoke(d, new DAOModifyEventArgs(true, d));
        listDao.ItemAddHandler += (d, e) => ModifiedChangeHandler?.Invoke(d, new DAOModifyEventArgs(true, d));
    }

    protected FieldHelper<TField> FieldHelper = DataManager.FieldHelper<TField>();

    public delegate void SetModifiedFieldValue<T>(T? newValue);
    protected void ModifyValue<T>(TField field, T? oldValue, T? newValue,
        SetModifiedFieldValue<T> OnSetModifiedFieldValue, object? oldValueForHistory = null)
    {
        if (!CheckValueModified(oldValue, newValue))
            return;

        OnFieldModified(
            new FieldModifiedEventArgs<TField>(
                this,
                field,
                oldValueForHistory ?? oldValue
            )
        );

        OnSetModifiedFieldValue?.Invoke(newValue);
        Modified = true;
    }

    protected override void MemberModifiedHandler(DAO dao, DAOModifyEventArgs e)
    {
        if (e.Modified)
            foreach (KeyValuePair<TField, DAO> item in FieldMembers)
                if (item.Value.Equals(dao))
                    OnFieldModified(
                        new FieldModifiedEventArgs<TField>(this, item.Key, e.OldValue)
                    );

        base.MemberModifiedHandler(dao, e);
    }

    protected virtual void OnFieldModified(FieldModifiedEventArgs<TField> e) => 
        FieldModified?.Invoke(e);

    protected virtual void SetFieldValue(TField field, object? value)
    {
        if (field.Equals(FieldHelper.TitleField))
            Name = StringValue(value);
        else
        if (field.Equals(FieldHelper.ImageField))
        {
            switch (value)
            {
                case Guid:
                    imageId = GuidValue(value);
                    break;
                case null:
                case Bitmap:
                    Image = (Bitmap?)value;
                    break;
            }
        }
    }
    protected virtual object? GetFieldValue(TField field) => 
        field.Equals(FieldHelper.TitleField)
            ? Name
            : field.Equals(FieldHelper.ImageField)
                ? Image
                : (object?)null;

    public override bool Equals(object? obj) => 
        base.Equals(obj)
        || (obj is RootDAO<TField> otherDAO
            && ImageId.Equals(otherDAO.ImageId)
            && Name.Equals(otherDAO.Name));

    public override void Clear()
    {
        if (State is 
            DAOState.Creating or 
            DAOState.Loading)
            return;

        Name = string.Empty;
        ImageId = Guid.Empty;
        Image = null;
    }

    protected override void SaveData(XmlElement element, bool clearModified = true)
    {
        XmlHelper.AppendElement(element, XmlConsts.Name, name);

        if (UseImageList && 
            !imageId.Equals(Guid.Empty))
            XmlHelper.AppendElement(element, XmlConsts.ImageId, imageId);
    }

    protected override void LoadData(XmlElement element)
    {
        name = XmlHelper.Value(element, XmlConsts.Name);
        imageId = XmlHelper.ValueGuid(element, XmlConsts.ImageId);
        AddImageUsage();
    }

    public bool IsListControllerDAO =>
        DataManager.FieldController<TField>().FullItemsList.Equals(TopOwnerDAO);

    protected override void CopyAdditionalInformationFrom(IDAO item) =>
        AddImageUsage();

    public override string ToString() =>
        Name;

    public virtual int CompareField(TField field, IFieldMapping<TField> y)
    {
        if (field.Equals(FieldHelper.TitleField) 
            || field.Equals(FieldHelper.ImageField))
            return StringValue(this[field]).CompareTo(StringValue(y[field]));

        string? thisString = (this[field] is null) 
            ? string.Empty 
            : this[field]?.ToString();

        return thisString is not null 
            ? thisString.CompareTo(y[field]?.ToString()) 
            : y[field] is null 
                ? 0 
                : -1;
    }

    public virtual object ParseCaldedValue(TField field, string value) =>
        value;

    public FilterOperation DefaultFilterOperation(TField field) =>
        TypeHelper.FieldHelper<TField>().DefaultFilterOperation(field);

    public virtual bool IsCalcedField(TField field) => false;

    protected sealed override void InitUniqueCopy()
    {
        if (UniqueField is not null)
        {
            UniqueValue =
                FieldHelper.GetFieldType(UniqueField) switch
                {
                    FieldType.Guid => Guid.NewGuid(),
                    FieldType.Label or FieldType.String => UniqueValue + " (Copy)",
                    FieldType.Memo or FieldType.ShortMemo => "COPY " + UniqueValue,
                    _ =>
                        UniqueValue
                };
        }

        this[TitleField] += " (Copy)";
    }

    private TField UniqueField => FieldHelper.UniqueField;

    private object? UniqueValue
    { 
        get => this[UniqueField];
        set => this[UniqueField] = value;
    }
    private TField TitleField => FieldHelper.TitleField;


    private Guid imageId = Guid.Empty;
    public Guid ImageId
    {
        get => imageId;
        set
        {
            if (!UseImageList)
                return;

            DAOImage?.UsageList.Remove(this);
            ModifyValue(FieldHelper.ImageField, imageId, value, n => imageId = GuidValue(n));
            AddImageUsage();
        }
    }

    public DAOImage? DAOImage 
    {
        get => DataManager.FieldController<TField>().GetImageInfo(imageId);
    }

    public Bitmap? Image
    {
        get => GetImage();
        set => UpdateImage(value);
    }

    private void UpdateImage(Bitmap? value)
    {
        if (!UseImageList
            || (Image is null && value is null)
            || (Image is not null && Image.Equals(value))
            || (value is not null && value.Equals(Image))
        )
            return;

        DAOImage? daoImage = DataManager.FieldController<TField>().SuitableImage(value);
        ImageId = daoImage is not null 
            ? daoImage.Id 
            : Guid.NewGuid();

        if (daoImage is null)
            DataManager.FieldController<TField>().UpdateImage(ImageId, value);

        AddImageUsage();
    }

    private string name = string.Empty;

    public string Name
    {
        get => name;
        set => ModifyValue(TitleField, name, value, n => name = StringValue(n));
    }

    private Bitmap? GetImage()
    {
        AddImageUsage();
        return DAOImage?.Image;
    }

    protected virtual bool AlwaysSaveImage => false;

    private void AddImageUsage()
    {
        if (!UseImageList
            || DAOImage is null
            )
            return;

        DAOImage.FixUsage = AlwaysSaveImage;
        DAOImage.UsageList.Add(this);
    }

    public override void Init() { }

    public override int GetHashCode() => 
        name.GetHashCode() ^ imageId.GetHashCode();

    private bool IsFiltrationDao = false;
    internal RootDAO<TField> MarkAsFiltration()
    {
        IsFiltrationDao = true;
        return this;
    }
   
    public virtual bool UseImageList =>
        DataManager.UseImageList<TField>()
        && !IsFiltrationDao;

    public virtual Color BaseColor => OxStyles.DefaultGridRowColor;
    public Color BackColor => new OxColorHelper(BaseColor).Darker(7);
}