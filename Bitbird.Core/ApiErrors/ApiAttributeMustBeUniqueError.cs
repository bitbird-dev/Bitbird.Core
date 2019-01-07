namespace Bitbird.Core
{
    public class ApiAttributeMustBeUniqueError : ApiAttributeError
    {
        public readonly string Value;

        public ApiAttributeMustBeUniqueError(string attributeName, string value)
            : base(attributeName, $"The {attributeName} '{value}' already exists.", ApiErrorType.MustBeUnique)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Value)}: {Value}";
        }
    }
}