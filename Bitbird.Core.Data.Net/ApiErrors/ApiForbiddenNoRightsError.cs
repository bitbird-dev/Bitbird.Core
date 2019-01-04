namespace Bitbird.Core.Data.Net
{
    public class ApiForbiddenNoRightsError : ApiError
    {
        public ApiForbiddenNoRightsError(string actionDescription)
            : base(ApiErrorType.ForbiddenNoRights, "Insufficient rights", $"The current session does not have the right to perform this action (Action description: {actionDescription}).")
        {
        }
    }
}