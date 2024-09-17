﻿using OxLibrary;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data
{
    public class DAOImage : DAO
    {
        public DAOImage() : base() { }

        private Guid id = Guid.Empty;
        private string imageBase64 = string.Empty;
        private Bitmap? image = null;
        private string name = string.Empty;

        public Guid Id
        {
            get => id;
            set => id = GuidValue(ModifyValue(id, value));
        }

        public string Name
        {
            get => name;
            set => name = StringValue(ModifyValue(name, value));
        }

        public Bitmap? Image
        {
            get => image;
            set
            {
                image = ModifyValue(image, value);
                imageBase64 = OxBase64.BitmapToBase64(image);
            }
        }

        public string ImageBase64 => imageBase64;

        public override void Clear()
        {
            Id = Guid.Empty;
            Name = string.Empty;
            Image = null;
            imageBase64 = string.Empty;
        }

        private void GenerateGuid() =>
            id = Guid.NewGuid();

        public override void Init() =>
            GenerateGuid();

        protected override void LoadData(XmlElement element)
        {
            id = XmlHelper.ValueGuid(element, XmlConsts.Id);
            if (id == Guid.Empty)
                GenerateGuid();

            name = XmlHelper.Value(element, XmlConsts.Name);
            imageBase64 = XmlHelper.Value(element, XmlConsts.Image);
            image = OxBase64.Base64ToBitmap(imageBase64);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.Id, id);
            XmlHelper.AppendElement(element, XmlConsts.Name, name);

            if (imageBase64 != string.Empty)
                XmlHelper.AppendElement(element, XmlConsts.Image, imageBase64);
        }

        public override string ToString() => 
            Name;

        public override bool Equals(object? obj) =>
            obj is DAOImage otherImage
            && (base.Equals(obj)
                || Id.Equals(otherImage.Id));

        public override int GetHashCode() =>
            id.GetHashCode();

        public readonly List<DAO> UsageList = new();
    }
}