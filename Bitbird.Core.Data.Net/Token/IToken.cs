using System;

namespace Bitbird.Core.Data.Net
{
    public interface IToken<TKey>
        where TKey : struct
    {
        TKey Id { get; set; }
        string TokenKey { get; set; }
        DateTime ValidUntil { get; set; }
    }
}