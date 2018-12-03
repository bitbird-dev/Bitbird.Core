using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonApiToManyRelationship : JsonApiRelationshipBase
    {
        [JsonProperty("data",NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<JsonApiResourceIdentifierObject> Data
        {
            get
            {
                return RawData as IEnumerable<JsonApiResourceIdentifierObject>;
            }

            set
            {
                RawData = value;
            }
        }
    }
}
