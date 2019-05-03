namespace Bitbird.Core.WebApi.JsonApi
{
    public class JsonApiOverridePrimaryType<T> : IJsonApiOverridePrimaryType
    {
        public object Data { get; }
        public string Type { get; }

        public JsonApiOverridePrimaryType(T data, string type)
        {
            Data = data;
            Type = type;
        }
    }
    public class JsonApiOverridePrimaryType : IJsonApiOverridePrimaryType
    {
        public object Data { get; }
        public string Type { get; }

        public JsonApiOverridePrimaryType(object data, string type)
        {
            Data = data;
            Type = type;
        }
    }
}