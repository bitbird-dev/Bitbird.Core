using System;

namespace Bitbird.Core.Log
{
    public interface ILogger : IDisposable
    {
        void Write(LogLevel level, string message, int? eventId = null, short? category = null);
        void Write<T>(LogLevel level, T data, string message, int? eventId = null, short? category = null);
    }
}