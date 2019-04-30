using System;
using System.Collections.Generic;
using Bitbird.Core.WebApi.Net.JsonApi;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    public class ActionMetaData
    {
        public readonly string Name;
        public readonly string HttpMethod;
        public readonly string RelativeRoute;
        public readonly bool SupportsQueryInfo;
        public readonly Type ReturnType;
        public readonly KeyValuePair<string, Type>? BodyParameter;
        public readonly Dictionary<string, Type> RouteParameter;
        public readonly JsonApiAttribute JsonApiAttribute;

        public ActionMetaData(string name, string httpMethod, string relativeRoute, bool supportsQueryInfo, Type returnType, KeyValuePair<string, Type>? bodyParameter, Dictionary<string, Type> routeParameter, JsonApiAttribute jsonApiAttribute)
        {
            Name = name;
            HttpMethod = httpMethod;
            RelativeRoute = relativeRoute;
            SupportsQueryInfo = supportsQueryInfo;
            ReturnType = returnType;
            BodyParameter = bodyParameter;
            RouteParameter = routeParameter;
            JsonApiAttribute = jsonApiAttribute;
        }
    }
}