using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Bitbird.Core.Api;
using Bitbird.Core.Api.Nodes.Core;
using Bitbird.Core.Data;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.WebApi.JsonApi;

namespace Bitbird.Core.WebApi.Controllers
{
    /// <summary>
    /// Provides the following methods in addition to methods provided by <see cref="ReadControllerBase{TService,TSession,TModel,TResource}" />: <see cref="CreateAsync" />, <see cref="UpdateAsync" /> and <see cref="DeleteAsync" />.
    /// Can be used for standard entity-access.
    /// </summary>
    public abstract class CrudControllerBase<TService, TSession, TModel, TResource> 
        : ReadControllerBase<TService, TSession, TModel, TResource>
        , ICrudControllerBase
        where TService : class, IApiServiceSessionCreator<TSession>
        where TSession : class, IApiSession
        where TModel : class, IIdSetter<long>, new()
        where TResource : JsonApiResource, new()
    {
        /// <summary>The <see cref="JsonApiResource"/> that describes data which will be returned from the Create-method. Returns an instance of TResource.</summary>
        private static readonly JsonApiResource JsonApiResourceCreate = new TResource();
        /// <summary>The <see cref="JsonApiResource"/> that describes data which will be returned from the Update-method. Returns an instance of TResource.</summary>
        private static readonly JsonApiResource JsonApiResourceUpdate = new TResource();

        /// <inheritdoc />
        public bool CanCreate { get; }
        /// <inheritdoc />
        public bool CanUpdate { get; }
        /// <inheritdoc />
        public bool CanDelete { get; }

        /// <summary>
        /// Creates a <see cref="CrudControllerBase{TService, TSession, TModel, TResource}"/> object.
        /// Enables deriving types to define which methods to support.
        ///
        /// Methods that are not supported will return a <see cref="HttpStatusCode.NotFound"/>(404), when the corresponding route is queried, but the route is still registered.
        /// </summary>
        /// <param name="canCreate">Whether to support the <see cref="CreateAsync"/> method.</param>
        /// <param name="canUpdate">Whether to support the <see cref="UpdateAsync"/> method.</param>
        /// <param name="canDelete">Whether to support the <see cref="DeleteAsync"/> method.</param>
        protected CrudControllerBase(bool canCreate = true, bool canUpdate = true, bool canDelete = true)
        {
            CanCreate = canCreate;
            CanUpdate = canUpdate;
            CanDelete = canDelete;
        }


        /// <inheritdoc />
        // ReSharper disable once ParameterHidesMember
        protected override void SetApiResourceModelsCore(Dictionary<string, JsonApiResource> apiResourceModelsById)
        {
            base.SetApiResourceModelsCore(apiResourceModelsById);

            apiResourceModelsById.Add(nameof(JsonApiResourceCreate), JsonApiResourceCreate);
            apiResourceModelsById.Add(nameof(JsonApiResourceUpdate), JsonApiResourceUpdate);
        }


        /// <inheritdoc />
        protected override IServiceReadNode<TSession, TModel, long> GetReadServiceNode(TService service) => GetCrudServiceNode(service);

        /// <summary>
        /// Returns the service-node on which this CRUD-controller is operating.
        /// </summary>
        /// <param name="service">The service on which to operate. Not null.</param>
        /// <returns>A <see cref="IServiceCrudNode{TSession,TModel,TId}"/> on which to operate.</returns>
        protected abstract IServiceCrudNode<TSession, TModel, long> GetCrudServiceNode(TService service);


        /// <summary>
        /// HTTP-verb: <c>POST</c>
        ///
        /// Route: <c>{controller}</c>
        /// 
        /// Expected HTTP-headers:
        /// - <c>Content-Type: application/vnd.api+json</c>
        /// - <c>Accept: application/vnd.api+json</c>
        /// - <c>X-ApiKey: {session token}</c>
        /// 
        /// Recommended HTTP-headers:
        /// - <c>Accept-Encoding: gzip,deflate</c>
        ///
        /// HTTP-body: A record of the entity specified by the TModel type parameter as JSON-Api.
        ///
        /// Query-parameters: empty
        ///
        /// Creates a new record of the entity specified by the TModel type parameter.
        /// The JSON-Api resource that is used for deserialization is defined as the default deserialization resource for the TModel type (defined by the <see cref="JsonApiResourceMappingAttribute"/> of a JSON-Api-resource).
        /// The JSON-Api resource that is used for serialization is defined by <see cref="JsonApiResourceCreate"/>, which will return an instance of the TResource parameter.
        ///
        /// Calls the corresponding api service node to create the given record and return it, if the current session is permitted to create.
        /// Not all attributes/relations of the passed record must be set.
        /// A detailed documentation of which properties are needed to be set can be found in the TModel class documentation (some properties are not tracked during creation, and therefore must not be provided).
        /// 
        /// Returns the created record.
        ///
        /// If the current session is not allowed to create the record, an error is returned.
        ///
        /// If the current controller does not support creating (see <see cref="CanCreate"/>), this action returns <see cref="HttpStatusCode.NotFound"/>(404).
        /// </summary>
        /// <param name="model">The record to create. Must not be null.</param>
        /// <returns>The created record.</returns>
        [HttpPost, Route(""), JsonApi(nameof(JsonApiResourceCreate))]
        public async Task<TModel> CreateAsync([FromBody] TModel model)
        {
            if (!CanCreate)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            if (model == null)
                throw new ApiErrorException(new ApiParameterError(nameof(model), "The passed model is null."));

            return await GetCrudServiceNode(Service).CreateAsync(await GetSessionAsync(), model);
        }

        /// <summary>
        /// HTTP-verb: <c>PATCH</c>
        ///
        /// Route: <c>{controller}/{id:long}</c>
        /// 
        /// Expected HTTP-headers:
        /// - <c>Content-Type: application/vnd.api+json</c>
        /// - <c>Accept: application/vnd.api+json</c>
        /// - <c>X-ApiKey: {session token}</c>
        /// 
        /// Recommended HTTP-headers:
        /// - <c>Accept-Encoding: gzip,deflate</c>
        ///
        /// HTTP-body: A partial record of the entity specified by the TModel type parameter as JSON-Api.
        ///
        /// Query-parameters: empty
        ///
        /// Updates an existing record of the entity specified by the TModel type parameter.
        /// The JSON-Api resource that is used for deserialization is defined as the default deserialization resource for the TModel type (defined by the <see cref="JsonApiResourceMappingAttribute"/> of a JSON-Api-resource).
        /// The JSON-Api resource that is used for serialization is defined by <see cref="JsonApiResourceCreate"/>, which will return an instance of the TResource parameter.
        ///
        /// Calls the corresponding api service node to update the given record and return it, if the current session is permitted to update.
        /// Not all attributes/relations of the passed record must be set.
        /// Only set attributes are taken into account, and among these, some may be ignored.
        /// A detailed documentation of which properties are not ignored can be found in the TModel class documentation (some properties are not tracked during update, and therefore must not be provided).
        /// 
        /// Returns the updated record.
        ///
        /// If the current session is not allowed to update the record, an error is returned.
        ///
        /// If the current controller does not support updating (see <see cref="CanUpdate"/>), this action returns <see cref="HttpStatusCode.NotFound"/>(404).
        /// </summary>
        /// <param name="id">Read from the route. The id of the record to update.</param>
        /// <param name="model">The partial record to update. Must not be null. Must have the id that the route suggests. Does not have to have all attributes/relations set. See summary.</param>
        /// <returns>The updated record.</returns>
        [HttpPatch, Route("{id:long}"), JsonApi(nameof(JsonApiResourceUpdate))]
        public async Task<TModel> UpdateAsync(long id, [FromBody] ContentInfo<TModel> model)
        {
            if (!CanUpdate)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            if (model.Data.Id != id)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            return await GetCrudServiceNode(Service).UpdateAsync(await GetSessionAsync(), model.Data, model.FoundAttributes);
        }

        /// <summary>
        /// HTTP-verb: <c>DELETE</c>
        ///
        /// Route: <c>{controller}/{id:long}</c>
        /// 
        /// Expected HTTP-headers:
        /// - <c>X-ApiKey: {session token}</c>
        /// 
        /// Recommended HTTP-headers:
        /// - <c>Content-Type: application/vnd.api+json</c>
        ///
        /// HTTP-body: empty
        ///
        /// Query-parameters: empty
        ///
        /// Deletes an existing record of the entity specified by the TModel type parameter.
        ///
        /// Calls the corresponding api service node to delete the given record and return it, if the current session is permitted to delete.
        /// If the entity type supports soft-delete (see <see cref="IIsDeletedFlagEntity"/>), the record is only marked as deleted.
        /// It will be returned by queries that directly access it by id, but not by range/cache queries.
        /// 
        /// Returns an empty body.
        ///
        /// If the current session is not allowed to delete the record, an error is returned.
        ///
        /// If the current controller does not support deleting (see <see cref="CanDelete"/>), this action returns <see cref="HttpStatusCode.NotFound"/>(404).
        /// </summary>
        /// <param name="id">Read from the route. The id of the record to delete.</param>
        /// <returns>Nothing.</returns>
        [HttpDelete, Route("{id:long}")]
        public async Task DeleteAsync(long id)
        {
            if (!CanDelete)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            await GetCrudServiceNode(Service).DeleteAsync(await GetSessionAsync(), id);
        }
    }
}