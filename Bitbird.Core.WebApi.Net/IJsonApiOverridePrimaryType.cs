namespace Bitbird.Core.WebApi.Net
{
    public interface IJsonApiOverridePrimaryType
    {
        object Data { get; }
        string Type { get; }
    }
}