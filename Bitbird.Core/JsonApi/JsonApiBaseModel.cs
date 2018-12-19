using Bitbird.Core.Extensions;
using Bitbird.Core.JsonApi.Attributes;
using Bitbird.Core.JsonApi.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    public interface IJsonApiDataModel
    {
    }

    public interface IJsonApiIdModel<T> : IJsonApiDataModel
    {
        T Id { get; set; }
    }
    

    public static class IJsonApiDataObjectExtensions
    {
        public static string GetJsonApiClassName(this IJsonApiDataModel obj)
        {
            return obj.GetType().GetJsonApiClassName();
        }

        public static string GetIdAsString(this IJsonApiDataModel obj)
        {
            var info = obj.GetType().GetProperty("Id");
            return BtbrdCoreIdConverters.ConvertToString(info.GetValueFast(obj));
        }

        public static void SetIdFromString(this IJsonApiDataModel obj, string stringId)
        {
            var info = obj.GetType().GetProperty("Id");
            info.SetValueFast(obj, BtbrdCoreIdConverters.ConvertFromString(stringId, info.PropertyType));
        }
    }
}
