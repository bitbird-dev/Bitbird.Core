using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bitbird.Core.Data;
using Bitbird.Core.Data.DbContext;
using Bitbird.Core.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Models.Base
{
    /// <summary>
    /// The base type for api models (i.e. api models MUST derive from this type).
    /// Assists in:
    /// - permission checking
    /// - delayed loading of relations
    /// - conditional update of fields and relations
    /// </summary>
    public abstract class ModelBase<TService, TSession, TDbContext, TState, TEntityChangeModel, TEntityTypeId, TId>
        where TService : class, IApiService<TDbContext, TState, TSession, TEntityChangeModel, TEntityTypeId, TId>
        where TSession : class, IApiSession
        where TDbContext : class, IDisposable, IHookedStateDataContext<TState>
        where TState : IBaseUnitOfWork<TEntityChangeModel, TEntityTypeId, TId>
        where TEntityChangeModel : class
    {
        /// <summary>
        /// Stores queried values by property name.
        /// </summary>
        [NotNull] private readonly Dictionary<string, object> loadedQueryValueCache = new Dictionary<string, object>();
        /// <summary>
        /// Stores the attached data service.
        /// If this is null, <see cref="session"/> is also null.
        /// Can be null.
        /// </summary>
        [CanBeNull] private TService service;
        /// <summary>
        /// Stores the attached permission service.
        /// If this is null, <see cref="service"/> is also null.
        /// Can be null.
        /// </summary>
        [CanBeNull] private TSession session;

        /// <summary>
        /// Attaches a data service and a permission resolver to the model.
        /// 
        /// The service is used to query relations. 
        /// The session is used for permission checks during delayed querying.
        /// 
        /// Thread safe.
        /// </summary>
        /// <param name="service">The api service to attach.</param>
        /// <param name="session">The session to attach.</param>
        /// <param name="recursive">True if the passed objects should also be attached to all queried, related <see cref="ModelBase{TService, TSession, TDbContext, TState, TEntityChangeModel, TEntityTypeId, TId}"/> instances.</param>
        public void AttachService(
            // ReSharper disable once ParameterHidesMember
            [NotNull] TService service,
            // ReSharper disable once ParameterHidesMember
            [NotNull] TSession session, 
            bool recursive = true)
        {
            this.service = service;
            this.session = session;

            if (!recursive)
                return;

            lock (loadedQueryValueCache)
            {
                foreach (var loadedValue in loadedQueryValueCache.Values)
                    if (loadedValue is ModelBase<TService, TSession, TDbContext, TState, TEntityChangeModel, TEntityTypeId, TId> modelBase)
                        modelBase.AttachService(service, session);
            }
        }

        /// <summary>
        /// Detaches the current api service and the current session from the model.
        /// If no api service or session were attached, nothing happens.
        /// 
        /// After detaching, delayed relation query is disabled.
        /// 
        /// Thread safe.
        /// </summary>
        /// <param name="recursive">True if all queried, related <see cref="ModelBase{TService, TSession, TDbContext, TState, TEntityChangeModel, TEntityTypeId, TId}"/> instances should also be detached.</param>
        internal void DetachService(bool recursive = true)
        {
            service = null;
            session = null;

            if (!recursive)
                return;

            lock (loadedQueryValueCache)
            {
                foreach (var loadedValue in loadedQueryValueCache.Values)
                    if (loadedValue is ModelBase<TService, TSession, TDbContext, TState, TEntityChangeModel, TEntityTypeId, TId> modelBase)
                        modelBase.DetachService();
            }
        }

        /// <summary>
        /// Tries to get a queried value from <see cref="loadedQueryValueCache"/>.
        /// Supports retrieving <c>null</c>.
        ///
        /// If no value is found, <c>result</c> is set to null.
        /// 
        /// Thread safe.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="propertyName">The name of the property that queried the value.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if a value was found.</returns>
        private bool TryGetLoadedQueryValuePropertyFromCache<T>(
            [NotNull] string propertyName, 
            [CanBeNull] out T result)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(propertyName));

            lock (loadedQueryValueCache)
            {
                if (loadedQueryValueCache.TryGetValue(propertyName ?? throw new ArgumentNullException(nameof(propertyName)), out var data))
                {
                    switch (data)
                    {
                        case null:
                            result = null;
                            return true;
                        case T typedData:
                            result = typedData;
                            return true;
                        default:
                            throw new InvalidCastException($"The queried object is stored as {data.GetType().FullName} but requested as {typeof(T).FullName}");
                    }
                }
            }

            result = null;
            return false;
        }
        /// <summary>
        /// Tries to retrieve a value from <see cref="loadedQueryValueCache"/>.
        /// If the value does not exists, the passed value is stored and returned.
        /// Supports storing/retrieving <c>null</c>.
        /// 
        /// Thread safe.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="propertyName">The name of the property that queried the value.</param>
        /// <param name="newObject">The new value, if no value was found.</param>
        /// <returns>The stored value, or, if no value was found, the passed value.</returns>
        [CanBeNull] 
        private T LoadOrStoreQueryValuePropertyFromToCache<T>(
            [NotNull] string propertyName, 
            [CanBeNull] T newObject)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(propertyName));

            lock (loadedQueryValueCache)
            {
                if (TryGetLoadedQueryValuePropertyFromCache<T>(propertyName, out var result))
                    return result;

                loadedQueryValueCache.Add(propertyName, newObject);
            }

            return newObject;
        }
        /// <summary>
        /// Queries data.
        /// If the data was already queried successfully once, it is not queried again but loaded from a cache.
        /// 
        /// Thread safe.
        /// </summary>
        /// <typeparam name="T">The type of data.</typeparam>
        /// <param name="dataQuery">The data-query.</param>
        /// <param name="propertyName">The name of the property that queried the value.</param>
        /// <returns>The queried data.</returns>
        [NotNull, ItemCanBeNull]
        private async Task<T> QueryAsync<T>(
            [NotNull] Func<TService, TDbContext, TSession, Task<T>> dataQuery, 
            [CallerMemberName] string propertyName = null)
            where T : class
        {
            if (dataQuery == null)
                throw new ArgumentNullException(nameof(dataQuery));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(propertyName));

            if (TryGetLoadedQueryValuePropertyFromCache<T>(propertyName, out var result))
                return result;

            if (service == null || session == null)
                return default;

            using (var db = service.CreateDbContext(session))
            {
                var getterTask = dataQuery(service, db, session);
                result = await getterTask;
            }

            return LoadOrStoreQueryValuePropertyFromToCache(propertyName, result);
        }
        /// <summary>
        /// Stores queried data (if not stored before).
        /// If the data was already queried successfully once, it is not stored.
        /// 
        /// Thread safe.
        /// </summary>
        /// <typeparam name="T">The type of data.</typeparam>
        /// <param name="data">The queried data.</param>
        /// <param name="propertyName">The name of the property that queried the value.</param>
        /// <returns>Nothing.</returns>
        protected void SetQueried<T>(
            [CanBeNull] T data, 
            [CallerMemberName] string propertyName = null)
            where T : class
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(propertyName));

            LoadOrStoreQueryValuePropertyFromToCache(propertyName, data);
        }
        /// <summary>
        /// Queries data.
        /// If the data was already queried successfully once, it is not queried again but loaded from a cache.
        /// 
        /// Thread safe.
        /// </summary>
        /// <typeparam name="T">The type of data.</typeparam>
        /// <param name="dataQuery">The data-query.</param>
        /// <param name="propertyName">The name of the property that queried the value.</param>
        /// <returns>The queried data.</returns>
        [NotNull, ItemCanBeNull]
        protected T[] Query<T>(
            [NotNull] Func<TService, TDbContext, TSession, Task<T[]>> dataQuery,
            [CallerMemberName] string propertyName = null)
            where T : class
        {
            if (dataQuery == null)
                throw new ArgumentNullException(nameof(dataQuery));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(propertyName));

            return AsyncHelper.RunSync(() => QueryAsync(dataQuery, propertyName));
        }
        /// <summary>
        /// Queries data.
        /// If the data was already queried successfully once, it is not queried again but loaded from a cache.
        /// 
        /// Thread safe.
        /// </summary>
        /// <typeparam name="T">The type of data.</typeparam>
        /// <param name="dataQuery">The data-query.</param>
        /// <param name="propertyName">The name of the property that queried the value.</param>
        /// <returns>The queried data.</returns>
        [NotNull]
        protected T Query<T>(
            [NotNull] Func<TService, TDbContext, TSession, Task<T>> dataQuery, 
            [CallerMemberName] string propertyName = null)
            where T : class
        {
            if (dataQuery == null)
                throw new ArgumentNullException(nameof(dataQuery));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(propertyName));

            return AsyncHelper.RunSync(() => QueryAsync(dataQuery, propertyName));
        }

        /// <summary>
        /// Gets the value of a field.
        /// If the attached <see cref="IPermissionResolver"/> does not have the permissions to do so, the default value for the field is returned.
        /// Needed permission: <see cref="AccessType.Read"/>.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">A reference of the field to read.</param>
        /// <param name="propertyName">The name of the property. Used for permission checks.</param>
        /// <returns>The value of the field, if succeeded. The default value for the type otherwise.</returns>
        [CanBeNull]
        protected T Get<T>(
            [CanBeNull] ref T field, 
            [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(propertyName));

            return field;
        }
        /// <summary>
        /// Sets a field to a value.
        /// If the attached <see cref="IPermissionResolver"/> does not have the permissions to do so, nothing happens and false is returned.
        /// Needed permission: <see cref="AccessType.Update"/>.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">A reference of the field to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="propertyName">The name of the property. Used for permission checks.</param>
        protected void Set<T>(
            [CanBeNull] ref T field,
            [CanBeNull] T value, 
            [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(propertyName));

            field = value;
        }
    }
}