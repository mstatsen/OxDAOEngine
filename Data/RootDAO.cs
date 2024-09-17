﻿using OxDAOEngine.Data.Fields;
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

    public delegate DAOImage? GetImageInfoHandler(Guid imageId);
    public delegate DAOImage UpdateImageHandler(Guid imageId, string name, Bitmap? image);

    public abstract class RootDAO<TField> : DAO, IFieldMapping<TField>
        where TField : notnull, Enum
    {
        public FieldModified<TField>? FieldModified;

        public RootDAO(): base() =>
            GenerateImageGuid();

        public FieldHelper<TField> FieldHelper = TypeHelper.FieldHelper<TField>();

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

        protected T? ModifyValue<T>(TField field, T? oldValue, T? newValue)
        {
            if (CheckValueModified(oldValue, newValue))
            {
                OnFieldModified(new FieldModifiedEventArgs<TField>(this, field, oldValue));
                Modified = true;
            }
            
            return newValue;
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

        public override bool Equals(object? obj) =>
            obj is RootDAO<TField> otherDAO
            && (base.Equals(obj)
                    && ImageId.Equals(otherDAO.ImageId)
                    && Name.Equals(otherDAO.Name)
            );
                    

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
            if (item is RootDAO<TField> rootItem)
            {
                UseImageList = rootItem.UseImageList;

                if (UseImageList)
                    daoImage = rootItem.DAOImage;
            }
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
                        FieldType.Memo => "COPY " + UniqueValue,
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
            set => imageId = GuidValue(ModifyValue(FieldHelper.ImageField, imageId, value));
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
                if (DAOImage != null && 
                    UniqueValue != null &&
                    DAOImage.UsageList.Find((d) =>
                        !UniqueValue.Equals(
                            ((RootDAO<TField>)d).GetFieldValue(UniqueField))
                        )
                    != null)
                    GenerateImageGuid();

                daoImage = OnUpdateImage?.Invoke(ImageId, Name, value);
                daoImage!.UsageList.Remove(this);
                daoImage!.UsageList.Add(this);
            }
        }

        private string name = string.Empty;

        public string Name
        {
            get => name;
            set => name = StringValue(ModifyValue(TitleField, name, value));
        }

        private Bitmap? GetImage()
        {
            if (UseImageList )
                if (daoImage == null && OnGetImageInfo != null)
                {
                    daoImage = OnGetImageInfo(imageId);
                    daoImage?.UsageList.Add(this);
                }

            return daoImage?.Image;
        }

        private void GenerateImageGuid()
        {
            if (UseImageList)
                ImageId = Guid.NewGuid();
        }

        public override void Init()
        {
            if (UseImageList)
                GenerateImageGuid();
        }

        private bool useImageList = false;
        public bool UseImageList 
        { 
            get => useImageList; 
            set => useImageList = value; 
        }
        public GetImageInfoHandler? OnGetImageInfo;
        public UpdateImageHandler? OnUpdateImage;

        public override int GetHashCode()
        {
            return name.GetHashCode() + imageId.GetHashCode();
        }
    }
}