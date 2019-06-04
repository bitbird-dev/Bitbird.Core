using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiCannotProcessFurtherError : ApiError
    {
        public ApiCannotProcessFurtherError([CanBeNull] string detailInfo)
            : base(ApiErrorType.CannotProcessFurther, 
                ApiErrorMessages.ApiCannotProcessFurtherError_Title, 
                string.Format(ApiErrorMessages.ApiCannotProcessFurtherError_Message, detailInfo ?? string.Empty))
        {
        }
    }
}