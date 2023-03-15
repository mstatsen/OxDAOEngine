using OxLibrary.Dialogs;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory;
using OxXMLEngine.Data.Decorator;
using OxXMLEngine.Data.Extract;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Data.Types;
using OxXMLEngine.Editor;
using OxXMLEngine.Export;
using OxXMLEngine.Settings;
using OxXMLEngine.Statistic;
using OxXMLEngine.View;
using System.Xml;

namespace OxXMLEngine.Data
{
    public static class DataManager
    {
        private static readonly Dictionary<Type, IDataController> FieldControllers = new();
        private static readonly List<IDataController> Controllers = new();
        private static readonly Dictionary<string, XmlDocument> Files = new();
        private static bool initialized = false;

        public static IDataController Register<TField>(IDataController controller)
            where TField : notnull, Enum
        {
            FieldControllers.Add(typeof(TField), controller);
            return Register(controller);
        }

        public static IDataController Register(IDataController controller)
        {
            if (Controllers.Contains(controller))
                return controller;

            Controllers.Add(controller);

            if ((controller.FileName != string.Empty) &&
                !Files.ContainsKey(controller.FileName))
                Files.Add(controller.FileName, new XmlDocument());

            return controller;
        }

        public static void Save()
        {
            foreach (var file in Files)
            {
                file.Value.RemoveAll();
                file.Value.AppendChild(file.Value.CreateElement("Data"));

                foreach (IDataController controller in Controllers)
                    if (controller.FileName == file.Key && 
                        file.Value.DocumentElement != null)
                        controller.Save(file.Value.DocumentElement);

                file.Value.Save(file.Key);
            }
        }

        public static void Load()
        {
            foreach (var file in Files)
            {
                file.Value.RemoveAll();

                if (File.Exists(file.Key))
                    file.Value.Load(file.Key);
                else
                    file.Value.AppendChild(file.Value.CreateElement("Data"));

                foreach (IDataController controller in Controllers)
                    if (controller.FileName == file.Key 
                        && file.Value.DocumentElement != null)
                        controller.Load(file.Value.DocumentElement);
            }
        }

        public static void SaveSystemData()
        {
            foreach (IDataController controller in Controllers)
                if (controller.IsSystem && controller.Modified
                    && Files[controller.FileName].DocumentElement != null)
                {
                    controller.Save(Files[controller.FileName].DocumentElement);
                    Files[controller.FileName].Save(controller.FileName);
                }
        }

        public static TDataController Controller<TDataController>()
            where TDataController : IDataController
        {
            foreach (IDataController controller in Controllers)
                if (controller is TDataController dataController)
                    return dataController;

            throw new KeyNotFoundException($"Controller {typeof(TDataController).Name} not found");
        }

        public static IDataController Controller(IOxPane face)
        {
           foreach (IDataController controller in Controllers)
                if (controller.Face == face)
                    return controller;

            throw new KeyNotFoundException($"Face Controller for {face.Text} not found");
        }

        public static IFieldController<TField> FieldController<TField>()
            where TField : notnull, Enum
        {
            try
            {
                return Controller<IFieldController<TField>>();
            }
            catch(KeyNotFoundException)
            {
                throw new KeyNotFoundException($"FieldController for {typeof(TField).Name} not found");
            }
        }

