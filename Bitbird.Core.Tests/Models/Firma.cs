using Bitbird.Core.Json.Helpers.JsonDataModel.Attributes;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Json.JsonApi.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Json.Tests.Models
{
    [JsonApiClass("firmacustom")]
    public class Firma : JsonApiBaseModel
    {
        public string FirmenName { get; set; }

        [JsonApiRelationId(nameof(Fahrer))]
        public int? FahrerId { get; set; }
        public Fahrer Fahrer { get; set; }
        
        public IEnumerable<Fahrzeug> FahrZeuge { get; set; }
        
    }
}
