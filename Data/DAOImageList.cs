﻿using System.Xml;

namespace OxDAOEngine.Data
{
    public class DAOImageList<TField, TDAO> : ListDAO<DAOImage>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public DAOImage? ImageInfo(Guid imageId) =>
            Find((i) => i.Id.Equals(imageId));

        public Bitmap? Image(Guid imageId) =>
            ImageInfo(imageId)?.Image;

        public DAOImage UpdateImage(Guid id, Bitmap? image)
        {
            DAOImage? imageInfo = ImageInfo(id);

            if (imageInfo is null)
            {
                imageInfo = Add();
                imageInfo.Id = id;
            }
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

                if (!item.FixUsage 
                    && realUsage is 0)
                    unusedList.Add(item);
            }

            Remove(i => unusedList.Contains(i), false);
            Remove(i => i.UsageList.Count is 0, false);
        }

        private void RemoveEmpty()
        {
            foreach (DAOImage item in FindAll((i) => i.Image is null || i.Id.Equals(Guid.Empty)))
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
                if (item.ImageBase64.Equals(string.Empty))
                    continue;

                DAOImage mergeToItem = item;

                foreach (DAOImage existItem in UsingImagesMap)
                    if (!existItem.Id.Equals(item.Id) &&
                        existItem.ImageBase64.Equals(item.ImageBase64))
                    {
                        mergeToItem = existItem;
                        break;
                    }

                if (mergeToItem.Equals(item))
                {
                    UsingImagesMap.Add(item);
                    continue;
                }
                
                mergeToItem.UsageList.AddRange(item.UsageList);

                foreach (DAO dao in item.UsageList)
                {
                    if (dao.OwnerDAO is null 
                        || dao is not RootDAO<TField> rootDAO)
                        continue;

                    DAOImage? oldDaoImage = rootDAO.DAOImage;

                    if (oldDaoImage is not null 
                        && !oldDaoImage.Equals(item))
                        oldDaoImage.UsageList.Clear();

                    rootDAO.ImageId = mergeToItem.Id;
                }

                item.UsageList.Clear();
            }
        }
    }
}
