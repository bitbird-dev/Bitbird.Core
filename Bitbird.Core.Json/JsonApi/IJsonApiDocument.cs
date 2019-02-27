using Bitbird.Core.Json.JsonApi.Dictionaries;
using System.Collections.Generic;

namespace Bitbird.Core.Json.JsonApi
{
    public interface IJsonApiDocument
    {
        IEnumerable<JsonApiErrorObject> Errors { get; set; }
        
        object Meta { get; set; }
        
        JsonApiLinksObject Links { get; set; }
        
        JsonApiResourceObjectDictionary Included { get; set; }
    }
}
