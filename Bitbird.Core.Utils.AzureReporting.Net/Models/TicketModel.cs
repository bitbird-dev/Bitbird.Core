using Newtonsoft.Json.Linq;

namespace Bitbird.Core.Api.AzureReporting.Net.Models
{
    public class TicketModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string Url { get; set; }


        public static TicketModel FromJToken(JToken token)
        {
            return new TicketModel
            {
                Id = token["id"].Value<long>(),
                Title = token["fields"]["System.Title"].Value<string>(),
                Type = token["fields"]["System.WorkItemType"].Value<string>(),
                State = token["fields"]["System.State"].Value<string>(),
                Url = token["_links"]["html"]["href"].Value<string>()
            };
        }
    }
}