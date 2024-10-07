using System.Xml;

namespace OxDAOEngine.Data
{
    public class DAOImageList<TField, TDAO> : ListDAO<DAOImage>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public DAOImage? ImageInfo(Guid imageId) =>
            Find((i) => i.Id == imageId);

        public Bitmap? Image(Guid imageId) =>
            ImageInfo(imageId)?.Image;

        public DAOImage UpdateImage(Guid id, string name, Bitmap? image)
        {
            DAOImage? imageInfo = ImageInfo(id);

            if (imageInfo == null)
            {
                imageInfo = Add();
                imageInfo.Id = id;
            }
            imageInfo.Name = name;
            imageInfo.Image = image;
            return imageInfo;
        }

        public override void Save(XmlElement? parentElement, bool clearModified = true)
        {
            RemoveEmpty();
            MergeDuplicates();
            RemoveUnused();
            base.Save(parentElement, clearModified);
        }

        private void RemoveUnused()
        {
            IListController<TField, TDAO> listController = DataManager.ListController<TField, TDAO>();
            RootListDAO<TField, TDAO> fullItemsList = listController.FullItemsList;
            List<DAOImage> unusedList = new();

            foreach (DAOImage item in FindAll((i) => i.UsageList.Count > 0))
            {
                int realUsage = 0;

                if (!item.FixUsage)
                    foreach (DAO usageDao in item.UsageList)
                        if (fullItemsList.Contains(usageDao)
                            && usageDao is RootDAO<TField> rootDao)
                            realUsage++;

                if (!item.FixUsage && realUsage == 0)
                    unusedList.Add(item);
            }

            foreach (DAOImage item in unusedList)
                Remove(item, false);


            foreach (DAOImage item in FindAll((i) => i.UsageList.Count == 0))
                Remove(item, false);
        }

        private void RemoveEmpty()
        {
            foreach (DAOImage item in FindAll((i) => i.Image == null || i.Id == Guid.Empty))
            {
                foreach (RootDAO<TField> dao in item.UsageList.Cast<RootDAO<TField>>())
                    dao[ListController.FieldHelper.ImageField] = Guid.Empty;

                Remove(item, false);
            }
        }

        private static readonly IListController<TField, TDAO> ListController = 
            DataManager.ListController<TField, TDAO>();

        public void MergeDuplicates()
        {
            List<DAOImage> UsingImagesMap = new();

            foreach (DAOImage item in this)
            {
                if (item.ImageBase64 == string.Empty)
                    continue;

                DAOImage mergeToItem = item;

                foreach (DAOImage existItem in UsingImagesMap)
                    if (!existItem.Id.Equals(item.Id) &&
                        existItem.ImageBase64.Equals(item.ImageBase64))
                    {
                        mergeToItem = existItem;
                        break;
                    }

                if (mergeToItem == item)
                {
                    UsingImagesMap.Add(item);
                    continue;
                }
                
                mergeToItem.UsageList.AddRange(item.UsageList);

                foreach (DAO dao in item.UsageList)
                {
                    if (dao is not RootDAO<TField> rootDAO)
                        continue;

                    DAOImage? oldDaoImage = rootDAO.DAOImage;
                    if (oldDaoImage != null && oldDaoImage != item)
                        oldDaoImage.UsageList.Clear();

                    rootDAO.DAOImage = mergeToItem;
                }

                item.UsageList.Clear();
            }
        }
    }
}
