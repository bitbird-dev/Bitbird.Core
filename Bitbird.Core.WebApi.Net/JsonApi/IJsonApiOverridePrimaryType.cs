namespace Bitbird.Core.WebApi.JsonApi
{
    public interface IJsonApiOverridePrimaryType
    {
        object Data { get; }
        string Type { get; }
    }
}