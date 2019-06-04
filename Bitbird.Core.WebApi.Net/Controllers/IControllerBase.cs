using System.Web.Http.Controllers;
using Bitbird.Core.WebApi.Benchmarking;
using Bitbird.Core.WebApi.JsonApi;

namespace Bitbird.Core.WebApi.Controllers
{
    public interface IControllerBase : IBenchmarkController, IJsonApiResourceController, IHttpController
    {
    }
}