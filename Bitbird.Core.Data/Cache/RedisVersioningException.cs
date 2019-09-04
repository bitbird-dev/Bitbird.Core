using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Cache
{
    public class RedisVersioningException : Exception
    {
        public RedisVersioningException([NotNull] string message)
            : base(message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
        }
    }
}