using System;

namespace Bitbird.Core.Data.Validation
{
    public static class AttributeTranslations
    {
        public static bool TryTranslateAttribute(object attribute, out object result)
        {
            var success = TryTranslateAttribute((Attribute) attribute, out var typedResult);
            result = typedResult;
            return success;
        }
        public static bool TryTranslateAttribute(Attribute attribute, out Attribute result)
        {
            if (attribute.GetType().Name.Equals("RequiredAttribute"))
            {
                result = new ValidatorCheckNotNullAttribute();
                return true;
            }

            if (attribute.GetType().Name.Equals("StringLengthAttribute"))
            {
                var maximumLength = Convert.ToInt32(attribute.GetType().GetProperty("MaximumLength")?.GetValue(attribute));
                result = new ValidatorCheckMaxStringLengthAttribute(maximumLength);
                return true;
            }

            result = attribute;
            return false;
        }
    }
}