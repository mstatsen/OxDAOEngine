using System.Xml;

namespace OxDAOEngine.Data
{
    public interface IDAO : IComparable
    {
        DAOEntityEventHandler? ChangeHandler { get; set; }

        ModifiedChangeHandler? ModifiedChangeHandler { get; set; }

        DAOState State { get; set; }

        void StartLoading();
        void FinishLoading();
        void StartCoping();
        void FinishCoping();

        int CompareTo(IDAO? other);

        string? MatchingString();

        bool SilentChange { get; set; }

        void StartSilentChange();

        void FinishSilentChange();

        bool Modified { get; set; }

        void NotifyAll(DAOOperation operation);

        TDAO GetCopy<TDAO>()
            where TDAO : IDAO, new();

        IDAO CopyFrom(IDAO? item, bool newUnique = false);

        bool IsEmpty { get; }

        IDAO TopOwnerDAO { get; }

        void Init();

        void Clear();

        string DefaultXmlElementName { get; }

        string XmlName { get; set; }

        string XmlElementName { get; }

        bool WithoutXmlNode { get; set; }

        IDAO? OwnerDAO { get; set; }

        void Save(XmlElement? parentElement, bool clearModified = true);

        void Load(XmlElement? element);


        string FullTitle();


        object ExtractKeyValue { get; }

        string ShortString { get; }
    }
}
