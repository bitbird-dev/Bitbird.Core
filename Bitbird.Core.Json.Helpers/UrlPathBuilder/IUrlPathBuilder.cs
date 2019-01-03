using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.ApiResource.UrlPathBuilder
{
    /// <summary>
    /// Used to build url paths.
    /// </summary>
    public interface IUrlPathBuilder
    {
        /// <summary>
        /// Builds the canonical path to a resource collection.
        /// </summary>
        /// <param name="resource">The resource this path refers to.</param>
        /// <returns>A <see cref="string"/> containing the path.</returns>
        string BuildCanonicalPath(JsonApiResource resource);

        /// <summary>
        /// Builds the canonical path to a specific resource.
        /// </summary>
        /// <param name="resource">The resource this path refers to.</param>
        /// <param name="id">The id of the resource.</param>
        /// <returns>A <see cref="string"/> containing the path.</returns>
        string BuildCanonicalPath(JsonApiResource resource, string id);

        /// <summary>
        /// Builds the path to a related resource.
        /// </summary>
        /// <param name="resource">The resource that contains the relationship.</param>
        /// <param name="id">The id of the resource.</param>
        /// <param name="relationship">The relationship this path refers to.</param>
        /// <returns>A <see cref="string"/> containing the path.</returns>
        string BuildRelationshipPath(JsonApiResource resource, string id, ResourceRelationship relationship);

        /// <summary>
        /// Builds the self path to a related resource.
        /// </summary>
        /// <param name="resource">The resource that contains the relationship.</param>
        /// <param name="id">The id of the resource.</param>
        /// <param name="relationship">The relationship this path refers to.</param>
        /// <param name="relatedResourceId">The id of the related resource.</param>
        /// <returns>A <see cref="string"/> containing the path.</returns>
        string BuildRelationshipPath(JsonApiResource resource, string id, ResourceRelationship relationship, string relatedResourceId);
    }
}
