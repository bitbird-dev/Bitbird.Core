using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.ApiResource
{
    /// <summary>
    /// Represents the different kinds of relationships between resources.
    /// </summary>
    public enum RelationshipKind
    {
        /// <summary>
        /// A to-one relationship between resources.
        /// </summary>
        BelongsTo,

        /// <summary>
        /// A to-many relationship between resources.
        /// </summary>
        HasMany
    }
}
