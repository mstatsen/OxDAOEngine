using OxXMLEngine.ControlFactory;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Data.Decorator
{
    public class DecoratorTypeHelper : AbstractTypeHelper<DecoratorType>
    {
        public override DecoratorType EmptyValue() => DecoratorType.Simple;

        public override string GetName(DecoratorType value) => 
            value switch
            {
                DecoratorType.Simple => "Simple",
                DecoratorType.Table => "Table",
                DecoratorType.Card => "Card",
                DecoratorType.Icon => "Icon",
                DecoratorType.FullInfo => "FullInfo",
                DecoratorType.Html => "Html",
                _ => string.Empty,
            };

        public DecoratorType DecoratorTypeByScope(ControlScope scope) => 
            scope switch
            {
                ControlScope.Table => DecoratorType.Table,
                ControlScope.Html => DecoratorType.Html,
                ControlScope.FullInfoView => DecoratorType.FullInfo,
                ControlScope.CardView => DecoratorType.Card,
                ControlScope.IconView => DecoratorType.Icon,
                _ => DecoratorType.Simple,
            };
    }
}