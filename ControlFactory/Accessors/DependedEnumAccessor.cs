using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class DependedEnumAccessor<TField, TDAO, TItem> 
        : EnumAccessor<TField, TDAO, TItem>, IDependedControl
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TItem : Enum
    {
        private readonly FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();
        protected override bool AvailableValue(TItem value)
        {
            if (!base.AvailableValue(value))
                return false;

            if ((Context is FieldContext<TField, TDAO> accessorContext) &&
                applyDependenciesProcess)
            {
                List<TField> dependsOnFields = fieldHelper.Depended(Field);

                foreach (TField dependsOnField in dependsOnFields)
                {
                    object? dependsOnValue = accessorContext.Builder.Value(dependsOnField);

                    if (dependsOnValue == null)
                        continue;

                    if (!TypeHelper.DependedList<TField, TItem>(
                            dependsOnField,
                            dependsOnValue
                        ).Contains(value))
                        return false;
                }
            }

            return true;
        }

        public DependedEnumAccessor(FieldContext<TField, TDAO> context) : base(context)
        {
            Field = context.Field;
            RenewControl(true);
        }

        public void ApplyDependencies()
        {
            applyDependenciesProcess = true;
            SuspendLayout();

            try
            {
                RenewControl(applyDependenciesProcess);
            }
            finally
            {
                applyDependenciesProcess = false;
                ResumeLayout();
            }
        }

        private readonly TField Field;
        private bool applyDependenciesProcess = false;
    }
}