        public static ControlFactory<TField, TDAO> ControlFactory<TField, TDAO>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                return ListController<TField, TDAO>().ControlFactory;
            }
            catch 
            {
                throw new KeyNotFoundException($"ControlFactory for {typeof(TDAO).Name}[{typeof(TField).Name}] not found");
            }
        }

        public static DecoratorFactory<TField, TDAO> DecoratorFactory<TField, TDAO>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                return ListController<TField, TDAO>().DecoratorFactory;
            }
            catch
            {
                throw new KeyNotFoundException($"DecoratorFactory for {typeof(TDAO).Name}[{typeof(TField).Name}] not found");
            }
        }

        public static ControlBuilder<TField, TDAO> Builder<TField, TDAO>(ControlScope scope, bool forceNew = false, object? variant = null)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                return ControlFactory<TField, TDAO>().Builder(scope, forceNew, variant);
            }
            catch
            {
                throw new KeyNotFoundException($"Builder for {typeof(TDAO).Name}[{typeof(TField).Name}]({scope}) not found");
            }
        }

        public static IListController<TField, TDAO> ListController<TField, TDAO>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                return Controller<IListController<TField, TDAO>>();
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"ListController for {typeof(TDAO).Name}[{typeof(TField).Name}] not found");
            }
        }

        public static IListController<TField, TDAO, TFieldGroup> ListController<TField, TDAO, TFieldGroup>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
            where TFieldGroup : notnull, Enum
        {
            try
            {
                return Controller<IListController<TField, TDAO, TFieldGroup>>();
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"ListController for {typeof(TDAO).Name}[{typeof(TField).Name}] not found");
            }
        }

        public static DAOWorker<TField, TDAO, TFieldGroup> Worker<TField, TDAO, TFieldGroup>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
            where TFieldGroup : notnull, Enum
        {
            try
            {
                return ListController<TField, TDAO, TFieldGroup>().Worker;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"Worker for {typeof(TDAO).Name}[{typeof(TField).Name}] not found");
            }
        }

        public static DAOEditor<TField, TDAO, TFieldGroup> Editor<TField, TDAO, TFieldGroup>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
            where TFieldGroup : notnull, Enum
        {
            try
            {
                return ListController<TField, TDAO, TFieldGroup>().Editor;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"Editor for {typeof(TDAO).Name}[{typeof(TField).Name}] not found");
            }
        }

        public static IFieldGroupController<TField, TFieldGroup> FieldController<TField, TFieldGroup>()
            where TField : notnull, Enum
            where TFieldGroup : notnull, Enum
        {
            try
            {
                return Controller<IFieldGroupController<TField, TFieldGroup>>();
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"FieldGroupController for {typeof(TField).Name}, {typeof(TFieldGroup).Name} not found");
            }
        }

        public static bool Modified
        {
            get
            {
                foreach (IDataController controller in Controllers)
                    if (controller.Modified && !controller.IsSystem)
                        return true;

                return false;

            }
        }

        public static void SetModifiedHandler(ModifiedChangeHandler handler)
        {
            foreach (IDataController controller in Controllers)
                controller.ModifiedHandler += handler;
        }

        public static List<OxPane> Faces
        {
            get 
            {
                List<OxPane> faces = new();

                foreach (IDataController controller in Controllers)
                    if (controller.Face != null)
                        faces.Add(controller.Face);

                return faces;
            }
        }

        public static void AddItem<TField, TDAO>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                ListController<TField, TDAO>().AddItem();
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot create {typeof(TDAO).Name} because it controller not found");
            }
        }

        public static bool SelectItem<TField, TDAO>(out TDAO? selectedItem, OxPane parentPane, 
            TDAO? initialItem = null, IMatcher<TField>? filter = null)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                return ListController<TField, TDAO>().SelectItem(out selectedItem, parentPane, initialItem, filter);
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot select {typeof(TDAO).Name} because it controller not found");
            }
        }

        public static void EditItem<TField, TDAO>(TDAO? item)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                ListController<TField, TDAO>().EditItem(item);
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot edit {item} because it controller not found");
            }
        }

        public static void CopyItem<TField, TDAO>(TDAO? item)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                ListController<TField, TDAO>().CopyItem(item);
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot copy {item} because it controller not found");
            }
        }

        public static void ViewItem<TField, TDAO>(TDAO? item, ItemViewMode viewMode = ItemViewMode.Simple)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                ListController<TField, TDAO>().ViewItem(item, viewMode);
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot view {item} because it controller not found");
            }
        }

        public static void ViewItem<TField, TDAO>(TField field, object? value, ItemViewMode viewMode = ItemViewMode.Simple)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                ListController<TField, TDAO>().ViewItem(field, value, viewMode);
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot view item because it controller not found");
            }
        }

        public static void ViewItems<TField, TDAO>(TField field, object? value)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                ListController<TField, TDAO>().ViewItems(field, value);
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot view items because it controller not found");
            }
        }

        public static RootListDAO<TField, TDAO> FullItemsList<TField, TDAO>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new ()
        {
            try
            {
                return ListController<TField, TDAO>().FullItemsList;
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot get full items list of {typeof(TDAO).Name} because it controller not found");
            }
        }

        public static RootListDAO<TField, TDAO> VisibleItemsList<TField, TDAO>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                return ListController<TField, TDAO>().VisibleItemsList;
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot get visible items list of {typeof(TDAO).Name} because it controller not found");
            }
        }

        public static FieldSortings<TField, TDAO>? DefaultSorting<TField, TDAO>()
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                return ListController<TField, TDAO>().DefaultSorting();
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot get default sorting for dao of {typeof(TField).Name} because it controller not found");
            }
        }

        public static FieldGroupHelper<TField, TFieldGroup> FieldGroupHelper<TField, TFieldGroup>()
            where TField : notnull, Enum
            where TFieldGroup : notnull, Enum
        {
            try
            {
                return FieldController<TField, TFieldGroup>().FieldGroupHelper;
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot get helper for {typeof(TFieldGroup).Name} because it controller not found");
            }
        }

        public static TDAO? Item<TField, TDAO>(TField field, object value)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new()
        {
            try
            {
                return ListController<TField, TDAO>().Item(field, value);
            }
            catch
            {
                throw new KeyNotFoundException($"Cannot get item of {typeof(TDAO).Name} because it controller not found");
            }
        }


        public static void Init()
        {
            if (initialized)
                return;

            TypeHelper.Register<FiltrationTypeHelper>();
            TypeHelper.Register<FilterConcatHelper>();
            TypeHelper.Register<FieldsFillingHelper>();
            TypeHelper.Register<FieldVariantHelper>();
            TypeHelper.Register<FilterOperationHelper>();
            TypeHelper.Register<SortOrderHelper>();
            TypeHelper.Register<SortingVariantHelper>();
            TypeHelper.Register<ControlCaptionVariantHelper>();
            TypeHelper.Register<ControlScopeHelper>();
            TypeHelper.Register<ExtractCompareTypeHelper>();
            TypeHelper.Register<TextFilterOperationHelper>();
            TypeHelper.Register<ExportFormatHelper>();
            TypeHelper.Register<ExportSummaryTypeHelper>();
            TypeHelper.Register<DecoratorTypeHelper>();
            TypeHelper.Register<IconSizeHelper>();
            TypeHelper.Register<IconClickVariantHelper>();
            TypeHelper.Register<IconContentHelper>();
            TypeHelper.Register<ItemsViewsTypeHelper>();
            TypeHelper.Register<StatisticTypeHelper>();
            TypeHelper.Register<SettingsPartHelper>();
            TypeHelper.Register<GeneralSettingHelper>();
            TypeHelper.Register<DAOSettingHelper>();
            SettingsManager.Init();
            initialized = true;
        }
    }
}