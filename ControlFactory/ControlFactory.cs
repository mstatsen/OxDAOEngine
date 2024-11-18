using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.View;
using OxDAOEngine.Grid;
using OxDAOEngine.ControlFactory.Filter;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Links;
using OxDAOEngine.View.Types;
using OxDAOEngine.ControlFactory.Controls.Fields;
using OxDAOEngine.ControlFactory.Controls.Links;
using OxDAOEngine.ControlFactory.Controls.Sorting;
using OxDAOEngine.Data.Filter.Types;

namespace OxDAOEngine.ControlFactory
{
    public class BuilderKey
    {
        public readonly ControlScope Scope;
        public readonly object? Variant;

        public BuilderKey(ControlScope scope, object? variant)
        {
            Scope = scope;
            Variant = variant;
        }
    }

    public abstract class ControlFactory<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected virtual IInitializer CreateExtractInitializer(TField field,
            bool fullExtract = true, bool fixedExtract = true) =>
            new ExtractInitializer<TField, TDAO>(
                field,
                fullExtract: true,
                fixedExtract: true
            );

        protected virtual IInitializer? Initializer(IBuilderContext<TField, TDAO> context)
        {
            switch (context.FieldType)
            {
                case FieldType.Memo:
                case FieldType.ShortMemo:
                    return new TextMultiLineInitializer(
                        context.AdditionalContext is bool boolContext
                        && boolContext,
                        context.FieldType == FieldType.Memo);
                case FieldType.Enum:
                case FieldType.Boolean:
                case FieldType.Integer:
                    if (context is FieldContext<TField, TDAO> accessorContext && context.IsQuickFilter)
                    {
                        object? variant = BuilderVariant(context.Builder);

                        return new ExtractInitializer<TField, TDAO>(
                            accessorContext.Field,
                            addAnyObject: true,
                            fullExtract:
                                variant != null
                                && variant.Equals(QuickFilterVariant.Export)
                        );
                    }
                    break;
            }

            return context.Key switch
            {
                "LinkEditor:LinkName" =>
                    new LinkNameInitializer<TField, TDAO>(),
                "SimpleFilter:Operation" =>
                    new FilterOperationInitializer<TField>(),
                _ =>
                    null
            };
        }


        protected readonly Dictionary<BuilderKey, ControlBuilder<TField, TDAO>> builders = new();
        protected readonly List<BuilderKey> buildersKeys = new();

        protected object? BuilderVariant(ControlBuilder<TField, TDAO> builder)
        {
            foreach (var item in builders)
                if (builder == item.Value)
                    return item.Key.Variant;

            return null;
        }

        public IControlAccessor? CreateAccessor(IBuilderContext<TField, TDAO> context)
        {
            if (context.IsView)
                return CreateViewAccessor(context);

            context.Initializer = Initializer(context);
            IControlAccessor? result = CreateAccessorByKey(context);

            return result 
                ?? context.FieldType switch
                {
                    FieldType.Label or
                    FieldType.Guid =>
                        CreateLabelAccessor(context),
                    FieldType.String =>
                        CreateTextBoxAccessor(context),
                    FieldType.Memo or
                    FieldType.ShortMemo =>
                        CreateMultilineAccessor(context),
                    FieldType.Image =>
                        CreateImageAccessor(context),
                    FieldType.Integer =>
                        CreateNumericAccessor(context),
                    FieldType.Boolean =>
                        CreateBoolAccessor(context),
                    FieldType.Extract =>
                        CreateExtractAccessor(context),
                    FieldType.MetaData =>
                        CreateMetaDataAccessor(context),
                    FieldType.Link =>
                        CreateLinkButtonAccessor(context),
                    FieldType.LinkList =>
                        CreateLinksAccessor(context),
                    FieldType.Country =>
                        CreateCountryAccessor(context),
                    FieldType.Color =>
                        CreateColorAccessor(context),
                    //TODO: may be abstract of enum and list accessors?
                    _ =>
                        CreateOtherAccessor(context),
                };
        }

        private IControlAccessor? CreateAccessorByKey(IBuilderContext<TField, TDAO> context) =>
            context.Key switch
            {
                "Category:Type" =>
                    CreateEnumAccessor<CategoryType>(context),
                "Category:BaseOnChilds" =>
                    CreateCheckBoxAccessor(context),
                "FilterGroup:Concat" or
                "Filter:Concat" =>
                    CreateEnumAccessor<FilterConcat>(context),
                "SimpleFilter:Operation" =>
                    CreateEnumAccessor<FilterOperation>(context),
                "SimpleFilter:Value" =>
                    CraeteFieldAccoringAccesorWithAdditinalContext(context),
                _ => null,
            };

