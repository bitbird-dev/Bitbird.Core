namespace Bitbird.Core
{
    public enum ApiErrorType
    {
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.IAmATeapot)]
        ApiVersionMismatch = 0,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Forbidden)]
        ForbiddenNoLogin = 1,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Forbidden)]
        ForbiddenNoRights = 2,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.NotFound)]
        NotFound = 3,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        CannotProcessFurther = 4,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        MustBeUnique = 5,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        InvalidAttribute = 6,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        InvalidParameter = 7,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        InvalidEntity = 8,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        PreconditionViolation = 9,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity)]
        OptimisticLocking = 10,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Unauthorized)]
        CredentialsAreInvalid = 11,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Unauthorized)]
        UserIsLocked = 12,
        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.Unauthorized)]
        AuthenticationIsNotActive = 13
    }
}