using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Bitbird.Core.Api;
using Bitbird.Core.Api.Calls.Core;
using Bitbird.Core.Data.Query;
using Bitbird.Core.Export.Xlsx;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Query;
using Bitbird.Core.WebApi.Extensions;
using Bitbird.Core.WebApi.JsonApi;
using Bitbird.Core.WebApi.Resources;

namespace Bitbird.Core.WebApi.Controllers
{
    /// <summary>
    /// Provides the following methods: <see cref="GetAsync"/>, <see cref="GetManyAsync"/>, <see cref="ExportToXlsxAsync"/>.
    /// Can be used for standard entity-access.
    /// </summary>
    public abstract class ReadControllerBase<TService, TSession, TModel, TResource> 
        : ControllerBase<TService, TSession>
        , IReadControllerBase
        where TService : class, IApiServiceSessionCreator<TSession>
        where TSession : class, IApiSession
        where TModel : class, IId<long>, new()
        where TResource : JsonApiResource, new()
    {
        /// <summary>
        /// Returns the Type of the passed TModel type parameter.
        /// </summary>
        public Type ModelType => typeof(TModel);
        /// <summary>
        /// Returns the Type of the passed TResource type parameter.
        /// </summary>
        public Type ResourceType => typeof(TResource);

        /// <summary>The <see cref="JsonApiResource"/> that describes data which will be returned from the GetMany-method. Returns an instance of TResource.</summary>
        private static readonly JsonApiResource JsonApiResourceGetMany = new TResource();
        /// <summary>The <see cref="JsonApiResource"/> that describes data which will be returned from the Get-method. Returns an instance of TResource.</summary>
        private static readonly JsonApiResource JsonApiResourceGet = new TResource();

        /// <summary>
        /// Stores JsonApiResources by a given id.
        /// These are used through the implementation of <see cref="IJsonApiResourceController"/>.
        /// <see cref="GetJsonApiResourceById"/> makes a lookup into this dictionary and returns null if the key is not found.
        /// 
        /// This pattern is needed because the <see cref="JsonApiAttribute"/> needs to know the <see cref="JsonApiResource"/> that a method returns.
        /// Since generic types cannot be passed to attributes, we pass an id (see nameof(...) in the definition of the <see cref="JsonApiAttribute"/>) that will then be
        /// resolved using the implementation of <see cref="IJsonApiResourceController"/>.
        ///
        /// This dictionary can be updated by deriving classes by overriding <see cref="SetApiResourceModelsCore"/>.
        /// <see cref="SetApiResourceModelsCore"/> is called during the controller initialization or manually by calling <see cref="SetApiResourceModels"/>.
        ///
        /// By default this dictionary contains the following entries:
        /// - <see cref="JsonApiResourceGetMany"/>
        /// - <see cref="JsonApiResourceGet"/>
        /// They are all stored by their name.
        /// </summary>
        private readonly Dictionary<string, JsonApiResource> apiResourceModelsById =
            new Dictionary<string, JsonApiResource>
            {
                { nameof(JsonApiResourceGetMany), JsonApiResourceGetMany },
                { nameof(JsonApiResourceGet), JsonApiResourceGet }
            };


        /// <inheritdoc />
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            SetApiResourceModels();
        }

        public void SetApiResourceModels()
        {
            // Update apiResourceModelsById from deriving classes.
            SetApiResourceModelsCore(apiResourceModelsById);
        }

        /// <summary>
        /// Can be used by deriving classes to register <see cref="JsonApiResource"/> by their id.
        /// These can be used by <see cref="JsonApiAttribute"/> assignments to methods of this or deriving classes.
        ///
        /// ATTENTION: If you implement this method in a deriving class, make sure to call the base-implementation,
        /// so all deriving classes can register entries.
        /// </summary>
        /// <param name="apiResourceModelsById">The dictionary in which to register entries.</param>
        // ReSharper disable once ParameterHidesMember
        protected virtual void SetApiResourceModelsCore(Dictionary<string, JsonApiResource> apiResourceModelsById)
        {
        }
        /// <inheritdoc />
        public override JsonApiResource GetJsonApiResourceById(string id)
        {
            return apiResourceModelsById.TryGetValue(id, out var value) ? value : null;
        }

        /// <summary>
        /// Returns the service-node on which this Read-controller is operating.
        /// </summary>
        /// <param name="service">The service on which to operate. Not null.</param>
        /// <returns>A <see cref="IServiceReadNode{TSession,TModel,TId}"/> on which to operate.</returns>
        protected abstract IServiceReadNode<TSession, TModel, long> GetReadServiceNode(TService service);
        /// <summary>
        /// The title that is set during the export to Xlsx.
        /// </summary>
        protected abstract string XlsxTitle { get; }


        /// <summary>
        /// HTTP-verb: <c>GET</c>
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
        /// HTTP-body: empty
        ///
        /// Query-parameters:
        /// - Supports query (see <see cref="QueryInfo"/>)
        /// 
        /// Get a collection of records of the entity specified by the TModel type parameter.
        /// The JSON-Api resource that is used for serialization is defined by <see cref="JsonApiResourceGetMany"/>, which will return an instance of the TResource parameter.
        ///
        /// Queries the corresponding api service node to return all records that the current session is permitted to read and match the optionally passed query.
        /// Supports queries (see <see cref="QueryInfo"/>).
        ///
        /// Returns a query result (see <see cref="QueryResult{TModel}"/>), which means that in addition to the data itself, a record count and a page count is also returned as meta data.
        ///
        /// If the current session is not allowed to access a record in the result, no error is returned, but the record is not included in the result set.
        /// </summary>
        /// <returns>The query result.</returns>
        [HttpGet, Route(""), JsonApi(nameof(JsonApiResourceGetMany), true)]
        public async Task<QueryResult<TModel>> GetManyAsync()
        {
            return await GetReadServiceNode(Service).GetManyAsync(await GetSessionAsync(), QueryInfo);
        }

