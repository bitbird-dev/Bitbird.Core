namespace Bitbird.Core.Api.Net.EntityChanges
{
    /// <summary>
    /// Defines the type of change to an entity.
    /// </summary>
    public enum EntityChangeType : byte
    {
        /// <summary>
        /// The given entity was created.
        /// </summary>
        Created = 1,

        /// <summary>
        /// The given entity was updated. 
        /// </summary>
        Updated = 2,

        /// <summary>
        /// The given entity was deleted.
        /// </summary>
        Deleted = 4
    }
}