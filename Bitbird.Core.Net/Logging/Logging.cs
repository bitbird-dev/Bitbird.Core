namespace Bitbird.Core.Log
{
    public static class Logging
    {
        public static EventLogLogger CreateEventLogLogger(string log, string source)
            => new EventLogLogger(log, source);

        public static ConsoleLogger CreateConsoleLogger()
            => new ConsoleLogger();
    }
}
