using OxDAOEngine.XML;
using OxLibrary.Geometry;
using System.Xml;

namespace OxDAOEngine.Data
{
    public abstract class DAO : IDAO
    {
        //TODO: add "event" directive
        public DAOEntityEventHandler? ChangeHandler { get; set; }

        //TODO: add "event" directive
        public ModifiedChangeHandler? ModifiedChangeHandler { get; set; }

        public DAO()
        {
            State = DAOState.Creating;
            try
            {
                Init();
                SetMembersHandlers();
                Clear();
            }
            finally
            {
                State = DAOState.Regular;
            }
        }

        public DAOState State { get; set; } = DAOState.Regular;

        public void StartLoading() => State = DAOState.Loading;
        public void FinishLoading() => State = DAOState.Regular;
        public void StartCoping() => State = DAOState.Coping;
        public void FinishCoping() => State = DAOState.Regular;

        public virtual int CompareTo(IDAO? other)
        {
            string? thisString = ToString();
            string? otherString = other?.ToString();

            return thisString is null 
                ? otherString is null 
                    ? 0 
                    : -1 
                : thisString.CompareTo(otherString);
        }

        public virtual string? MatchingString() => ToString();

        protected List<DAO> Members = new();

        protected bool AutoSaveMembers = true;
        protected bool AutoLoadMembers = true;

        private bool silentChange = false;
        public bool SilentChange
        {
            get => silentChange;
            set => silentChange = value;
        }

        private bool modified = false;

        public void StartSilentChange() =>
            SilentChange = true;

        public void FinishSilentChange()
        {
            SilentChange = false;
            FinishMembersSilentChange();
            ModifiedChangeHandler?.Invoke(this, new DAOModifyEventArgs(Modified, null));

            if (Modified)
                NotifyAll(DAOOperation.Modify);
        }

        private void FinishMembersSilentChange()
        {
            SetMembersHandlers(false);

            try
            {
                foreach (DAO member in Members)
                    member.FinishSilentChange();
            }
            finally
            {
                SetMembersHandlers(true);
            }
        }

        public bool Modified
        {
            get => modified;
            set => SetModified(value);
        }

        private void SetModified(bool value)
        {
            bool oldValue = modified;
            modified = value;
            NotifyAboutModify(oldValue);
        }

        protected virtual void NotifyAboutModify(bool oldValue)
        {
            if (!oldValue.Equals(modified))
                ModifiedChangeHandler?.Invoke(this, new DAOModifyEventArgs(modified, null));

            if (SilentChange)
                return;

            if (modified)
                NotifyAll(DAOOperation.Modify);
        }

        public void NotifyAll(DAOOperation operation)
        {
            ChangeHandler?.Invoke(this, new(operation));
            modified = true;
        }

        protected virtual void InitUniqueCopy() { }

        public TDAO GetCopy<TDAO>()
            where TDAO : IDAO, new()
        {
            TDAO newItem = new();
            newItem.CopyFrom(this);
            return newItem;
        }    

        public IDAO CopyFrom(IDAO? item, bool newUnique = false)
        {
            State = DAOState.Coping;

            item?.StartCoping();

            try
            {
                if (item is null)
                {
                    Clear();
                    return this;
                }

                try
                {
                    if (Equals(item)
                        && ((OwnerDAO is null && item.OwnerDAO is null)
                            || (OwnerDAO is not null && OwnerDAO.Equals(item.OwnerDAO))))
                        return this;

                    XmlDocument document = new();
                    document.AppendChild(document.CreateElement("CopyData"));

                    string oldXmlName = xmlName;
                    string oldOtherXmlName = item.XmlName;

                    xmlName = string.Empty;
                    item.XmlName = string.Empty;

                    try
                    {
                        if (document.DocumentElement is not null)
                        {
                            item.Save(document.DocumentElement, false);
                            Load(document.DocumentElement);

                            if (newUnique)
                                InitUniqueCopy();
                        }
                    }
                    finally
                    {
                        xmlName = oldXmlName;
                        item.XmlName = oldOtherXmlName;
                    }
                }
                finally
                {
                    CopyAdditionalInformationFrom(item);
                }

                SetMemberHandlers(item);
                Modified = true;
            }
            finally
            {
                State = DAOState.Regular;
                item?.FinishCoping();
            }

            return this;
        }

        protected virtual void CopyAdditionalInformationFrom(IDAO item) { }
        protected virtual void MemberModifiedHandler(DAO dao, DAOModifyEventArgs e) =>
            Modified |= e.Modified;

        protected virtual void SetMemberHandlers(IDAO member, bool set = true)
        {
            member.ModifiedChangeHandler -= MemberModifiedHandler;

            if (set)
                member.ModifiedChangeHandler += MemberModifiedHandler;
        }

        protected T? ModifyValue<T>(T? oldValue, T? newValue)
        {
            Modified |= CheckValueModified(oldValue, newValue);
            return newValue;
        }

