using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonApiToOneRelationship : JsonApiRelationshipBase
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiResourceIdentifierObject Data
        {
            get
            {
                return (RawData as List<JsonApiResourceIdentifierObject>)?.FirstOrDefault();
            }

            set
            {
                RawData = new List<JsonApiResourceIdentifierObject> { value };
            }
        }
    }
}
