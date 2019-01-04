namespace Bitbird.Core.Data.Net
{
    public interface IUserToken<TUserKey, TTokenKey> : IToken<TTokenKey>
        where TUserKey : struct
        where TTokenKey : struct
    {
        TUserKey UserId { get; set; }
    }
}