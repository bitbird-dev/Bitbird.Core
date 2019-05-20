using System;
using JetBrains.Annotations;

namespace Bitbird.Core.WebApi.Controllers
{
    public interface ICrudControllerBase : IReadControllerBase
    {
        bool CanCreate { get; }
        bool CanDelete { get; }
        bool CanUpdate { get; }
    }
}