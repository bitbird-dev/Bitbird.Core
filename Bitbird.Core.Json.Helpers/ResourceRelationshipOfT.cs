using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Bitbird.Core.Json.Helpers.ApiResource
{
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1649:File name must match first type name",
        Justification = "Non-generic version exists")]
    internal class ResourceRelationship<T> : ResourceRelationship
            where T : JsonApiResource, new()
    {
        internal ResourceRelationship(string name, string idPropertyName, string urlPath, RelationshipKind kind, T resource, LinkType withLinks)
            : base(name, idPropertyName, urlPath, kind, resource, withLinks)
        {
        }
    }
}
