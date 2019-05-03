using System;
using JetBrains.Annotations;

namespace Bitbird.Core.WebApi.Controllers
{
    public interface IReadControllerBase
    {
        [NotNull] Type ModelType { get; }
        [NotNull] Type ResourceType { get; }

        void SetApiResourceModels();
    }
}