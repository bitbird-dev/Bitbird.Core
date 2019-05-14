using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Json.JsonApi.Attributes;

namespace Bitbird.Core.Json.Tests.Models
{
    [JsonApiClass("fahrzeuCustomTypeName")]
    public class Fahrzeug : JsonApiBaseModel
    {
        public int Kilometerstand { get; set; }
    }
}
