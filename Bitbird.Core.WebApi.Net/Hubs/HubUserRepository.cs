using System;
using System.Linq;
using System.Threading.Tasks;
using Bitbird.Core.Data.Net.Cache;
using JetBrains.Annotations;

namespace Bitbird.Core.WebApi.Net.Hubs
{
    /// <summary>
    /// A repository that manages sessions, connections and groups.
    /// Uses the cache for storage.
    /// 
    /// Internally uses the following store concept:
    /// A store is a collection of entries.
    /// It is automatically deleted when the last entry is removed and automatically created when the first entry is added.
    /// Deleting a store removes all entries from it.
    /// If entries of different types are added to a store, they are stored separately and do not interact.
    /// </summary>
    public static class HubUserRepository
    {
        [NotNull] private const string Prefix = "signalR";
        [NotNull] private const string PrefixSession = Prefix + ":Sessions";
        [NotNull] private const string PrefixConnection = Prefix + ":Connections";
        [NotNull] private const string PrefixGroups = Prefix + ":Groups";

        [Obsolete("Use property.")]
        [CanBeNull]
        private static Redis redis;
        [NotNull]
#pragma warning disable 618 // ignore obsolete warning here, because the field is intended to be accessed from here.
        private static Redis Redis => redis ?? throw new Exception($"{nameof(HubUserRepository)} was used before initialization.");
#pragma warning restore 618

        [ContractAnnotation("redis:null => halt")]
        // ReSharper disable once ParameterHidesMember
        public static void Init([CanBeNull] Redis redis)
        {
#pragma warning disable 618 // ignore obsolete warning here, because the field is intended to be accessed from here.
            HubUserRepository.redis = redis ?? throw new ArgumentNullException(nameof(redis));
#pragma warning restore 618
        }

        /// <summary>
        /// Direct cache access.
        /// Returns all connections ids from a given session id store.
        /// If there is no session id store or it is empty, an empty collection is returned.
        /// </summary>
        /// <param name="sessionId">Identifies the store.</param>
        /// <returns>All connection ids in this session id store.</returns>
        [NotNull, ItemNotNull]
        private static Task<string[]> GetConnectionIdsBySessionAsync(long sessionId)
        {
            return Redis.LowLevelSetGetAsync<string, long>(PrefixSession, sessionId);
        }

        /// <summary>
        /// Direct cache access.
        /// Adds a connection id to a session id store.
        /// Not vice-versa.
        /// Has no effect if the connection id is contained in the session id store.
        /// </summary>
        /// <param name="sessionId">Identifies the store from which to remove.</param>
        /// <param name="connectionId">The connection id that should be added.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        private static Task AddConnectionIdToSessionAsync(long sessionId, [NotNull] string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));

            return Redis.LowLevelSetAddAsync(PrefixSession, sessionId, connectionId);
        }

        /// <summary>
        /// Direct cache access.
        /// Removes a connection id from a session id store.
        /// Not vice-versa.
        /// Has no effect if the connection id is not contained in the session id store.
        /// </summary>
        /// <param name="sessionId">Identifies the store from which to remove.</param>
        /// <param name="connectionId">The connection id that should be removed.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        private static Task RemoveConnectionIdFromSessionAsync(long sessionId, [NotNull] string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));

