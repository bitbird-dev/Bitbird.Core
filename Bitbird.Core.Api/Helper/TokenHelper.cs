using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Helper
{
    public static class TokenHelper
    {
        [NotNull]
        private static readonly Dictionary<Type, TimeSpan> ValidityDuration = new Dictionary<Type, TimeSpan>();
        private static readonly TimeSpan DefaultValidityDuration = TimeSpan.FromHours(1);

        public static void RegisterValidity([NotNull] Type type, TimeSpan duration)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (ValidityDuration.ContainsKey(type))
                ValidityDuration[type] = duration;

            ValidityDuration.Add(type, duration);
        }

        public static TimeSpan GetValidityDuration([NotNull] Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return ValidityDuration.TryGetValue(type, out var ts) ? ts : DefaultValidityDuration;
        }

        public static string GenerateNewTokenKey(int length = 60)
        {
            var guid = Guid.NewGuid().ToString("N");
            if (guid.Length > length)
                guid = guid.Substring(0, length);
            return guid;
        }
    }
}