        private IControlAccessor? CraeteFieldAccoringAccesorWithAdditinalContext(
            IBuilderContext<TField, TDAO> context) =>
            context.AdditionalContext is FieldAdditionalContext<TField> fieldAdditionalContext
                ? CreateAccessor(new FieldContext<TField, TDAO>(
                    context.Builder,
                    fieldAdditionalContext.Field))
                : null;

        private IControlAccessor CreateColorAccessor(IBuilderContext<TField, TDAO> context) =>
            new ColorComboBoxAccessor<TField, TDAO>(context);

        private IControlAccessor CreateMetaDataAccessor(IBuilderContext<TField, TDAO> context) =>
            new FieldAccessor<TField, TDAO>(context);

        private IControlAccessor CreateCountryAccessor(IBuilderContext<TField, TDAO> context) => 
            new CountryComboBoxAccessor<TField, TDAO>(context);

        private IControlAccessor CreateLinkButtonAccessor(IBuilderContext<TField, TDAO> context) =>
            new LinkButtonAccessor<TField, TDAO>(context);

        private IControlAccessor CreateLinksAccessor(IBuilderContext<TField, TDAO> context) =>
            context.Scope switch
            {
                ControlScope.Editor =>
                    new CustomControlAccessor<TField, TDAO, LinksListControl<TField, TDAO>, Links<TField>>(context).Init(),
                ControlScope.BatchUpdate or
                ControlScope.QuickFilter =>
                    new ButtonEditAccessor<TField, TDAO, Links<TField>, Link<TField>, LinksListControl<TField, TDAO>>(context).Init(),
                ControlScope.InfoView =>
                    new LinkButtonListAccessor<TField, TDAO>(context, ButtonListDirection.Horizontal),
                _ =>
                    new LinkButtonListAccessor<TField, TDAO>(context, ButtonListDirection.Vertical)
            };

        public IControlAccessor CreateFieldListAccessor(IBuilderContext<TField, TDAO> context) =>
            new CustomControlAccessor<TField, TDAO, 
                FieldsControl<TField, TDAO>, FieldColumns<TField>>(context)
            .Init();

        public IControlAccessor CreateCategoriesAccessor(IBuilderContext<TField, TDAO> context) =>
            new CustomControlAccessor<TField, TDAO, CategoriesControl<TField, TDAO>, Categories<TField, TDAO>>(
                context
                ).Init();

        public IControlAccessor CreateSortingListAccessor(IBuilderContext<TField, TDAO> context) =>
            new CustomControlAccessor<TField, TDAO,
                SortingsControl<TField, TDAO>, FieldSortings<TField, TDAO>>(context)
            .Init();

        protected virtual IControlAccessor? CreateOtherAccessor(IBuilderContext<TField, TDAO> context) => null;

        protected virtual IControlAccessor CreateViewAccessor(IBuilderContext<TField, TDAO> context) =>
            context.FieldType switch
            {
                FieldType.Image =>
                    CreateImageAccessor(context),
                FieldType.Link =>
                    CreateLinkButtonAccessor(context),
                FieldType.LinkList =>
                    CreateLinksAccessor(context),
                _ => CreateLabelAccessor(context)
            };

        protected IControlAccessor CreateLabelAccessor(IBuilderContext<TField, TDAO> context) =>
            new LabelAccessor<TField, TDAO>(context);

        protected IControlAccessor CreateTextBoxAccessor(IBuilderContext<TField, TDAO> context) =>
            new TextAccessor<TField, TDAO>(context);

        protected IControlAccessor CreateExtractAccessor(IBuilderContext<TField, TDAO> context)
        {
            object? variant = BuilderVariant(context.Builder);
            return context is FieldContext<TField, TDAO> accessorContext
                ? new ExtractAccessor<TField, TDAO>(accessorContext, context.IsQuickFilter,
                    (context.IsQuickFilter && variant != null && variant.Equals(QuickFilterVariant.Export)) || !context.IsQuickFilter)
                : new ComboBoxAccessor<TField, TDAO>(context);
        }

        protected IControlAccessor CreateMultilineAccessor(IBuilderContext<TField, TDAO> context) =>
            new TextAccessor<TField, TDAO>(context);

        protected IControlAccessor CreateImageAccessor(IBuilderContext<TField, TDAO> context) => 
            context.IsView 
                ? new ImageAccessor<TField, TDAO>(context) 
                : new PictureContainerAccessor<TField, TDAO>(context);

        protected IControlAccessor CreateNumericAccessor(IBuilderContext<TField, TDAO> context) =>
            context.IsQuickFilter
                ? CreateExtractAccessor(context)
                : new NumericAccessor<TField, TDAO>(context);

