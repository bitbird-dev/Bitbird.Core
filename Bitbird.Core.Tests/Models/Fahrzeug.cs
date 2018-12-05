using Bitbird.Core.JsonApi;
using Bitbird.Core.JsonApi.Attributes;

namespace Bitbird.Core.Tests.Models
{
    [JsonApiClass("fahrzeuCustomTypeName")]
    public class Fahrzeug : JsonApiBaseModel
    {
        public int Kilometerstand { get; set; }
    }
}
