﻿using System.Xml;

namespace OxXMLEngine.Data
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
            foreach (DAOImage item in FindAll((i) => i.UsageList.Count == 0))
                Remove(item);
        }

        private void RemoveEmpty()
        {
            foreach (DAOImage item in FindAll((i) => i.Image == null || i.Id == Guid.Empty))
            {
                foreach (TDAO dao in item.UsageList.Cast<TDAO>())
                    dao[ListController.FieldHelper.ImageField] = Guid.Empty;

                Remove(item);
            }
        }

        private static readonly IListController<TField, TDAO> ListController = DataManager.ListController<TField, TDAO>();

        public void MergeDuplicates()
        {
            Dictionary<DAOImage, List<Guid>> UsingImagesMap = new();

            foreach (DAOImage item in this)
            {
                if (item.ImageBase64 == string.Empty)
                    continue;

                DAOImage mergeToItem = item;

                foreach (DAOImage existItem in UsingImagesMap.Keys)
                    if (!existItem.Id.Equals(item.Id) &&
                        existItem.ImageBase64.Equals(item.ImageBase64))
                    {
                        mergeToItem = existItem;
                        break;
                    }

                if (mergeToItem == item)
                {
                    UsingImagesMap[item] = new();
                    continue;
                }
                
                mergeToItem.UsageList.AddRange(item.UsageList);
                UsingImagesMap[mergeToItem].Add(item.Id);

                foreach (TDAO dao in item.UsageList.Cast<TDAO>())
                {
                    DAOImage? oldDaoImage = dao.DAOImage;
                    if (oldDaoImage != null && oldDaoImage != item)
                        oldDaoImage.UsageList.Clear();

                    dao.DAOImage = mergeToItem;
                }

                item.UsageList.Clear();
            }
        }
    }
}
