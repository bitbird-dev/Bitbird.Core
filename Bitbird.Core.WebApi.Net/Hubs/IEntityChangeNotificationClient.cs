using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.WebApi.Hubs
{
    /// <summary>
    /// Defines an interface for clients that connect to a change notification hub.
    /// All declared methods can be called by the server and therefore should be implemented by clients.
    /// </summary>
    public interface IEntityChangeNotificationClient<in TEntityChangeModel>
        where TEntityChangeModel : class
    {
        /// <summary>
        /// The server sends (and the client receives) changes to entities.
        /// Is an async method on the server-side, but for naming convention reasons on the client side, the postfix "Async" is omitted.
        /// </summary>
        /// <param name="changesModel">The changes that occurred.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        Task ReceiveChanges([NotNull, ItemNotNull] TEntityChangeModel[] changesModel);


        /// <summary>
        /// The server calls this method, when the passed query string contains an 'interfaceVersion'-entry that does not match the server's interface version.
        /// </summary>
        /// <param name="serverVersion">The server's interface version.</param>
        /// <param name="clientVersion">The passed version in the query string.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        Task NotifyWrongVersion(long serverVersion, long clientVersion);
    }
}