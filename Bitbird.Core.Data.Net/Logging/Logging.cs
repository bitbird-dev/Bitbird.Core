namespace Bitbird.Core.Data.Net
{
    public static class Logging
    {
        public static EventLogLogger CreateEventLogLogger(string source)
            => new EventLogLogger(source);

        public static ConsoleLogger CreateConsoleLogger()
            => new ConsoleLogger();
    }
}
