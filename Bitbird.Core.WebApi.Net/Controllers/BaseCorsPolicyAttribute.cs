using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace Bitbird.Core.WebApi.Net.Controllers
{
    /// <summary>
    /// Allows CORS for specified controllers/actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class BaseCorsPolicyAttribute : Attribute, ICorsPolicyProvider
    {
        private readonly CorsPolicy policy;

        public BaseCorsPolicyAttribute()
        {
            // Create a CORS policy.
            policy = new CorsPolicy
            {
                AllowAnyMethod = true,
                AllowAnyHeader = true,
                AllowAnyOrigin = true
            };

            policy.Methods.Add("POST");
            policy.Methods.Add("GET");
            policy.Methods.Add("PUT");
            policy.Methods.Add("PATCH");
            policy.Methods.Add("DELETE");
            policy.Methods.Add("OPTIONS");

            // ConcurrentEnqueue allowed origins.
            policy.Origins.Add("http://localhost:4200");
            policy.Origins.Add("*");
        }

        /// <inheritdoc />
        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(policy);
        }
    }
}