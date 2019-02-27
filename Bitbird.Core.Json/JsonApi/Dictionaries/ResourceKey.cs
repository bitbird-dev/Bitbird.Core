using System.Runtime.Serialization;

namespace Bitbird.Core.Json.JsonApi.Dictionaries
{
    public class ResourceKey
    {
        [DataMember(Name = "id")]
        public string Id { get; private set; }

        [DataMember(Name = "type")]
        public string Type { get; private set; }

        public ResourceKey(string id, string type)
        {
            Id = id;
            Type = type;
        }

        public override int GetHashCode()
        {
            return (Type + Id).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }
    }
}