            return Redis.LowLevelSetRemoveAsync(PrefixSession, sessionId, connectionId);
        }

        /// <summary>
        /// Direct cache access.
        /// Removes a session id store from the cache.
        /// Has no effect if no store exists for the session id.
        /// </summary>
        /// <param name="sessionId">Identifies the store which to remove.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        private static Task DeleteSessionAsync(long sessionId)
        {
            return Redis.LowLevelSetClearAsync(PrefixSession, sessionId);
        }

        /// <summary>
        /// Direct cache access.
        /// Gets the session id in the connection id store.
        /// If there is no connection id store or it is empty, null is returned.
        /// </summary>
        /// <param name="connectionId">Identifies the store which to query.</param>
        /// <returns>The session id in the store, or null.</returns>
        [NotNull, ItemCanBeNull]
        private static async Task<long?> GetSessionByConnectionAsync([NotNull] string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));

            return (await Redis.LowLevelSetGetAsync<long, string>(PrefixConnection, $"{connectionId}:Session"))
                .Select(x => (long?)x)
                .SingleOrDefault();
        }

        /// <summary>
        /// Direct cache access.
        /// Adds the session id in the connection id store.
        /// Has no effect if the session id is contained in the connection id store.
        /// </summary>
        /// <param name="connectionId">Identifies the store to which to add.</param>
        /// <param name="sessionId">The session id to add.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        private static Task SetSessionByConnectionAsync([NotNull] string connectionId, long sessionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));

            return Redis.LowLevelSetAddAsync(PrefixConnection, $"{connectionId}:Session", sessionId);
        }
        /// <summary>
        /// Direct cache access.
        /// Removes a connection id store from the cache.
        /// Has no effect if no store exists for the connection id.
        /// </summary>
        /// <param name="connectionId">Identifies the store which to remove.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        private static Task DeleteConnectionAsync([NotNull] string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));

            return Task.WhenAll(
                Redis.LowLevelSetClearAsync(PrefixConnection, $"{connectionId}:Session"),
                Redis.LowLevelSetClearAsync(PrefixConnection, $"{connectionId}:Groups"));
        }

        /// <summary>
        /// Direct cache access.
        /// Adds the group name to a connection id store.
        /// Adds the connection id to the group store.
        /// No entry is added twice.
        /// </summary>
        /// <param name="connectionId">The connection id.</param>
        /// <param name="group">The group name.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        private static Task AddConnectionToGroupAsync(
            [NotNull] string connectionId, 
            [NotNull] string group)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));
            if (string.IsNullOrWhiteSpace(group))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(group));

            return Task.WhenAll(
                Redis.LowLevelSetAddAsync(PrefixConnection, $"{connectionId}:Groups", group),
                Redis.LowLevelSetAddAsync(PrefixGroups, group, connectionId));
        }

        /// <summary>
        /// Direct cache access.
        /// Removes the group name from a connection id store.
        /// Removes the connection id from a group store.
        /// No error is thrown if either entry is not existent.
        /// </summary>
        /// <param name="connectionId">The connection id.</param>
        /// <param name="group">The group name.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        private static Task RemoveConnectionFromGroupAsync(
            [NotNull] string connectionId,
            [NotNull] string group)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));
            if (string.IsNullOrWhiteSpace(group))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(group));

            return Task.WhenAll(
                Redis.LowLevelSetRemoveAsync(PrefixConnection, $"{connectionId}:Groups", group),
                Redis.LowLevelSetRemoveAsync(PrefixGroups, group, connectionId));
        }

        /// <summary>
        /// Direct cache access.
        /// Gets all group names in a connection id store.
        /// Returns an empty collection if the connection id store does not exist.
        /// </summary>
        /// <param name="connectionId">Identifies the store to query.</param>
        /// <returns>A collection of group names.</returns>
        [NotNull, ItemNotNull]
        private static Task<string[]> GetGroupsByConnectionAsync([NotNull] string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));

            return Redis.LowLevelSetGetAsync<string, string>(PrefixConnection, $"{connectionId}:Groups");
        }

        /// <summary>
        /// Direct cache access.
        /// Gets all connection ids in a group store.
        /// Returns an empty collection if the connection id store does not exist.
        /// </summary>
        /// <param name="group">Identifies the store to query.</param>
        /// <returns>A collection of connection ids.</returns>
        [NotNull, ItemNotNull]
        private static Task<string[]> GetConnectionsByGroupAsync([NotNull] string group)
        {
            if (string.IsNullOrWhiteSpace(group))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(group));

            return Redis.LowLevelSetGetAsync<string, string>(PrefixGroups, group);
        }

        /// <summary>
        /// Adds a connection id to a group.
        /// </summary>
        /// <param name="connectionId">The connection id.</param>
        /// <param name="group">The group name.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        public static Task AddToGroupAsync([NotNull] string connectionId, [NotNull] string group)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));
            if (string.IsNullOrWhiteSpace(group))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(group));

            return AddConnectionToGroupAsync(connectionId, group);
        }

        /// <summary>
        /// Removes a connection id from a group.
        /// </summary>
        /// <param name="connectionId">The connection id.</param>
        /// <param name="group">The group name.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        public static Task RemoveFromGroupAsync([NotNull] string connectionId, [NotNull] string group)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));
            if (string.IsNullOrWhiteSpace(group))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(group));

            return RemoveConnectionFromGroupAsync(connectionId, group);
        }

        [NotNull, ItemNotNull]
        public static Task<string[]> GetGroupMembersAsync([NotNull] string group)
        {
            if (string.IsNullOrWhiteSpace(group))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(group));

            return GetConnectionsByGroupAsync(group);
        }

        [NotNull]
        public static Task InvalidateSessionAsync(long sessionId)
        {
            return Task.Run(async () =>
                {
                    var connectionIds = await GetConnectionIdsBySessionAsync(sessionId);

                    await Task.WhenAll(connectionIds
                        .Select(async connectionId =>
                        {
                            var groups = await GetGroupsByConnectionAsync(connectionId);

                            await Task.WhenAll(groups
                                .Select(group => RemoveConnectionFromGroupAsync(connectionId, group))
                                .Concat(new []
                                {
                                    DeleteConnectionAsync(connectionId)
                                })
                                .ToArray());
                        })
                        .Concat(new []
                        {
                            DeleteSessionAsync(sessionId)
                        })
                        .ToArray());
                });
        }
        /// <summary>
        /// Checks whether a session id is already associated with a connection.
        /// If so the connection is considered logged in.
        /// </summary>
        /// <param name="connectionId">The connection id.</param>
        /// <returns>True if a session id was found, else false.</returns>
        [NotNull]
        public static async Task<bool> IsLoggedInAsync([NotNull] string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));

            return (await GetSessionByConnectionAsync(connectionId)).HasValue;
        }
        /// <summary>
        /// Adds a connection to a session.
        /// No checks are performed whether the connection already has a session or not.
        /// </summary>
        /// <param name="connectionId">The connection' id.</param>
        /// <param name="sessionId">The session's id.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        public static Task LoginAsync([NotNull] string connectionId, long sessionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));

            return Task.WhenAll(
                SetSessionByConnectionAsync(connectionId, sessionId),
                AddConnectionIdToSessionAsync(sessionId, connectionId));
        }
        /// <summary>
        /// Removes a connection from a session.
        /// No checks are performed whether the session already belongs to the connection or not.
        /// Removes the connection from all groups.
        /// Removes all information about the connection.
        /// </summary>
        /// <param name="connectionId">The connection' id.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        public static Task LogoutAsync([NotNull] string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionId));

            return Task.WhenAll(
                Task.Run(async () =>
                {
                    var groups = await GetGroupsByConnectionAsync(connectionId);
                    await Task.WhenAll(groups.Select(group => RemoveConnectionFromGroupAsync(connectionId, group)).ToArray());
                }),
                Task.Run(async () =>
                {
                    var sessionId = await GetSessionByConnectionAsync(connectionId);
                    if (sessionId != null)
                        await RemoveConnectionIdFromSessionAsync(sessionId.Value, connectionId);
                }),
                DeleteConnectionAsync(connectionId)
            );
        }
    }
}