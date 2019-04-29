using System.Web.Http.Filters;

namespace Bitbird.Core.WebApi.Net.JsonApi
{
    public class JsonApiErrorFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            context.Response = context.Exception.ToJsonApiErrorResponseMessage();
        }
    }
}