namespace Bitbird.Core.Data.Net
{
    /// <summary>
    /// An object that indicates it's "active"-state.
    /// Non-active objects are usually indicated to the user in a special way (e.g. grayed out or hidden).
    /// </summary>
    public interface IIsActiveFlagEntity
    {
        /// <summary>
        /// The "active"-state of the object.
        /// Non-active objects are usually indicated to the user in a special way (e.g. grayed out or hidden).
        /// </summary>
        bool IsActive { get; set; }
    }
}