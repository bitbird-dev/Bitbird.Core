using System;
using System.Diagnostics;
using System.Text;
using Bitbird.Core.Extensions;
using Newtonsoft.Json;

namespace Bitbird.Core.Log
{
    public class EventLogLogger : ILogger
    {
        public readonly string Source;
        public readonly EventLog EventLog;

        public EventLogLogger(string source)
        {
            Source = source;

            const string log = "Application";

            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, log);

            EventLog = new EventLog
            {
                Source = source,
                Log = log
            };
        }
        
        public void Write(LogLevel level, string message, int? eventId = null, short? category = null)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Timestamp = Local:{0:o} Utc:{1:o}", DateTime.Now, DateTime.UtcNow); sb.AppendLine();
            sb.AppendFormat("Level = {0}", level); sb.AppendLine();
            sb.AppendFormat("EventId = {0}", eventId); sb.AppendLine();
            sb.AppendFormat("Category = {0}", category); sb.AppendLine();
            sb.AppendLine("[Message]");
            sb.AppendLine(message.ConsistentNewLines());

            WriteEntry(level, sb.ToString(), eventId, category);
        }

        public void Write<T>(LogLevel level, T data, string message, int? eventId = null, short? category = null)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Timestamp = Local:{0:o} Utc:{1:o}", DateTime.Now, DateTime.UtcNow); sb.AppendLine();
            sb.AppendFormat("Level = {0}", level); sb.AppendLine();
            sb.AppendFormat("EventId = {0}", eventId); sb.AppendLine();
            sb.AppendFormat("Category = {0}", category); sb.AppendLine();
            sb.AppendFormat("Data Type = {0}", data?.GetType().FullName ?? "NULL"); sb.AppendLine();
            sb.AppendLine("[Message]");
            sb.AppendLine(message.ConsistentNewLines() ?? string.Empty);
            sb.AppendLine("[Data]");
            sb.AppendLine(JsonConvert.SerializeObject(data));

            WriteEntry(level, sb.ToString(), eventId, category);
        }

        private void WriteEntry(LogLevel level, string content, int? eventId, short? category)
        {
            if (eventId.HasValue && category.HasValue)
                EventLog.WriteEntry(content, (EventLogEntryType)(int)level, eventId.Value, category.Value);
            else if (eventId.HasValue)
                EventLog.WriteEntry(content, (EventLogEntryType)(int)level, eventId.Value);
            else
                EventLog.WriteEntry(content, (EventLogEntryType)(int)level);
        }

        public void Dispose()
        {
            EventLog?.Dispose();
        }
    }
}