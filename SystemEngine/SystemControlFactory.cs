using OxXMLEngine.ControlFactory;

namespace OxXMLEngine.SystemEngine
{
    public abstract class SystemControlFactory<TSetting>
        : ControlFactory<TSetting, SystemRootDAO<TSetting>>
        where TSetting : Enum
    {

        public override ControlBuilder<TSetting, SystemRootDAO<TSetting>> Builder(
            ControlScope scope, bool forceNew = false)
        {
            ControlBuilder<TSetting, SystemRootDAO<TSetting>> builder = base.Builder(scope, forceNew);
            builder.BuildOnly = true;
            return builder;
        }

        protected override bool IsMetaDataField(TSetting field) => false;
    }
}