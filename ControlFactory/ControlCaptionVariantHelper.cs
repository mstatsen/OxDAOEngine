﻿using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory
{
    public class ControlCaptionVariantHelper : AbstractTypeHelper<ControlCaptionVariant>
    {
        public override ControlCaptionVariant EmptyValue() =>
            ControlCaptionVariant.Left;

        public override string GetName(ControlCaptionVariant value) => 
            value switch
            {
                ControlCaptionVariant.Left => "Left",
                ControlCaptionVariant.Top => "Top",
                _ => "Default",
            };
    }
}