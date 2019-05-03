using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Bitbird.Core.Api;
using Bitbird.Core.Api.Calls.Core;
using Bitbird.Core.Api.Models.Base;
using Bitbird.Core.Data;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.WebApi.JsonApi;
using Bitbird.Core.WebApi.Models;
using Bitbird.Core.WebApi.Resources;
using Newtonsoft.Json.Linq;

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
        /// <inheritdoc />
        public Func<string, bool> CanCreateRelation { get; }
        /// <inheritdoc />
        public Func<string, bool> CanUpdateRelation { get; }
        /// <inheritdoc />
        public Func<string, bool> CanDeleteRelation { get; }

        /// <summary>
        /// Creates a <see cref="CrudControllerBase{TService, TSession, TModel, TResource}"/> object.
        /// Enables deriving types to define which methods to support.
        ///
        /// Methods that are not supported will return a <see cref="HttpStatusCode.NotFound"/>(404), when the corresponding route is queried, but the route is still registered.
        /// </summary>
        /// <param name="canCreate">Whether to support the <see cref="CreateAsync"/> method.</param>
        /// <param name="canUpdate">Whether to support the <see cref="UpdateAsync"/> method.</param>
        /// <param name="canDelete">Whether to support the <see cref="DeleteAsync"/> method.</param>
        /// <param name="canCreateRelation">A predicate defining whether to support the <see cref="CreateRelationAsync"/> method for a given relation name.</param>
        /// <param name="canUpdateRelation">A predicate defining whether to support the <see cref="UpdateRelationAsync"/> method for a given relation name.</param>
        /// <param name="canDeleteRelation">A predicate defining whether to support the <see cref="DeleteRelationAsync"/> method for a given relation name.</param>
        protected CrudControllerBase(bool canCreate = true, bool canUpdate = true, bool canDelete = true, Func<string, bool> canCreateRelation = null, Func<string, bool> canUpdateRelation = null, Func<string, bool> canDeleteRelation = null)
        {
            CanCreate = canCreate;
            CanUpdate = canUpdate;
            CanDelete = canDelete;
            CanCreateRelation = canCreateRelation ?? (x => true);
            CanUpdateRelation = canUpdateRelation ?? (x => true);
            CanDeleteRelation = canDeleteRelation ?? (x => true);
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


        /// <summary>
        /// HTTP-verb: <c>POST</c>
        ///
        /// Route: <c>{controller}/{id:long}/relationships/{relation name}</c>
        /// 
        /// Expected HTTP-headers:
        /// - <c>Content-Type: application/vnd.api+json</c>
        /// - <c>Accept: application/vnd.api+json</c>
        /// - <c>X-ApiKey: {session token}</c>
        /// 
        /// Recommended HTTP-headers:
        /// - <c>Accept-Encoding: gzip,deflate</c>
        ///
        /// HTTP-body: A collection of <see cref="IdModel"/>(containing type and id) as JSON-Api.
        ///
        /// Query-parameters: empty
        ///
        /// This route is only supported if the defined relation is a to-many relation, and if the controller supports it.
        /// Most of the times it is only supported if the relation actually is a many-to-many relation.
        ///
        /// The JSON-Api resource that is used for deserialization is defined as <see cref="IdModelResource"/>, which will take a type and and id only.
        /// The JSON-Api resource that is used for serialization is defined as <see cref="IdModelResource"/>, which will return a type and and id only.
        ///
        /// Actually patches the defined relation to a union of the current related ids and the passed ids (i.e. if a relation already exists, it is not created).
        /// For information about patching a relation, see <see cref="UpdateRelationAsync"/> or the <c>PATCH {controller}/{id:long}/relationships/{relation name}</c> route.
        ///
        /// Returns the full collection of related ids for the defined relation.
        ///
        /// If the current session is not allowed to create the relation, an error is returned.
        ///
        /// If the current controller does not support creating this relation (see <see cref="CanCreateRelation"/>), this action returns <see cref="HttpStatusCode.NotFound"/>(404).
        /// </summary>
        /// <param name="id">Read from the route. The id of the primary record to update.</param>
        /// <param name="relationName">The name of the relation.</param>
        /// <param name="rawData">JSON-Api data containing a collection of <see cref="IdModel"/> objects as defined in <see cref="IdModelResource"/>.</param>
        /// <returns>The complete collection of related ids for the defined relation as collection of <see cref="IdModel"/> as defined in <see cref="IdModelResource"/>.</returns>
        [HttpPost, Route("{id:long}/relationships/{relationName}"), JsonApi(typeof(IdModelResource))]
        public async Task<JsonApiOverridePrimaryType> CreateRelationAsync(long id, string relationName, [FromBody] JToken rawData)
        {
            if (!CanCreateRelation(relationName))
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var relation = CrudControllerResourceMetaData.Instance.ForModel<TModel>()(relationName)
                           ?? throw new HttpResponseException(HttpStatusCode.NotFound);

            if (!relation.IsToMany)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var updateEntry = await GetAsync(id);

            var ids = ((IdModel[])relation.ReadIdModels(updateEntry)).Select(x => x.Id ?? throw new Exception("null not allowed")).ToArray(); // TODO: better error
            ids = ids.Union((long[])relation.IdModelsFromData(rawData["data"])).ToArray();
            relation.UpdateModel(updateEntry, ids);
            if (updateEntry is IOptimisticLockableModel optimisticLockableModel)
            {
                optimisticLockableModel.OptimisticLockingToken = rawData["meta"]["optimisticLockingToken"].Value<string>();
            }

            bool IdentifyRelation(string property) => string.Equals(property.ToLowerInvariant(), relation.IdPropertyName.ToLowerInvariant());

            var updatedEntity = await GetCrudServiceNode(Service).UpdateAsync(await GetSessionAsync(), updateEntry, IdentifyRelation);
            return new JsonApiOverridePrimaryType(relation.ReadIdModels(updatedEntity), relation.Type);
        }

        /// <summary>
        /// HTTP-verb: <c>PATCH</c>
        ///
        /// Route: <c>{controller}/{id:long}/relationships/{relation name}</c>
        /// 
        /// Expected HTTP-headers:
        /// - <c>Content-Type: application/vnd.api+json</c>
        /// - <c>Accept: application/vnd.api+json</c>
        /// - <c>X-ApiKey: {session token}</c>
        /// 
        /// Recommended HTTP-headers:
        /// - <c>Accept-Encoding: gzip,deflate</c>
        ///
        /// HTTP-body: A single object (can be null) or a collection of <see cref="IdModel"/>(containing type and id) as JSON-Api.
        ///
        /// Query-parameters: empty
        ///
        /// This route is only supported if the controller supports it.
        ///
        /// The JSON-Api resource that is used for deserialization is defined as <see cref="IdModelResource"/>, which will take a type and and id only.
        /// The JSON-Api resource that is used for serialization is defined as <see cref="IdModelResource"/>, which will return a type and and id only.
        ///
        /// If the defined relation is a to-many relation, this action expects a collection of <see cref="IdModel"/> records.
        /// The passed records will replace all existing related objects stored in the defined relation.
        ///
        /// If the defined relation is a belongs-to relation, this action expects a single object (can be null) of <see cref="IdModel"/>.
        /// The object can be null IF the relation is optional.
        /// The passed id defines the related object.
        ///
        /// Returns the a single object (can be null) or full collection of related ids for the defined relation.
        ///
        /// If the current session is not allowed to update the relation, an error is returned.
        ///
        /// If the current controller does not support update this relation (see <see cref="CanUpdateRelation"/>), this action returns <see cref="HttpStatusCode.NotFound"/>(404).
        /// </summary>
        /// <param name="id">Read from the route. The id of the primary record to update.</param>
        /// <param name="relationName">The name of the relation.</param>
        /// <param name="rawData">JSON-Api data containing a single object of or a collection of <see cref="IdModel"/> objects as defined in <see cref="IdModelResource"/>.</param>
        /// <returns>A single object (may be null) or a complete collection of related ids for the defined relation as collection of <see cref="IdModel"/> as defined in <see cref="IdModelResource"/>.</returns>
        [HttpPatch, Route("{id:long}/relationships/{relationName}"), JsonApi(typeof(IdModelResource))]
        public async Task<JsonApiOverridePrimaryType> UpdateRelationAsync(long id, string relationName, [FromBody] JToken rawData)
        {
            if (!CanUpdateRelation(relationName))
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var relation = CrudControllerResourceMetaData.Instance.ForModel<TModel>()(relationName)
                           ?? throw new HttpResponseException(HttpStatusCode.NotFound);
            
            var updateEntry = new TModel
            {
                Id = id
            };
            relation.UpdateModel(updateEntry, relation.IdModelsFromData(rawData["data"]));
            if (updateEntry is IOptimisticLockableModel optimisticLockableModel)
            {
                optimisticLockableModel.OptimisticLockingToken = rawData["meta"]["optimisticLockingToken"].Value<string>();
            }

            bool IdentifyRelation(string property) => string.Equals(property.ToLowerInvariant(), relation.IdPropertyName.ToLowerInvariant());

            var updatedEntity = await GetCrudServiceNode(Service).UpdateAsync(await GetSessionAsync(), updateEntry, IdentifyRelation);
            return new JsonApiOverridePrimaryType(relation.ReadIdModels(updatedEntity), relation.Type);
        }

        /// <summary>
        /// HTTP-verb: <c>DELETE</c>
        ///
        /// Route: <c>{controller}/{id:long}/relationships/{relation name}</c>
        /// 
        /// Expected HTTP-headers:
        /// - <c>Content-Type: application/vnd.api+json</c>
        /// - <c>Accept: application/vnd.api+json</c>
        /// - <c>X-ApiKey: {session token}</c>
        /// 
        /// Recommended HTTP-headers:
        /// - <c>Accept-Encoding: gzip,deflate</c>
        ///
        /// HTTP-body: A collection of <see cref="IdModel"/>(containing type and id) as JSON-Api.
        ///
        /// Query-parameters: empty
        ///
        /// This route is only supported if the defined relation is a to-many relation, and if the controller supports it.
        /// Most of the times it is only supported if the relation actually is a many-to-many relation.
        ///
        /// The JSON-Api resource that is used for deserialization is defined as <see cref="IdModelResource"/>, which will take a type and and id only.
        /// The JSON-Api resource that is used for serialization is defined as <see cref="IdModelResource"/>, which will return a type and and id only.
        ///
        /// Actually patches the defined relation to a difference of the current related ids and the passed ids (i.e. if a relation does not exist, it is not deleted).
        /// For information about patching a relation, see <see cref="UpdateRelationAsync"/> or the <c>PATCH {controller}/{id:long}/relationships/{relation name}</c> route.
        ///
        /// Returns the full collection of related ids for the defined relation.
        ///
        /// If the current session is not allowed to delete the relation, an error is returned.
        ///
        /// If the current controller does not support deleting this relation (see <see cref="CanDeleteRelation"/>), this action returns <see cref="HttpStatusCode.NotFound"/>(404).
        /// </summary>
        /// <param name="id">Read from the route. The id of the primary record to update.</param>
        /// <param name="relationName">The name of the relation.</param>
        /// <param name="rawData">JSON-Api data containing a collection of <see cref="IdModel"/> objects as defined in <see cref="IdModelResource"/>.</param>
        /// <returns>The complete collection of related ids for the defined relation as collection of <see cref="IdModel"/> as defined in <see cref="IdModelResource"/>.</returns>
        [HttpDelete, Route("{id:long}/relationships/{relationName}"), JsonApi(typeof(IdModelResource))]
        public async Task<JsonApiOverridePrimaryType> DeleteRelationAsync(long id, string relationName, [FromBody] JToken rawData)
        {
            if (!CanDeleteRelation(relationName))
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var relation = CrudControllerResourceMetaData.Instance.ForModel<TModel>()(relationName)
                           ?? throw new HttpResponseException(HttpStatusCode.NotFound);

            if (!relation.IsToMany)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var updateEntry = await GetAsync(id);

            var ids = ((IdModel[])relation.ReadIdModels(updateEntry)).Select(x => x.Id ?? throw new Exception("null not allowed")).ToArray(); // TODO: better error
            ids = ids.Except((long[]) relation.IdModelsFromData(rawData["data"])).ToArray();
            relation.UpdateModel(updateEntry, ids);
            if (updateEntry is IOptimisticLockableModel optimisticLockableModel)
            {
                optimisticLockableModel.OptimisticLockingToken = rawData["meta"]["optimisticLockingToken"].Value<string>();
            }

            bool IdentifyRelation(string property) => string.Equals(property.ToLowerInvariant(), relation.IdPropertyName.ToLowerInvariant());

            var updatedEntity = await GetCrudServiceNode(Service).UpdateAsync(await GetSessionAsync(), updateEntry, IdentifyRelation);
            return new JsonApiOverridePrimaryType(relation.ReadIdModels(updatedEntity), relation.Type);
        }
    }
}