namespace Bitbird.Core.Api.AzureReporting.Net.Models
{
    public enum Severity
    {
        Critical = 1,
        High = 2,
        Medium = 3,
        Low = 4
    }
    public static class SeverityExtensions
    {
        public static string FormatForAzure(this Severity severity)
        {
            return $"{(int)severity} - {severity}";
        }
    }
}