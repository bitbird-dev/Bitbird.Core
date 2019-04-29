namespace Bitbird.Core
{
    public class ApiNotFoundError : ApiError
    {
        public ApiNotFoundError(string elementTypeName, string elementIdentifier, string identifierInfo = null)
            : base(ApiErrorType.NotFound, "Not found", $"A {elementTypeName} with the given identifier could not be found (identifier='{elementIdentifier}', identifier info='{identifierInfo ?? "primary key"}').")
        {
        }

        public static ApiNotFoundError Create<TId>(string elementTypeName, TId elementIdentifier, string identifierInfo = null)
        {
            return new ApiNotFoundError(elementTypeName, elementIdentifier == null ? "null" : elementIdentifier.ToString(), identifierInfo);
        }
    }
}