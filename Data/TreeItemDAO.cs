using OxLibrary.Controls;
using System.Xml;

namespace OxDAOEngine.Data
{
    public interface ITreeItemDAO<TDAO>
        where TDAO : DAO, new()
    {
        TDAO? Parent { get; set; }

        TDAO AddChild(TDAO child);
    }

    public class TreeItemDAO<TDAO> : DAO, ITreeItemDAO<TDAO>
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
            if (Childs.Contains(child))
                return child;

            Childs.Add(child);
            child.Parent = (TDAO?)this;
            return child;
        }

        public TDAO InsertChild(int index, TDAO child)
        {
            if (Childs.Contains(child))
                return child;

            Childs.Insert(index, child);
            child.Parent = (TDAO?)this;
            return child;
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

            foreach (TDAO child in Childs)
                child.Parent = (TDAO)this;
        }

        protected TDAO AsDAO => (TDAO)this;

        public int Index
        {
            get => 
                Parent is not null
                    ? Parent.Childs.IndexOf(AsDAO)
                    : -1;
            set => Move(value);
        }

        private void Move(int newIndex)
        {
            if (Parent is null 
                || newIndex < 0 
                || newIndex >= Parent.Count)
                return;

            TreeItemDAO<TDAO> thisParent = Parent;
            thisParent.RemoveChild(AsDAO);
            thisParent.InsertChild(newIndex, AsDAO);
        }

        private void Move(OxUpDown direction)
        {
            if (Parent is null 
                || Index < 0)
                return;

            Move(Index + OxUpDownHelper.Delta(direction));
        }

        public void MoveUp() => Move(OxUpDown.Up);

        public void MoveDown() => Move(OxUpDown.Down);
    }
}