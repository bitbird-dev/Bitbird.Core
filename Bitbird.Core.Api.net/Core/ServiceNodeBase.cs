using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Core
{
    public abstract class ServiceNodeBase<TService>
    {
        [NotNull] public readonly TService Service;

        protected ServiceNodeBase([NotNull] TService service)
        {
            Service = service;
        }
    }
}