using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Decorator;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory
{
    public class ControlBuilder<TField, TDAO> 
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public ControlScope Scope { get; set; }
        public bool Modified { get; internal set; }

        private readonly Dictionary<IBuilderContext<TField, TDAO>, IControlAccessor> Accessors = new();

        private readonly Dictionary<TField, FieldContext<TField, TDAO>> FieldContexts = new();

        private readonly Dictionary<string, IBuilderContext<TField, TDAO>> SimpleContexts = new();

        private void ModifiedHandler(object? sender, EventArgs e) =>
            Modified = true;

        public ControlBuilder(ControlFactory<TField, TDAO> controlFactory, ControlScope scope)
        {
            Scope = scope;
            Factory = controlFactory;
            Layouter = new ControlLayouter<TField, TDAO>(this);
        }

        internal void DisposeControls()
        {
            foreach (IControlAccessor accessor in Accessors.Values)
                accessor.Control.Dispose();

            Accessors.Clear();
        }

        private IControlAccessor Accessor(IBuilderContext<TField, TDAO> context, 
            Func<IBuilderContext<TField, TDAO>, IControlAccessor>? createFunction = null)
        {
            if (!Accessors.TryGetValue(context, out var controlAccessor))
            {
                if (context is FieldContext<TField, TDAO> fieldContext)
                {
                    ITypeHelper helper = TypeHelper.Helper(default(TField)!);

                    if (helper is FieldHelper<TField> fieldHelper)
                        fieldHelper.FillAdditionalContext(fieldContext.Field, context);
                }

                controlAccessor = 
                    createFunction != null 
                        ? createFunction(context) 
                        : Factory.CreateAccessor(context);

                if (controlAccessor == null)
                    throw new KeyNotFoundException($"Control Accessor not exist {context}");

                Accessors.Add(context, controlAccessor);
            }

            return controlAccessor;
        }


        public IControlAccessor Accessor(TField field) =>
            Accessor(Context(field));

        public IControlAccessor Accessor(string key, FieldType fieldType, object? additionalContext = null) =>
            Accessor(Context(key, fieldType, additionalContext));

        public EnumAccessor<TField, TDAO, TItem> Accessor<TItem>(object? additionalContext = null)
            where TItem : Enum => 
            (EnumAccessor<TField, TDAO, TItem>)
                Accessor(
                    Context(typeof(TItem).Name, FieldType.Enum, additionalContext),
                    (c) => Factory.CreateEnumAccessor<TItem>(c)
                );

        public EnumAccessor<TField, TDAO, TItem> Accessor<TItem>(string name, FieldType fieldType, 
            object? additionalContext = null)
            where TItem : Enum =>
            (EnumAccessor<TField, TDAO, TItem>)Accessor(Context(name, fieldType, additionalContext));

        public IControlAccessor FieldListAccessor(object? additionalContext = null) =>
            Accessor(
                Context("FieldListAccessor", FieldType.Custom, additionalContext),
                c => Factory.CreateFieldListAccessor(c)
            );

        public IControlAccessor CategoriesAccessor(object? additionalContext = null) =>
            Accessor(
                Context("CategoriesAccessor", FieldType.Custom, additionalContext),
                c => Factory.CreateCategoriesAccessor(c)
            );

        public IControlAccessor SortingListAccessor(object? additionalContext = null) =>
            Accessor(
                Context("SortingListAccessor", FieldType.Custom, additionalContext),
                c => Factory.CreateSortingListAccessor(c)
            );

        public T Control<T>(TField field)
            where T : Control =>
            (T)Accessor(field).Control;

        public FieldContext<TField, TDAO> Context(TField field)
        {
            if (!FieldContexts.TryGetValue(field, out var context))
            {
                context = new FieldContext<TField, TDAO>(this, field)
                {
                    AvailableDependencies = !BuildOnly
                };
                FieldContexts.Add(field, context);
            }
            
            return context;
        }

        public IBuilderContext<TField, TDAO> Context(string key, FieldType fieldType, object? additionalContext)
        {
            string hashKey = $"{key}_{fieldType}";

            if (additionalContext != null)
                hashKey += $"_{additionalContext}";

            if (!SimpleContexts.TryGetValue(hashKey, out var context))
            {
                string name = key;

                if (name.IndexOf(":") > 0)
                    name = name[(name.IndexOf(":") + 1)..];

                context = new AccessorContext<TField, TDAO>(this, key, name, fieldType)
                {
                    AdditionalContext = additionalContext
                };
                SimpleContexts.Add(hashKey, context);
            }

            return context;
        }

        public void FillControls(TDAO item)
        {
            if (BuildOnly)
                return;

            FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();
            List<TField>? availableFields = fieldHelper.AvailableFields(Scope);
            DecoratorFactory<TField, TDAO> decoratorFactory = DataManager.DecoratorFactory<TField, TDAO>();
            Decorator<TField, TDAO> decorator = decoratorFactory.Decorator(Scope, item);
            Decorator<TField, TDAO> simpleDecorator = decoratorFactory.Decorator(DecoratorType.Simple, item);
            Decorator<TField, TDAO>? currentDecorator = null;
            object? value = null;

            foreach (TField field in TypeHelper.All<TField>())
            {
                IControlAccessor? accessor = null;
                currentDecorator = null;

                if (item == null)
                {
                    accessor = this[field];
                    value = null;
                }
                else
                {
                    switch (Scope)
                    {
                        case ControlScope.InfoView:
                        case ControlScope.IconView:
                        case ControlScope.CardView:
                            if ((availableFields != null) && availableFields.Contains(field))
                                currentDecorator = decorator;
                            break;
                        default:
                            currentDecorator = fieldHelper.CalcedFields.Contains(field) 
                                ? decorator 
                                : fieldHelper.EditingFieldsExtended.Contains(field) 
                                    ? simpleDecorator 
                                    : null;

                            break;
                    }
                }

                if (currentDecorator != null)
                {
                    value = currentDecorator[field];
                    accessor = this[field];
                }

                if (accessor == null)
                    continue;

                accessor.ValueChangeHandler -= ModifiedHandler;

                try
                {
                    if (accessor.Control is ICustomControl<TField, TDAO> listControl)
                        listControl.OwnerDAO = item;

                    accessor.RenewControl();

                    if (accessor is IDependedControl dependedAccessor)
                        dependedAccessor.ApplyDependencies();

                    accessor.Value = value;
                }
                finally
                {
                    accessor.ValueChangeHandler += ModifiedHandler;
                }
            }

            Modified = false;
        }

        public void ApplyDependencies()
        {
            if (BuildOnly)
                return;

            if (TypeHelper.Helper<ControlScopeHelper>().IsView(Scope))
                return;

            FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

            foreach (TField field in TypeHelper.All<TField>())
            {
                if (!fieldHelper.CalcedFields.Contains(field) &&
                    !fieldHelper.EditingFieldsExtended.Contains(field))
                    continue;

                if (this[field] is IDependedControl dependedAccessor)
                    dependedAccessor.ApplyDependencies();
            }
        }

        private void GrabEditorControls(IFieldMapping<TField> item)
        {
            foreach (TField field in TypeHelper.FieldHelper<TField>().EditingFields)
                item[field] = Value(field);
        }

        public void GrabControls(IFieldMapping<TField> item, List<TField>? fields = null)
        {
            if (BuildOnly)
                return;

            if (item is FilterGroup<TField, TDAO> filterGroup)
                GrabQuickFilterControls(filterGroup, fields);
            else
                GrabEditorControls(item);
        }

        public void GrabQuickFilterControls(FilterGroup<TField, TDAO> filterGroup, List<TField>? fields)
        {
            if (fields == null)
                return;

            foreach (TField field in fields)
            {
                if (this[field].IsEmpty)
                    continue;

                filterGroup.Add(field, Value(field));
            }
        }

        public IControlAccessor this[TField field] => Accessor(field);

        public T? Value<T>(TField field) =>
            (T?)(this[field].Value is null
                or not T
                ? default(T)
                : this[field].Value);

        public object? Value(TField field) => 
            this[field].Value;

        public object? ObjectValue(TField field) =>
            this[field].ObjectValue;

        public void SetVisible(TField field, bool visible)
        {
            this[field].Visible = visible;
            OxLabel? label = Layouter.PlacedControl(field)?.Label;

            if (label != null)
                label.Visible = visible;
        }

        public Control Control(TField field) =>
            this[field].Control;

        public ControlFactory<TField, TDAO> Factory { get; set; }

        public readonly ControlLayouter<TField, TDAO> Layouter;
        public bool BuildOnly = false;
    }
}