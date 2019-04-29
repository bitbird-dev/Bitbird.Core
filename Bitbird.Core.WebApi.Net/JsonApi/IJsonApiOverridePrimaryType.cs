namespace Bitbird.Core.WebApi.Net.JsonApi
{
    public interface IJsonApiOverridePrimaryType
    {
        object Data { get; }
        string Type { get; }
    }
}