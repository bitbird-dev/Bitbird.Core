namespace Bitbird.Core.Data
{
    /// <summary>
    /// An object that supports the use of a soft-delete-flag.
    /// If this flag true, this entity was deleted (most likely by a user), but is still kept in the database in case some other entity maintains a link to it.
    /// Soft-deleted entries are usually not returned by range-queries but only by queries-by-id.
    /// </summary>
    public interface IIsDeletedFlagEntity
    {
        /// <summary>
        /// Soft-delete-flag.
        /// If this is true, this entity was deleted (most likely by a user), but is still kept in the database in case some other entity maintains a link to it.
        /// Soft-deleted entries are usually not returned by range-queries but only by queries-by-id.
        /// </summary>
        bool IsDeleted { get; set; }
    }
}