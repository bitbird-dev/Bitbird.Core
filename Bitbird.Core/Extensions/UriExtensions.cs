using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Extensions
{
    internal static class UriExtensions
    {
        public static string GetFullHost(this Uri uri, bool writePort = false)
        {
            StringBuilder bob = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(uri.Scheme)) bob.Append(uri.Scheme).Append("://");
            bob.Append(uri.Host);
            if (writePort) bob.Append(":").Append(uri.Port);
            return bob.ToString();
        }
    }
}
