namespace Bitbird.Core
{
    public class ApiParameterError : ApiError
    {
        public readonly string ParameterName;

        public ApiParameterError(string parameterName, string detailMessage) 
            : base(ApiErrorType.InvalidParameter, "Parameter Error", detailMessage)
        {
            ParameterName = parameterName;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(ParameterName)}: {ParameterName}";
        }
    }
}