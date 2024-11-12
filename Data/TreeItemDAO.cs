using System.Xml;

namespace OxDAOEngine.Data
{
    public class TreeItemDAO<TDAO> : DAO
        where TDAO : TreeItemDAO<TDAO>, new()
    {
        public TreeItemDAO() { }

        public TDAO? Parent 
        { 
            get => OwnerDAO is TDAO treeItemDAO 
                ? treeItemDAO 
                : null; 
            set => OwnerDAO = value; 
        }

        public readonly TreeDAO<TDAO> Childs = new()
        {
            XmlName = "Childs"
        };

        public override void Clear()
        {
            foreach (TreeItemDAO<TDAO> child in Childs)
                child.Parent = null;

            Childs.Clear();
        }

        public override void Init() => 
            AddMember(Childs);

        public TDAO AddChild(TDAO child)
        {
            child.Parent = (TDAO)this;
            return Childs.Add(child);
        }

        public void RemoveChild(TDAO child)
        {
            child.Parent = null;
            Childs.Remove(child);
        }

        public int Count => Childs.Count;

        protected override void SaveData(XmlElement element, bool clearModified = true) { }

        protected override void LoadData(XmlElement element) { }

        protected override void AfterLoad()
        {
            base.AfterLoad();

            foreach(TDAO child in Childs)
                child.Parent = (TDAO)this;
        }
    }
}