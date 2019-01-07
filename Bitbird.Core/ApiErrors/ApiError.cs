namespace Bitbird.Core
{
    public class ApiError
    {
        public readonly ApiErrorType Type;
        public readonly string Title;
        public readonly string DetailMessage;

        public ApiError(ApiErrorType type, string title, string detailMessage)
        {
            Type = type;
            Title = title;
            DetailMessage = detailMessage;
        }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Title)}: {Title}, {nameof(DetailMessage)}: {DetailMessage}";
        }
    }
}