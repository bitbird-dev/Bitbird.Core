namespace Bitbird.Core.Api.AzureReporting.Net.Models
{
    public enum Priority
    {
        MustFix = 1,
        High = 2,
        Medium = 3,
        Unimportant = 4
    }
    public static class PriorityExtensions
    {
        public static string FormatForAzure(this Priority priority)
        {
            return $"{(int)priority}";
        }
    }
}