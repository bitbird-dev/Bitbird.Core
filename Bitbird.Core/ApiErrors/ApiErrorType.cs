using System.Net;

namespace Bitbird.Core
{
    public enum ApiErrorType
    {
        [HttpStatusCode(StatusCode = 418)]
        ApiVersionMismatch,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Forbidden)]
        ForbiddenNoLogin,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Forbidden)]
        ForbiddenNoRights,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.NotFound)]
        NotFound,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        CannotProcessFurther,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        MustBeUnique,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        InvalidAttribute,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        InvalidParameter,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        InvalidEntity,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        PreconditionViolation,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        OptimisticLocking,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Unauthorized)]
        CredentialsAreInvalid,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Unauthorized)]
        UserIsLocked,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Unauthorized)]
        AuthenticationIsNotActive
    }
}