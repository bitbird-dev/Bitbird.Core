using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.Base.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.JsonDataModel
{
    public interface IJsonApiDataModel
    {
    }

    public interface IJsonApiDataModel<T> : IJsonApiDataModel
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
