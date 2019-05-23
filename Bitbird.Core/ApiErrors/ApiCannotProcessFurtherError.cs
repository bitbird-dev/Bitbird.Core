using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiCannotProcessFurtherError : ApiError
    {
        public ApiCannotProcessFurtherError([CanBeNull] string detailInfo)
            : base(ApiErrorType.CannotProcessFurther, 
                ApiErrorMessages.ApiErrorType_CannotProcessFurther_Title, 
                string.Format(ApiErrorMessages.ApiErrorType_CannotProcessFurther_Title, detailInfo ?? string.Empty))
        {
        }
    }
}