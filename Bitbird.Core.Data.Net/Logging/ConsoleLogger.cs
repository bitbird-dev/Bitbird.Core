using System;
using System.Text;
using Newtonsoft.Json;
using Bitbird.Core.Extensions;

namespace Bitbird.Core.Data.Net
{
    public class ConsoleLogger : ILogger
    {
        private static string Format(LogLevel level, int? eventId, short? category)
        {
            var sb = new StringBuilder();
            sb.Append(level);
            if (eventId.HasValue)
            {
                sb.AppendFormat(":{0}", eventId.Value);
                if (category.HasValue)
                {
                    sb.AppendFormat(":{0}", category.Value);
                }
            }
            return sb.ToString();
        }
        private static string FormatContext(LogLevel level, int? eventId, short? category)
        {
            var sb = new StringBuilder();
            sb.Append(level);
            if (eventId.HasValue)
            {
                sb.AppendFormat(":{0}", eventId.Value);
                if (category.HasValue)
                {
                    sb.AppendFormat(":{0}", category.Value);
                }
            }
            return sb.ToString();
        }

        public void Write(LogLevel level, string message, int? eventId = null, short? category = null)
        {
            Console.WriteLine($"@{DateTime.Now:HH:mm:ss} [{FormatContext(level, eventId, category)}] {message.ConsistentNewLines()}");
        }

        public void Write<T>(LogLevel level, T data, string message, int? eventId = null, short? category = null)
        {
            message = message.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);

            Console.WriteLine($"@{DateTime.Now:HH:mm:ss} [{FormatContext(level, eventId, category)}] {message.ConsistentNewLines()}{Environment.NewLine}Data.Type={data?.GetType().FullName??"NULL"}{Environment.NewLine}Data:{Environment.NewLine}{JsonConvert.SerializeObject(data)}");
        }

        public void Dispose()
        {
        }
    }
}