        public static bool CheckValueModified(object? oldValue, object? newValue) =>
            oldValue is null
                ? newValue is not null
                : !oldValue.Equals(newValue);

        public virtual bool IsEmpty => false;

        protected virtual void SetMembersHandlers(bool set = true)
        {
            foreach (DAO member in Members)
                SetMemberHandlers(member, set);
        }

        protected void AddMember(DAO member)
        {
            Members.Add(member);
            member.OwnerDAO = this;
        }

        protected void RemoveMember(DAO member)
        {
            member.ModifiedChangeHandler -= MemberModifiedHandler;
            member.OwnerDAO = null;
            Members.Remove(member);
        }

        public IDAO TopOwnerDAO
        { 
            get 
            {
                IDAO topOwnerDAO = this;

                while (topOwnerDAO.OwnerDAO is not null)
                    topOwnerDAO = topOwnerDAO.OwnerDAO;

                return topOwnerDAO;
            }
        }

        public abstract void Init();

        public abstract void Clear();

        public virtual string DefaultXmlElementName
        {
            get
            {
                string typeName = GetType().Name;

                if (typeName.Contains('`'))
                    typeName = typeName[..typeName.IndexOf('`')];

                return typeName;
            }
        }

        private string xmlName = string.Empty;
        public string XmlName
        {
            get => xmlName;
            set => xmlName = value;
        }

        public string XmlElementName =>
            xmlName.Trim().Equals(string.Empty)
                ? DefaultXmlElementName
                : xmlName;

        protected abstract void SaveData(XmlElement element, bool clearModified = true);
        protected abstract void LoadData(XmlElement element);

        public bool WithoutXmlNode { get; set; } = false;

        public IDAO? OwnerDAO { get; set; }

        public virtual void Save(XmlElement? parentElement, bool clearModified = true)
        {
            if (parentElement is null)
                return;

            BeforeSave();
            XmlElement? element = WithoutXmlNode ? parentElement : XmlHelper.AppendElement(parentElement, XmlElementName, string.Empty);

            if (element is null)
                return;

            SaveData(element, clearModified);
            SaveMembersData(element, clearModified);

            if (clearModified)
                Modified = false;

            AfterSave();
        }

        protected virtual void AfterSave() { }

        protected virtual void BeforeSave() { }

        private void SaveMembersData(XmlElement element, bool clearModified = true)
        {
            if (!AutoSaveMembers)
                return;

            foreach (DAO member in Members)
                if (!member.IsEmpty)
                    member.Save(element, clearModified);
        }

        private void LoadMembersData(XmlElement element)
        {
            if (!AutoLoadMembers)
                return;

            foreach (DAO member in Members)
                member.Load(element);
        }

        public void Load(XmlElement? element)
        {
            if (State is not DAOState.Coping)
                State = DAOState.Loading;

            try
            {
                Clear();

                if (element is null)
                    return;

                if (!WithoutXmlNode 
                    && !element.Name.Equals(XmlElementName))
                    foreach (XmlNode node in element.ChildNodes)
                        if (node.Name.Equals(XmlElementName))
                        {
                            element = (XmlElement)node;
                            break;
                        }

                if (WithoutXmlNode 
                    || element.Name.Equals(XmlElementName))
                {
                    LoadData(element);
                    LoadMembersData(element);
                    AfterLoad();
                }

                Modified = false;
            }
            finally
            {
                if (State is DAOState.Loading)
                    State = DAOState.Regular;
            }
        }

        protected virtual void AfterLoad() { }

        public virtual string FullTitle() => XmlElementName;

        public int CompareTo(object? obj) =>
            obj is DAO dao ? CompareTo(dao) : -1;

        public virtual object ExtractKeyValue => this;

        public static int IntValue(object? value) =>
            value switch
            {
                null => 0,
                bool boolean =>
                    boolean ? 1 : 0,
                _ =>
                    int.TryParse(value.ToString(), out int intValue)
                        ? intValue
                        : 0,
            };

        public static short ShortValue(object? value) =>
            OxSH.Short(
                value switch
                {
                    null => 0,
                    bool boolean =>
                        boolean ? 1 : 0,
                    _ =>
                        short.TryParse(value.ToString(), out short shortValue)
                            ? shortValue
                            : 0
                }
            );

        public static bool BoolValue(object? value) =>
            bool.TryParse(value?.ToString(), out bool boolValue) && boolValue;

        public static T EnumValue<T>(object? value) =>
            value is null 
                ? default! 
                : (T)value;

        public static T? DAOValue<T>(object? value) where T : DAO =>
            value is null
                ? null 
                : (T)value;

        public static string StringValue(object? value) =>
            value is not null 
            && value.ToString() is not null
                ? value.ToString()!
                : string.Empty;

        public static Guid GuidValue(object? value) => 
            Guid.TryParse(value?.ToString(), out Guid guid) ? guid : Guid.Empty;

        public virtual string ShortString => ToString() ?? string.Empty;
    }
}
