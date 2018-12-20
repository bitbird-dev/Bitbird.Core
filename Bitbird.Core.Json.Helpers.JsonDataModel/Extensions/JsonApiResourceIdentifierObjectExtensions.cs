using Bitbird.Core.Json.JsonApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.JsonDataModel.Extensions
{
    public static class JsonApiResourceIdentifierObjectExtensions
    {
        public static IJsonApiDataModel ToObject(this JsonApiResourceIdentifierObject resourceIdentifierObject, Type type)
        {
            IJsonApiDataModel result = null;

            try
            {
                result = Activator.CreateInstance(type) as IJsonApiDataModel;
                result.SetIdFromString(resourceIdentifierObject.Id);
            }
            catch { }

            return result;
        }
    }
}
