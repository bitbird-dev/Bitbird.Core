using JetBrains.Annotations;

namespace Bitbird.Core.WebApi.Net.Models
{
    /// <summary>
    /// A model used to store ids.
    /// Used in various places, but mainly to update/query/delete relations during JsonApi-requests.
    /// </summary>
    public class IdModel
    {
        /// <summary>
        /// The id that should be transmitted. Can be null.
        /// </summary>
        [CanBeNull, UsedImplicitly]
        public long? Id { get; set; }
    }
}