namespace Bitbird.Core
{
    public class ApiPreconditionError : ApiError
    {
        public ApiPreconditionError(string violationDescription)
            : base(ApiErrorType.PreconditionViolation, "Precondition violated", violationDescription)
        {
        }
    }
}