namespace Bitbird.Core.Data.Net
{
    public class ApiNotFoundError : ApiError
    {
        public ApiNotFoundError(string elementTypeName, long? elementIdentifier, string identifierInfo = null)
            : this(elementTypeName, elementIdentifier.HasValue ? elementIdentifier.ToString() : "null", identifierInfo)
        {
        }
        public ApiNotFoundError(string elementTypeName, string elementIdentifier, string identifierInfo = null)
            : base(ApiErrorType.NotFound, "Not found", $"A {elementTypeName} with the given identifier could not be found (identifier='{elementIdentifier}', identifier info='{identifierInfo ?? "primary key"}').")
        {
        }
    }
}