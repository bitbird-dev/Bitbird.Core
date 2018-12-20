using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.ApiResource
{
    /// <summary>
    /// Describes one or more link types to be generated.
    /// Source: https://github.com/joukevandermaas/saule/
    /// </summary>
    [Flags]
    public enum LinkType
    {
        /// <summary>
        /// No links
        /// </summary>
        None = 0,

        /// <summary>
        /// Only self links
        /// </summary>
        Self = 1,

        /// <summary>
        /// Only related links
        /// </summary>
        Related = 2,

        /// <summary>
        /// Only self links in the top section
        /// </summary>
        TopSelf = 4,

        /// <summary>
        /// Generate all possible links
        /// </summary>
        All = ~None
    }
}