        /// <summary>
        /// HTTP-verb: <c>POST</c>
        /// 
        /// Route: <c>{controller}/export-to-xlsx</c>
        /// 
        /// Expected HTTP-headers:
        /// - <c>Content-Type: application/json</c>
        /// - <c>X-ApiKey: {session token}</c>
        /// 
        /// Recommended HTTP-headers:
        /// - <c>Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet</c>
        /// - <c>Accept-Encoding: gzip,deflate</c>
        /// 
        /// HTTP-body: <see cref="XlsxExport"/> as JSON
        /// 
        /// Query-parameters:
        /// - Supports query (see <see cref="QueryInfo"/>), paging and includes are ignored.
        /// 
        /// Get a collection of records of the entity specified by the TModel type parameter, stores the data into an Xlsx-file,
        /// where every record is a row and columns can be defined in the passed <see cref="XlsxExport"/>.
        /// 
        /// Queries the corresponding api service node to return all records that the current session is permitted to read and match the optionally passed query.
        /// Supports queries (see <see cref="QueryInfo"/>).
        ///
        /// The HTTP-response contains additional information:
        /// - <c>Content-Disposition</c> is set to <c>attachment; filename={filename}.xlsx</c>
        /// - <c>Content-Length</c> is set to the file size.
        ///
        /// If the current session is not allowed to access a record in the result, no error is returned, but the record is not included in the result set.
        /// </summary>
        /// <param name="export">Read from body. Defines the structure of the generated Xlsx-file.</param>
        /// <returns>The generated file.</returns>
        [HttpPost, Route("export-to-xlsx"), JsonApi(supportsQuery: true, returnsBinary: true)]
        public async Task<HttpResponseMessage> ExportToXlsxAsync([FromBody] XlsxExport export)
        {
            // ignore paging (set it to null), ignore includes (set it to null)
            QueryInfo = new QueryInfo(QueryInfo?.SortProperties, QueryInfo?.Filters);
            var queryResult = await GetManyAsync();
            return queryResult.Data.DeliverAsXlsxAsync(export, XlsxTitle);
        }

        /// <summary>
        /// HTTP-verb: <c>GET</c>
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
        /// HTTP-body: empty
        ///
        /// Query-parameters: empty
        /// 
        /// Returns the record of the entity specified by the TModel type parameter with the given id (passed in the route).
        /// The JSON-Api resource that is used for serialization is defined by <see cref="JsonApiResourceGet"/>, which will return an instance of the TResource parameter.
        ///
        /// Queries the corresponding api service node to return the requested record, if the current session is permitted to read it.
        ///
        /// If the current session is not allowed to access the primary record (defined by the passed id),
        /// an error is returned.
        /// </summary>
        /// <param name="id">Read from route. The id of the requested record.</param>
        /// <returns>The requested record.</returns>
        [HttpGet, Route("{id:long}"), JsonApi(nameof(JsonApiResourceGet))]
        public async Task<TModel> GetAsync(long id)
        {
            return await GetReadServiceNode(Service).GetByIdAsync(await GetSessionAsync(), id);
        }


        /// <summary>
        /// HTTP-verb: <c>GET</c>
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
        /// HTTP-body: empty
        /// 
        /// Query-parameters: empty
        /// 
        /// Returns the requested relation of the record of the entity specified by the TModel type parameter with the given id (passed in the route).
        /// The return value can be a single record or a collection of records depending on the type of relation (belongs-to vs to-many).
        /// The JSON-Api resource that is used for serialization is <see cref="IdModelResource"/>, which will return a type and and id only.
        /// 
        /// Queries the corresponding api service node to return the requested record, if the current session is permitted to read it,
        /// then queries the api service to return the relation, if the current session nis permitted to read it.
        /// 
        /// If the current session is not allowed to access the primary record (defined by the passed id),
        /// an error is returned.
        /// 
        /// If the relation is a single-result-relation (belongs-to), and the current session is not allowed to access the relation or the related record,
        /// an error is returned.
        /// 
        /// If the relation is a multi-result-relation (to-many), and the current session is not allowed to access the relation, an error is returned.
        /// If the relation is a multi-result-relation (to-many), and the current session is not allowed to access a related record, no error is returned, but the record is not included in the result set.
        /// </summary>
        /// <param name="id">Read from route. The id of the requested record.</param>
        /// <param name="relationName">The name of the relation.</param>
        /// <returns>The requested record.</returns>
        [HttpGet, Route("{id:long}/relationships/{relationName}"), JsonApi(typeof(IdModelResource))]
        public async Task<JsonApiOverridePrimaryType> GetRelationAsync(long id, string relationName)
        {
            var relation = CrudControllerResourceMetaData.Instance.ForModel<TModel>()(relationName)
                ?? throw new HttpResponseException(HttpStatusCode.NotFound);

            var primaryData = await GetReadServiceNode(Service).GetByIdAsync(await GetSessionAsync(), id);
            return new JsonApiOverridePrimaryType(relation.ReadIdModels(primaryData), relation.Type);
        }
    }
}