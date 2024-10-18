using OxLibrary;
using OxDAOEngine.XML;
using System.Xml;
using OxLibrary.Data;

namespace OxDAOEngine.Data
{
    public class DAOImage : DAO
    {
        public DAOImage() : base() { }

        private Guid id = Guid.Empty;
        private string imageBase64 = string.Empty;
        private Bitmap? image = null;
        public bool FixUsage { get; set; }

        public Guid Id
        {
            get => id;
            set => id = GuidValue(ModifyValue(id, value));
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
            id = Guid.NewGuid();
            Image = null;
            imageBase64 = string.Empty;
        }

        public override void Init() { }

        protected override void LoadData(XmlElement element)
        {
            id = XmlHelper.ValueGuid(element, XmlConsts.Id, true);
            imageBase64 = XmlHelper.Value(element, XmlConsts.Image);
            image = OxBase64.Base64ToBitmap(imageBase64);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.Id, id);

            if (imageBase64 != string.Empty)
                XmlHelper.AppendElement(element, XmlConsts.Image, imageBase64);
        }

        public override string ToString() => 
            Id.ToString();

        public override bool Equals(object? obj) =>
            obj is DAOImage otherImage
            && (base.Equals(obj)
                || Id.Equals(otherImage.Id));

        public override int GetHashCode() =>
            id.GetHashCode();

        public readonly UniqueList<DAO> UsageList = new();
    }
}