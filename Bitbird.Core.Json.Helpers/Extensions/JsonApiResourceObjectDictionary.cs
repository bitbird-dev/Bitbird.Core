using Bitbird.Core.Json.Helpers.Base.Converters;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Json.JsonApi.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.ApiResource.Extensions
{
    internal static class JsonApiResourceObjectDictionaryExtensions
    {
        #region GetResource

        public static JsonApiResourceObject GetResource(this JsonApiResourceObjectDictionary resourceDictionary, object id, JsonApiResource apiResource)
        {
            return resourceDictionary.GetResource(BtbrdCoreIdConverters.ConvertToString(id), apiResource.ResourceType);
        }

        #endregion
    }
}
