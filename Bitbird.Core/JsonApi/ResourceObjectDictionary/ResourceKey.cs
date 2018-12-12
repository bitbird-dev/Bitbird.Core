using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi.ResourceObjectDictionary
{
    public class ResourceKey
    {
        public string Id { get; private set; }
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
