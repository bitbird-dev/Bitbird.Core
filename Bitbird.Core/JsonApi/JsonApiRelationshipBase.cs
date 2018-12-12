using Bitbird.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    /// <summary>
    /// A “relationship object” MUST contain at least one of the following:
    /// 
    /// links: a links object containing at least one of the following:
    ///     self: a link for the relationship itself(a “relationship link”). 
    ///         This link allows the client to directly manipulate the relationship.
    ///         For example, removing an author through an article’s relationship URL would 
    ///         disconnect the person from the article without deleting the people resource itself. 
    ///         When fetched successfully, this link returns the linkage for the related resources as its primary data. 
    ///         (See Fetching Relationships.)
    ///     related: a related resource link
    /// data: resource linkage
    /// meta: a meta object that contains non-standard meta-information about the relationship.
    /// 
    /// A relationship object that represents a to-many relationship MAY also contain pagination links under the links member, 
    /// as described below. Any pagination links in a relationship object MUST paginate the relationship data, 
    /// not the related resources.
    /// </summary>
    [JsonConverter(typeof(JsonApiRelationshipsObjectConverter))]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public abstract class JsonApiRelationshipBase
    {
        [JsonIgnore]
        protected object RawData { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLinksObject Links { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; set; }
    }
}
