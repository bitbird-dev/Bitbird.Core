using System.Net;

namespace Bitbird.Core.Data.Net
{
    public enum ApiErrorType
    {
        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Forbidden)]
        ForbiddenNoLogin,
        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Forbidden)]
        ForbiddenNoRights,
        [HttpStatusCode(StatusCode = (int)HttpStatusCode.NotFound)]
        NotFound,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        MustBeUnique,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        InvalidAttribute,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        InvalidParameter,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        InvalidEntity,
        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized)]
        CredentialsAreInvalid,
        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized)]
        UserIsLocked,
        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized)]
        AuthenticationIsNotActive
    }
}