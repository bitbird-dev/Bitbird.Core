using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace Bitbird.Core.WebApi.Controllers
{
    /// <summary>
    /// Provides the same functionality as the standard route provider (<see cref="DefaultDirectRouteProvider"/>),
    /// except actions of inherited controllers are also mapped to routes.
    /// </summary>
    public class CustomDirectRouteProvider : DefaultDirectRouteProvider
    {
        /// <inheritdoc />
        protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>(true);
        }
    }
}