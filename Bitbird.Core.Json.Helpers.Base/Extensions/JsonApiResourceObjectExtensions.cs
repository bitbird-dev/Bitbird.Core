using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.Base.Converters;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Json.JsonApi.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.Base.Extensions
{
    public static class JsonApiResourceObjectExtensions
    {
        public static JsonApiResourceObject GetResource(this JsonApiResourceObjectDictionary resourceDictionary, object id, Type type)
        {
            return resourceDictionary.GetResource(BtbrdCoreIdConverters.ConvertToString(id), type.GetJsonApiClassName());
        }
    }
}