        protected IControlAccessor CreateBoolAccessor(IBuilderContext<TField, TDAO> context) =>
            context.IsBatchUpdate
            || context.IsCategory
            || context.IsQuickFilter
                ? new BoolAccessor<TField, TDAO>(context)
                : CreateCheckBoxAccessor(context);

        protected IControlAccessor CreateCheckBoxAccessor(IBuilderContext<TField, TDAO> context) =>
            new CheckBoxAccessor<TField, TDAO>(context.SetInitializer(
                new CheckBoxInitializer(context.Name))
            );

        public IControlAccessor CreateEnumAccessor<TItem>(IBuilderContext<TField, TDAO> context)
            where TItem : Enum =>
            (context is FieldContext<TField, TDAO> accessorContext 
            && accessorContext.AvailableDependencies)
                ? !context.IsQuickFilter
                    ? (IControlAccessor)new DependedEnumAccessor<TField, TDAO, TItem>(accessorContext)
                    : new ComboBoxAccessor<TField, TDAO>(context)
                : new EnumAccessor<TField, TDAO, TItem>(context);

        protected IControlAccessor CreateButtonEditAccessor<TItem, TList, TListControl>(IBuilderContext<TField, TDAO> context)
            where TItem : DAO, new()
            where TList : ListDAO<TItem>, new()
            where TListControl : CustomItemsControl<TField, TDAO, TList, TItem>, new() =>
            new ButtonEditAccessor<TField, TDAO, TList, TItem, TListControl>(context).Init();

        protected IControlAccessor CreateListAccessor<TItem, TList, TListControl>(
            IBuilderContext<TField, TDAO> context, List<ControlScope> simpleControlScopes)
            where TItem : DAO, new()
            where TList : ListDAO<TItem>, new()
            where TListControl : CustomItemsControl<TField, TDAO, TList, TItem>, new() =>
            (simpleControlScopes == null) || simpleControlScopes.Contains(context.Scope)
                ? new CustomControlAccessor<TField, TDAO, TListControl, TList>(context).Init()
                : CreateButtonEditAccessor<TItem, TList, TListControl>(context);

        protected IControlAccessor CreateListAccessor<TItem, TList, TListControl>(
            IBuilderContext<TField, TDAO> context, 
            ControlScope simpleControlScope)
            where TItem : DAO, new()
            where TList : ListDAO<TItem>, new()
            where TListControl : CustomItemsControl<TField, TDAO, TList, TItem>, new() =>
            CreateListAccessor<TItem, TList, TListControl>(
                context, new List<ControlScope>() { simpleControlScope }
            );

        protected IControlAccessor CreateListAccessor<TItem, TList, TListControl>(
            IBuilderContext<TField, TDAO> context)
            where TItem : DAO, new()
            where TList : ListDAO<TItem>, new()
            where TListControl : CustomItemsControl<TField, TDAO, TList, TItem>, new() =>
            CreateListAccessor<TItem, TList, TListControl>(context, ControlScope.Editor);

        public virtual ControlBuilder<TField, TDAO> Builder(ControlScope scope, bool forceNew = false, object? variant = null)
        {
            BuilderKey? builderKey = buildersKeys.Find(k => k.Scope == scope && k.Variant == variant);

            if (builderKey == null)
            {
                builderKey = new BuilderKey(scope, variant);
                buildersKeys.Add(builderKey);
            }

            if (forceNew || !builders.TryGetValue(builderKey, out var builder))
            {
                builder = new ControlBuilder<TField, TDAO>(this, scope);

                if (!forceNew)
                    builders.Add(builderKey, builder);
            }

            return builder;
        }

        public virtual IItemInfo<TField, TDAO>? CreateInfoPanel() => null;

        public virtual GridPainter<TField, TDAO>? CreateGridPainter(
            GridFieldColumns<TField> columns, GridUsage usage) => null;

        public virtual IQuickFilterLayouter<TField>? CreateQuickFilterLayouter() => null;

        public virtual IItemCard<TField, TDAO>? CreateCard(ItemViewMode viewMode) => null;

        public ItemIcon<TField, TDAO> CreateIcon() => new();

        private ItemColorer<TField, TDAO> itemColorer = default!;

        public ItemColorer<TField, TDAO> ItemColorer
        {
            get
            {
                itemColorer ??= CreateItemColorer();
                return itemColorer;
            }
        }

        protected virtual ItemColorer<TField, TDAO> CreateItemColorer() => new();

        public IItemView<TField, TDAO>? CreateItemView(ItemsViewsType viewType, ItemViewMode viewMode) =>
            viewType switch
            {
                ItemsViewsType.Cards => CreateCard(viewMode),
                ItemsViewsType.Icons => CreateIcon(),
                _ => null,
            };
    }
}