using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data
{
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

    public abstract class RootDAO<TField> : DAO, IFieldMapping<TField>
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
                    if (item.Value == dao)
                        OnFieldModified(new FieldModifiedEventArgs<TField>(this, item.Key, e.OldValue));

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

        protected virtual object? GetFieldValue(TField field) 
        {
            if (field.Equals(FieldHelper.TitleField))
                return Name;

            if (field.Equals(FieldHelper.ImageField))
                return Image;

            return null;
        }

        public override bool Equals(object? obj)
        {
            if (base.Equals(obj)) 
                return true;

            if (obj is RootDAO<TField> otherDAO)
                return
                    ImageId.Equals(otherDAO.ImageId)
                    && (daoImage != null
                            ? daoImage.Equals(otherDAO.DAOImage)
                            : otherDAO.DAOImage == null
                        )
                    && Name.Equals(otherDAO.Name);
            else return false;
        }

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

            if (UseImageList)
                XmlHelper.AppendElement(element, XmlConsts.ImageId, imageId);
        }

        protected override void LoadData(XmlElement element)
        {
            name = XmlHelper.Value(element, XmlConsts.Name);
            imageId = XmlHelper.ValueGuid(element, XmlConsts.ImageId);
        }

        protected override void CopyAdditionalInformationFrom(DAO item)
        {
            if (UseImageList && item is RootDAO<TField> rootItem)
                daoImage = rootItem.DAOImage;
        }

        public override string ToString() =>
            Name;

        public virtual int CompareField(TField field, IFieldMapping<TField> y)
        {
            if (field.Equals(FieldHelper.TitleField) ||
                field.Equals(FieldHelper.ImageField))
                return StringValue(this[field]).CompareTo(StringValue(y[field]));

            string? thisString = (this[field] == null) ? string.Empty : this[field]?.ToString();
            return thisString != null ? thisString.CompareTo(y[field]?.ToString()) : y[field] == null ? 0 : -1;
        }

        public virtual object ParseCaldedValue(TField field, string value) =>
            value;
        public FilterOperation DefaultFilterOperation(TField field) =>
            TypeHelper.FieldHelper<TField>().DefaultFilterOperation(field);

        public virtual bool IsCalcedField(TField field) => false;

        protected sealed override void InitUniqueCopy()
        {
            if (UniqueField != null)
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
            set => ModifyValue(FieldHelper.ImageField, imageId, value, n => imageId = GuidValue(n));
        }

        private DAOImage? daoImage;
        public DAOImage? DAOImage 
        {
            get => daoImage;
            set
            {
                daoImage = value;
                ImageId = daoImage != null ? daoImage.Id : Guid.Empty;
            } 
        }

        public Bitmap? Image
        {
            get => GetImage();
            set => UpdateImage(value);
        }

        private void UpdateImage(Bitmap? value)
        {
            if (Image == null && value == null)
                return;

            if (Image != null && Image.Equals(value))
                return;

            if (value != null && value.Equals(Image))
                return;

            if (UseImageList)
            {
                ImageId = Guid.NewGuid();
                daoImage = DataManager.FieldController<TField>().UpdateImage(ImageId, Name, value);
                daoImage!.UsageList.Remove(this);
                daoImage!.UsageList.Add(this);
            }
        }

        private string name = string.Empty;

        public string Name
        {
            get => name;
            set => ModifyValue(TitleField, name, value, n => name = StringValue(n));
        }

        private Bitmap? GetImage()
        {
            if (UseImageList )
                if (daoImage == null)
                {
                    daoImage = GetImageInfo();
                    daoImage?.UsageList.Add(this);
                }

            return daoImage?.Image;
        }

        private DAOImage? GetImageInfo() => DataManager.FieldController<TField>().GetImageInfo(imageId);

        public override void Init() { }

        public override int GetHashCode() => 
            name.GetHashCode() ^ imageId.GetHashCode();

        private readonly bool useImageList = DataManager.UseImageList<TField>();
        public virtual bool UseImageList => useImageList;
    }
}